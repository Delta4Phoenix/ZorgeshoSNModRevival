#define DISABLE_VERSION_CHECK_IN_DEVBUILD

using System;
using System.IO;
using System.Threading;
using System.Reflection;
using System.Globalization;

using UnityEngine;

namespace Common
{
	using Utils;
	using Reflection;

	static partial class Mod
	{
		public static bool IsShuttingDown { get; private set; }
		class ShutdownListener: MonoBehaviour { void OnApplicationQuit() { IsShuttingDown = true; "Shutting down".logDbg(); } }

		const string tmpFileName = "run the game to generate configs"; // name is also in the post-build.bat
		const string updateMessage = "An update is available! (current version is v<color=orange>{0}</color>, new version is v<color=orange>{1}</color>)";

		public static readonly string id = Assembly.GetExecutingAssembly().GetName().Name; // not using mod.json for ID
		public static string Name { get { Init(); return _name; } }
		static string _name;

		static bool inited;

		// supposed to be called before any other mod's code
		public static void Init()
		{
			if (inited || !(inited = true))
				return;

			UnityHelper.CreatePersistentGameObject<ShutdownListener>($"{id}.ShutdownListener");

			// may be overkill to make it for all mods and from the start
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

			try { File.Delete(Paths.modRootPath + tmpFileName); }
			catch (UnauthorizedAccessException) { }



			"Mod inited".logDbg();
		}
	}
}