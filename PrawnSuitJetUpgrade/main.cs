using Common;
using Common.Harmony;
using Common.Crafting;

namespace PrawnSuitJetUpgrade
{
	public static class Main
	{
		internal static readonly ModConfig config = Mod.init<ModConfig>();

		public static void patch()
		{
			LanguageHelper.Init();

			HarmonyHelper.PatchAll();
			CraftHelper.patchAll();
		}
	}
}