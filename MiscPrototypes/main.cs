using Common;
using Common.Harmony;
using Common.Crafting;

namespace MiscPrototypes
{
	public static partial class Main
	{
		internal static readonly ModConfig config = Mod.init<ModConfig>();

		static partial void InitTestConfig();

		public static void Patch()
		{
			InitTestConfig();

			HarmonyHelper.PatchAll(true);
			LanguageHelper.Init();
			CraftHelper.patchAll();

			PersistentConsoleCommands.register<TestConsoleCommands>();
		}
	}
}