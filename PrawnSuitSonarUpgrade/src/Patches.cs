using HarmonyLib;
using Common;

namespace PrawnSuitSonarUpgrade
{
	[HarmonyPatch(typeof(Vehicle), "OnUpgradeModuleChange")]
	static class Vehicle_OnUpgradeModuleChange_Patch
	{
		static void Postfix(Vehicle __instance, TechType techType, bool added)
		{
			if (techType != PrawnSonarModule.TechType || __instance.GetType() != typeof(Exosuit))
				return;

			var sonarControl = ObjectAndComponentExtensions.EnsureComponent<PrawnSonarControl>(__instance.gameObject);

			if (added)
				sonarControl.enabled = true;
			else if (__instance.modules.GetCount(PrawnSonarModule.TechType) == 0)
				sonarControl.enabled = false;
		}
	}

	[HarmonyPatch(typeof(Vehicle), "OnUpgradeModuleToggle")]
	static class Vehicle_OnUpgradeModuleToggle_Patch
	{
		static void Postfix(Vehicle __instance, int slotID, bool active)
		{
			if (__instance.GetSlotBinding(slotID) == PrawnSonarModule.TechType)
				__instance.gameObject.GetComponent<PrawnSonarControl>()?.OnToggle(slotID, active);
		}
	}

	[HarmonyPatch(typeof(Exosuit), "OnPilotModeBegin")]
	static class Exosuit_OnPilotModeBegin_Patch
	{
		static void Postfix(Exosuit __instance) => __instance.GetComponent<PrawnSonarControl>()?.SetPlayerInside(true);
	}

	[HarmonyPatch(typeof(Exosuit), "OnPilotModeEnd")]
	static class Exosuit_OnPilotModeEnd_Patch
	{
		static void Postfix(Exosuit __instance) => __instance.GetComponent<PrawnSonarControl>()?.SetPlayerInside(false);
	}
}