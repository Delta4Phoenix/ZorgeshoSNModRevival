﻿using Common;
using Common.Harmony;

namespace UITweaks
{
	public static class Main
	{
		internal static readonly ModConfig config = Mod.init<ModConfig>();

		public static void patch()
		{
			HarmonyHelper.PatchAll(true);
			LanguageHelper.Init();

			PersistentConsoleCommands.register<ConsoleCommands>();
		}
	}
}