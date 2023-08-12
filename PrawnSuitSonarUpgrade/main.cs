using Common;
using Common.Harmony;
using Common.Crafting;

namespace PrawnSuitSonarUpgrade
{
	public static class Main
	{
		public static void Patch()
		{
			Mod.Init();

			HarmonyHelper.PatchAll();
			CraftHelper.patchAll();
		}
	}
}