using Common;
using Common.Harmony;
using Common.Crafting;
using Common.Configuration;

namespace DebrisRecycling
{
	public static class Main
	{
		internal const string prefabsConfigName = "prefabs_config.json";

		internal static readonly ModConfig config = Mod.init<ModConfig>();

		public static void patch()
		{
			HarmonyHelper.PatchAll(true);
			CraftHelper.patchAll();

			LanguageHelper.Init(); // after CraftHelper

			DebrisPatcher.init(Mod.LoadConfig<PrefabsConfig>(prefabsConfigName, Config.LoadOptions.ProcessAttributes));
		}
	}
}