﻿using Common;
using Common.Harmony;
using Common.Crafting;

namespace OxygenRefill
{
	public static class Main
	{
		internal static readonly ModConfig config = Mod.init<ModConfig>();

		public static void patch()
		{
			HarmonyHelper.PatchAll(true);
			LanguageHelper.Init();
			CraftHelper.patchAll();

			PersistentConsoleCommands.register<ConsoleCommands>();
		}
	}
}