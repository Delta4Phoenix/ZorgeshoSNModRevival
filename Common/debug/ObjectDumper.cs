using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Collections.Generic;

using UnityEngine;

namespace Common
{
	using Reflection;

	static partial class Debug
	{
#if DEBUG
		const string pathForDumps = "c:/projects/subnautica/dumps/";
#endif
		public static string DumpGameObject(GameObject go, bool dumpProperties = true, bool dumpFields = false) =>
			ObjectDumper.Dump(go, dumpProperties, dumpFields);

		public static void Dump(this GameObject go, string filename = null, int dumpParent = 0)
		{
			while (dumpParent-- > 0 && go.GetParent())
				go = go.GetParent();

			filename ??= go.name.Replace("(Clone)", "").ToLower();
#if DEBUG
			Paths.EnsurePath(pathForDumps);
			filename = pathForDumps + filename;
#endif
			ObjectDumper.Dump(go, true, true).SaveToFile(filename + ".yml");
		}


		static class ObjectDumper
		{
			const string indentStep = "    ";

			static readonly StringBuilder output = new();
			static readonly Regex sanitizer = new ("[\\r\\n\\t\0]", RegexOptions.Compiled);

			static readonly Type[] dumpTypes = // dump these non-Component types too
			{
				typeof(Sprite),
				typeof(Texture2D),
				typeof(Material),
				Type.GetType("UnityEngine.UIVertex, UnityEngine.TextRenderingModule")
			};

			static bool dumpProperties;
			static bool dumpFields;

			public static string Dump(GameObject go, bool dumpProperties, bool dumpFields)
			{
				output.Clear();
				ObjectDumper.dumpProperties = dumpProperties;
				ObjectDumper.dumpFields = dumpFields;

				Dump(go, "");

				return output.ToString();
			}

			static void Dump(GameObject go, string indent)
			{
				output.AppendLine($"{indent}gameobject: {go.name} activeS/activeH:{go.activeSelf}/{go.activeInHierarchy}");

				foreach (var cmp in go.GetComponents<Component>())
					Dump(cmp, indent + indentStep, "component");

				foreach (Transform child in go.transform)
					Dump(child.gameObject, indent + indentStep);
			}

			static void Dump(object obj, string indent, string title = null)
			{
				if (obj == null) // it happens sometimes for some reason
				{
					output.AppendLine($"{indent}{title ?? ""}: NULL");
					return;
				}

				Type objType = obj.GetType();

				if (title != null)
					output.AppendLine($"{indent}{title}: {objType}");

				try
				{
					var bf = ReflectionHelper.bfAll ^ BindingFlags.Static;

					if (dumpProperties)
					{
						var properties = objType.properties(bf).ToList();
						if (properties.Count > 0)
						{
							_sort(properties);
							output.AppendLine($"{indent}{indentStep}PROPERTIES:");

							foreach (var prop in properties)
								if (prop.GetGetMethod() != null)
									_dumpValue(prop.Name, prop.PropertyType, prop.GetValue(obj, null), indent);
						}
					}

					if (dumpFields)
					{
						var fields = objType.fields(bf).ToList();
						if (fields.Count > 0)
						{
							_sort(fields);
							output.AppendLine($"{indent}{indentStep}FIELDS:");

							foreach (var field in fields)
								_dumpValue(field.Name, field.FieldType, field.GetValue(obj), indent);
						}
					}
				}
				catch (Exception e) { Log.msg(e); }

				static void _sort<T>(List<T> list) where T: MemberInfo => list.Sort((m1, m2) => m1.Name.CompareTo(m2.Name));

				static void _dumpValue(string name, Type type, object value, string indent)
				{
					string sanitized = value != null? sanitizer.Replace(value.ToString(), " ").Trim(): "";
					output.AppendLine($"{indent}{indentStep}{name} [{type.Name}]: \"{sanitized}\"");

					if (value == null)
						return;

					if (dumpTypes.Contains(type))
						Dump(value, indent + indentStep);

					if (type.IsArray)
					{
						var array = value as Array;

						for (int i = 0; i < array.Length; i++)
							_dumpValue($"[{i}]", type.GetElementType(), array.GetValue(i), indent + indentStep);
					}
				}
			}
		}
	}
}