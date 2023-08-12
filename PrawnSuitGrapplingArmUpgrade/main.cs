using Common;
using Common.Harmony;
using Common.Crafting;

namespace PrawnSuitGrapplingArmUpgrade
{
	public static class Main
	{
		internal static readonly ModConfig config = Mod.init<ModConfig>();

		public static void Patch()
		{
			HarmonyHelper.PatchAll();
			CraftHelper.patchAll();
		}
	}
}