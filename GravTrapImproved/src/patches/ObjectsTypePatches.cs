﻿using HarmonyLib;
using UnityEngine;

using Common;
using Common.Harmony;

namespace GravTrapImproved
{
	[PatchClass]
	static class ObjectsTypePatches
	{
		[HarmonyPostfix, HarmonyPatch(typeof(Gravsphere), "Start")]
		static void Gravsphere_Start_Postfix(Gravsphere __instance)
		{
			ObjectAndComponentExtensions.EnsureComponent<GravTrapObjectsType>(__instance.gameObject);
		}

		[HarmonyPostfix, HarmonyPatch(typeof(Gravsphere), "AddAttractable")]
		static void Gravsphere_AddAttractable_Postfix(Gravsphere __instance, Rigidbody r)
		{																										$"Gravsphere.AddAttractable: {r.gameObject.name} mass: {r.mass}".logDbg();
			__instance.GetComponent<GravTrapObjectsType>().HandleAttracted(r.gameObject, true);
		}

		[HarmonyPostfix, HarmonyPatch(typeof(Gravsphere), "DestroyEffect")]
		static void Gravsphere_DestroyEffect_Postfix(Gravsphere __instance, int index)
		{
			var rigidBody = __instance.attractableList[index];
			if (rigidBody)
				__instance.GetComponent<GravTrapObjectsType>().HandleAttracted(rigidBody.gameObject, false);
		}

		[HarmonyPrefix, HarmonyPatch(typeof(Gravsphere), "IsValidTarget")]
		static bool Gravsphere_IsValidTarget_Prefix(Gravsphere __instance, GameObject obj, ref bool __result)
		{
			__result = __instance.GetComponent<GravTrapObjectsType>().IsValidTarget(obj);
			return false;
		}
	}
}