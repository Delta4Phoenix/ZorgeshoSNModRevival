using Common;
using Common.Harmony;
using Common.Crafting;
using BepInEx;

namespace GravTrapImproved
{
	[BepInPlugin(GUID, MODNAME, VERSION)]
	public class Main:BaseUnityPlugin
	{
		#region[Declarations]

		public const string
			MODNAME = "GravTrapImprovedUpdated",
			AUTHOR = "delta4phoenix",
			GUID = AUTHOR + "." + MODNAME,
			VERSION = "1.0.0.0";

		#endregion

		internal static readonly ModConfig config = Mod.Init<ModConfig>();
		internal static readonly TypesConfig typesConfig = Mod.LoadConfig<TypesConfig>("types_config.json", Common.Configuration.Config.LoadOptions.ReadOnly | Common.Configuration.Config.LoadOptions.ProcessAttributes);

		public void Patch()
		{
			LanguageHelper.Init();
			PersistentConsoleCommands.register<ConsoleCommands>();

			HarmonyHelper.PatchAll(true);
			CraftHelper.patchAll();

			GravTrapObjectsType.Init(typesConfig);
		}
	}
}