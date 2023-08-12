using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Sprite = Atlas.Sprite;

namespace Common
{
	static partial class StringExtensions
	{
		public static string OnScreen(this string s)
		{
			if (!GameUtils.IsLoadingState && Time.timeScale != 0f)
				ErrorMessage.AddDebug(s);

			return s;
		}

		// messages with the same prefix will stay in the same message slot
		public static string OnScreen(this string s, string prefix) { Debug.AddMessage(s, prefix); return s; }

		public static void OnScreen(this List<string> list, string msg = "", int maxCount = 30)
		{
			var listToPrint = list.Count > maxCount? list.GetRange(0, maxCount): list;
			listToPrint.ForEach(s => ErrorMessage.AddDebug(msg + s));
		}

		public static string OnScreen(this List<string> list, string prefix)
		{
			StringBuilder sb = new();
			list.ForEach(line => sb.AppendLine(line));
			return sb.ToString().OnScreen(prefix);
		}
	}

	static class Strings
	{
		public static class Mouse
		{
			static string Str(int utf32) => $"<color=#ADF8FFFF>{char.ConvertFromUtf32(utf32)}</color>";

			public static readonly string rightButton	= Str(57404);
			public static readonly string middleButton	= Str(57405);
			public static readonly string scrollUp		= Str(57406);
			public static readonly string scrollDown	= Str(57407);
		}
	}

	static partial class SpriteHelper // extended in other Common projects
	{
		public static Sprite GetSprite(object spriteID)
		{
			$"TechSpriteHelper.getSprite({spriteID.GetType()}) is not implemented!".logError();
			return SpriteManager.defaultSprite;
		}
	}

	static class GameUtils
	{
		// can't use vanilla GetVehicle in OnPlayerModeChange after 06.11 update :(
		public static Vehicle GetVehicle(this Player player) => player? player.GetComponentInParent<Vehicle>(): null; // don't use null-conditional here

		public static TechType GetHeldToolType() => Inventory.main?.GetHeld()?.GetTechType() ?? TechType.None;

		public static bool IsLoadingState =>
			WaitScreen.IsWaiting == true;

		// use that when needed (Time.time -> PDA.time in BZ)
		public static float Time =>
			PDA.time;
		public static void ClearScreenMessages() => // expire all messages except QMM main menu messages
			ErrorMessage.main?.messages.Where(m => m.timeEnd - Time < 1e3f).ForEach(m => m.timeEnd = Time - 1f);

		public static GameObject GetTarget(float maxDistance)
		{
			Targeting.GetTarget(Player.main.gameObject, maxDistance, out GameObject result, out _);
			return result;
		}

		public static void SetText(this HandReticle hand, string textUse = null, string textUseSubscript = null, string textHand = null, string textHandSubscript = null)
		{
			if (textUse != null)			hand.textUse = textUse;
			if (textHand != null)			hand.textHand = textHand;
			if (textUseSubscript != null)	hand.textUseSubscript = textUseSubscript;
			if (textHandSubscript != null)	hand.textHandSubscript = textHandSubscript;
		}

		// findNearest* methods are for use in non-performance critical code
		public static C FindNearestToCam<C>(Predicate<C> condition = null) where C: Component =>
			UnityHelper.FindNearest(LargeWorldStreamer.main?.cachedCameraPosition, out _, condition);

		public static C FindNearestToPlayer<C>(Predicate<C> condition = null) where C: Component =>
			UnityHelper.FindNearest(Player.main?.transform.position, out _, condition);

		public static C FindNearestToPlayer<C>(out float distance, Predicate<C> condition = null) where C: Component =>
			UnityHelper.FindNearest(Player.main?.transform.position, out distance, condition);
	}
}