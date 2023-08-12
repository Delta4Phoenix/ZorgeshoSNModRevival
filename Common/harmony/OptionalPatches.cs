using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using HarmonyLib;

namespace Common.Harmony
{
	using Reflection;

	[AttributeUsage(AttributeTargets.Class)]
	public class OptionalPatchAttribute: Attribute {}

	static class OptionalPatches // dynamic patching/unpatching
	{
		static List<Type> optionalPatches = null;

		public static void Update()
		{
			optionalPatches ??= ReflectionHelper.definedTypes.Where(type => type.CheckAttr<OptionalPatchAttribute>()).ToList();

			using (Debug.DProfiler("Update optional patches"))
				optionalPatches.ForEach(type => Update(type));
		}

		// calls setEnabled with result of 'prepare' method
		public static void Update(Type patchType)
		{
			using var _ = Debug.DProfiler($"Update optional patch: {patchType}", allowNested: false);

			var prepare = patchType.method("prepare", ReflectionHelper.bfAll | BindingFlags.IgnoreCase).wrap();
			Debug.assert(prepare, $"OptionalPatches.update: 'prepare' method is absent for {patchType}");

			if (prepare)
				SetEnabled(patchType, prepare.invoke<bool>());
		}

		public static void SetEnabled(Type patchType, bool enabled)
		{
			if (patchType.getAttr<HarmonyPatch>() is HarmonyPatch patch) // regular harmony patch
				SetEnabled(patchType, patch, enabled);
			else if (patchType.CheckAttr<PatchClassAttribute>()) // optional patch class
				HarmonyHelper.patch(patchType, enabled);
		}

		static void SetEnabled(Type patchType, HarmonyPatch patch, bool enabled)
		{																									$"OptionalPatches: setEnabled {patchType} => {enabled}".logDbg();
			var method = patch.info.GetTargetMethod();

			if (method == null)
			{
				"OptionalPatches: method is null!".logError();
				return;
			}

			var prefix = patchType.method("Prefix");
			var postfix = patchType.method("Postfix");
			var transpiler = patchType.method("Transpiler");

			var patches = HarmonyHelper.GetPatchInfo(method);

			bool prefixActive = patches.IsPatchedBy(prefix);
			bool postfixActive = patches.IsPatchedBy(postfix);
			bool transpilerActive = patches.IsPatchedBy(transpiler);

			if (enabled)
			{
				if (!prefixActive && !postfixActive && !transpilerActive)
					HarmonyHelper.Patch(method, prefix, postfix, transpiler);
			}
			else
			{
				// need to check if this is actual patches to avoid unnecessary updates in harmony (with transpilers especially)
				if (prefixActive)	  HarmonyHelper.Unpatch(method, prefix);
				if (postfixActive)	  HarmonyHelper.Unpatch(method, postfix);
				if (transpilerActive) HarmonyHelper.Unpatch(method, transpiler);
			}
		}
	}
}