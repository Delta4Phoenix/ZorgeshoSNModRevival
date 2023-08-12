﻿using System;
using System.Reflection.Emit;
using System.Collections.Generic;

using HarmonyLib;

using Common;
using Common.Harmony;

namespace SeamothStorageSlots
{
	[PatchClass]
	static class SeamothStorageInputPatches
	{
		static bool Prepare() => Main.config.SlotsOffset > 0;

		// substitute call for 'this.seamoth.GetStorageInSlot()'
		[HarmonyTranspiler]
		[HarmonyPatch(typeof(SeamothStorageInput), "OpenPDA")]
		[HarmonyPatch(typeof(SeamothStorageInput), "OnHandClick")]
		static IEnumerable<CodeInstruction> SeamothStorageInput_Transpiler(IEnumerable<CodeInstruction> cins)
		{
			static ItemsContainer _getStorageInSlot(Vehicle vehicle, int slotID, TechType techType) =>
				 vehicle.GetStorageInSlot(slotID, techType) ?? vehicle.GetStorageInSlot(slotID + Main.config.SlotsOffset, techType);

			return CIHelper.ciReplace(cins, ci => ci.isOp(OpCodes.Callvirt),
				CIHelper.emitCall<Func<Vehicle, int, TechType, ItemsContainer>>(_getStorageInSlot));
		}
	}


	[HarmonyPatch(typeof(Equipment), "AllowedToAdd")]
	static class Equipment_AllowedToAdd_Patch
	{
		static bool Prepare() => Main.config.SlotsOffset > 0;

		[HarmonyPriority(Priority.HigherThanNormal)]
		static bool Prefix(Equipment __instance, string slot, Pickupable pickupable, bool verbose, ref bool __result)
		{
			TechType techType = pickupable.GetTechType();

			if (techType != TechType.VehicleStorageModule || !slot.startsWith("SeamothModule"))
				return true;

			SeaMoth seamoth = __instance.owner.GetComponent<SeaMoth>();
			if (seamoth == null)
				return true;

			int slotID = int.Parse(slot.Substring(13)) - 1;

			if (slotID > 3 && (slotID < Main.config.SlotsOffset || slotID > Main.config.SlotsOffset + 3))
				return true;

			// HACK: trying to swap one storage to another while drag, silently refusing because of ui problems
			if (seamoth.GetSlotItem(slotID)?.item.GetTechType() == TechType.VehicleStorageModule)
			{
				__result = false;
				return false;
			}

			__result = !seamoth.storageInputs[slotID % Main.config.SlotsOffset].state; //already active

			if (!__result && verbose)
				$"Storage module is already in slot {(slotID < 4? slotID + Main.config.SlotsOffset: slotID - Main.config.SlotsOffset) + 1}".onScreen();

			return false;
		}
	}


	[HarmonyPatch(typeof(Vehicle), "OnUpgradeModuleChange")]
	static class Vehicle_OnUpgradeModuleChange_Patch
	{
		static bool Prepare() => Main.config.SlotsOffset > 0;

		static void Postfix(Vehicle __instance, int slotID, TechType techType, bool added)
		{
			if (__instance is not SeaMoth seamoth)
				return;

			//any non-storage module added in seamoth slots 1-4 disables corresponding storage, checking if we need to enable it again
			if (slotID < 4 && techType != TechType.VehicleStorageModule)
			{
				if (__instance.GetSlotItem(slotID + Main.config.SlotsOffset)?.item.GetTechType() == TechType.VehicleStorageModule)
					seamoth.storageInputs[slotID].SetEnabled(true);
			}
			else // if we adding/removing storage module in linked slots, we need to activate/deactivate corresponing storage unit
			if (slotID >= Main.config.SlotsOffset && slotID < Main.config.SlotsOffset + 4 && techType == TechType.VehicleStorageModule)
			{
				seamoth.storageInputs[slotID - Main.config.SlotsOffset].SetEnabled(added);
			}
		}
	}
}