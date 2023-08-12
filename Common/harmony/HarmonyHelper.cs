﻿//#define VALIDATE_PATCHES

using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using HarmonyLib;

namespace Common.Harmony
{
	using Reflection;
	using Harmony = HarmonyLib.Harmony;

	static partial class HarmonyHelper
	{
		public static Harmony HarmonyInstance { get; } = new (Mod.id);

		public static void PatchAll(bool searchForPatchClasses = false)
		{
			try
			{
#if VALIDATE_PATCHES
				PatchesValidator.validate();
#endif
				using var _ = Debug.DProfiler("HarmonyHelper.patchAll");

				HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());

				if (searchForPatchClasses)
					ReflectionHelper.definedTypes.Where(type => type.CheckAttr<PatchClassAttribute>()).ForEach(type => patch(type));
			}
			catch (Exception e)
			{
				Log.msg(e, "HarmonyHelper.patchAll"); // so the exception will be in the mod's log
				throw e;
			}
		}

		public static void Patch(MethodBase original, MethodInfo prefix = null, MethodInfo postfix = null, MethodInfo transpiler = null)
		{
			static string _dbg(string type, MethodInfo method) => method != null? $"{type}:{method} ": "";
			Debug.assert(original != null);										$"HarmonyHelper.patch: patching '{original.fullName()}' with {_dbg("prefix", prefix)}{_dbg("postfix", postfix)}{_dbg("transpiler", transpiler)}".logDbg();

			if (original == null)
			{
				"HarmonyHelper.patch: target method is null".logError();
				return;
			}

			try
			{
				static HarmonyMethod _harmonyMethod(MethodInfo method) => method == null? null: new HarmonyMethod(method);

				using (Debug.DProfiler($"HarmonyHelper.patch '{original.fullName()}'"))
					HarmonyInstance.Patch(original, _harmonyMethod(prefix), _harmonyMethod(postfix), _harmonyMethod(transpiler));
			}
			catch (Exception e)
			{
				Log.msg(e, "HarmonyHelper.patch");
				throw e;
			}
		}

		public static void Unpatch(MethodBase original, MethodInfo patch) => HarmonyInstance.Unpatch(original, patch);

		public static Patches GetPatchInfo(MethodBase method) => Harmony.GetPatchInfo(method);

		// checkByName - comparing patches by method's names (for use with shared projects)
		public static bool IsPatchedBy(MethodBase original, MethodBase patch, bool checkByName = false)
		{
			Debug.assert(original != null && patch != null, $"'{original}' '{patch}'");
			return GetPatchInfo(original).IsPatchedBy(patch, checkByName);
		}
	}

	static partial class HarmonyHelper
	{
		// helper for patching iterator methods
		public class IteratorWrapper
		{
			readonly object obj;
			FieldInfo _fiState, _fiCurrent;

			FieldInfo FiState => _fiState ??= obj.GetType().field("<>1__state");
			FieldInfo FiCurrent => _fiCurrent ??= obj.GetType().field("<>2__current");

			public IteratorWrapper(object obj) => this.obj = obj;

			public int State
			{
				get => FiState.GetValue(obj).cast<int>();
				set => FiState.SetValue(obj, value);
			}

			public object Current
			{
				get => FiCurrent.GetValue(obj);
				set => FiCurrent.SetValue(obj, value);
			}
		}

		// helper for lazy patching
		public class LazyPatcher
		{
			bool patched = false;
			readonly Type target;

			// 'autopatch' is true: patch target class on first access
			// 'target' is null: use parent class as target
			public LazyPatcher(bool autopatch = false, Type target = null)
			{
				this.target = target ?? ReflectionHelper.GetCallingType();							$"HarmonyHelper.LazyPatcher created: target = {this.target}".logDbg();

				if (autopatch)
				{
					// we don't want any exceptions in constructor
					// in case of failed patching we will not try it again
					try { Patch(); }
					catch (Exception e) { Log.msg(e); }
				}
			}

			public void Patch()
			{
				if (!patched && (patched = true))
					HarmonyHelper.patch(target);
			}
		}
	}

	static partial class HarmonyExtensions
	{
		public static MethodBase GetTargetMethod(this HarmonyMethod harmonyMethod)
		{
			if (harmonyMethod.methodName != null)
				return harmonyMethod.declaringType?.method(harmonyMethod.methodName, harmonyMethod.argumentTypes);

			if (harmonyMethod.methodType == MethodType.Constructor)
				return harmonyMethod.declaringType?.GetConstructor(ReflectionHelper.bfAll, null, harmonyMethod.argumentTypes, null);

			return null;
		}

		public static bool IsPatchedBy(this Patches patches, MethodBase patch, bool checkByName = false)
		{
			if (patches == null)
				return false;

			string patchFullName = checkByName? patch.fullName(): null;

			bool _contains(IList<Patch> list) => list.Count > 0 &&
				list.Any(p => (checkByName && p.PatchMethod?.fullName() == patchFullName) || (!checkByName && Equals(p.PatchMethod, patch)));

			return _contains(patches.Prefixes) || _contains(patches.Postfixes) || _contains(patches.Transpilers);
		}
	}
}