using Common;
using Common.Harmony;

namespace RenameBeacons
{
	public static class Main
	{
		public static void patch()
		{
			HarmonyHelper.PatchAll();
			LanguageHelper.Init();
		}
	}

	class L10n: LanguageHelper
	{
		public static readonly string ids_name = "Name";
		public static readonly string ids_rename = "rename";
	}
}