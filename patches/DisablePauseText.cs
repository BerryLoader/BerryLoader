using HarmonyLib;

namespace BerryLoaderNS
{
	public static partial class Patches
	{

		[HarmonyPatch(typeof(GameScreen), "Update")]
		[HarmonyPostfix]
		static void DisablePauseText(GameScreen __instance)
		{
			if (BerryLoader.configDisablePauseText.Value == true)
				__instance.PausedText.text = "";
		}
	}
}