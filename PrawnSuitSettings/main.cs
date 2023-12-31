﻿using Common;
using Common.Harmony;

namespace PrawnSuitSettings
{
	public static class Main
	{
		internal static readonly ModConfig config = Mod.init<ModConfig>();

		public static void patch()
		{
			HarmonyHelper.PatchAll(true);

			if (config.armsEnergyUsage.enabled)
				ArmsEnergyUsage.refresh();
		}
	}
}