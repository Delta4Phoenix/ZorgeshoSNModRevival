using System.Linq;
using System.Collections.Generic;

namespace Common.Configuration.Utils
{
	static class CfgVarBinder
	{
		class CfgVarCommands: PersistentConsoleCommands
		{
			public void Setcfgvar(string varName, string varValue)
			{																				$"setcfgvar: '{varName}' '{varValue}'".logDbg();
				SetVarValue(varName, varValue);
			}

			public void Getcfgvar(string varName)
			{																				$"getcfgvar: '{varName}'".logDbg();
				if (GetVarValue(varName) is object value)
					$"{varName} = {value}".OnScreen();
			}
		}

		static void Init() => PersistentConsoleCommands.register<CfgVarCommands>();

		static readonly UniqueIDs uniqueIDs = new();
		static readonly Dictionary<string, Config.Field> cfgFields = new();

		public static string[] GetVarNames() => cfgFields.Keys.ToArray();

		public static void AddField(Config.Field cfgField, string varNamespace = null)
		{																									$"CfgVarBinder: adding field {cfgField.id}".logDbg();
			Init();

			string varName = ((varNamespace.IsNullOrEmpty()? "": varNamespace + ".") + cfgField.id).ToLower();
			uniqueIDs.EnsureUniqueID(ref varName);

			cfgFields[varName] = cfgField;
		}

		static Config.Field GetField(string name)
		{
			return name != null && cfgFields.TryGetValue(name, out Config.Field cf)? cf: null;
		}

		static void SetVarValue(string name, string value)
		{
			if (GetField(name) is Config.Field cf)
				cf.value = value;
		}

		static object GetVarValue(string name)
		{
			return GetField(name)?.value;
		}
	}
}