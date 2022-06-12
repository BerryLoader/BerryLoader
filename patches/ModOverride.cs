using HarmonyLib;

namespace BerryLoaderNS
{
	public static partial class Patches
	{
		// TODO: implement translation system here!
		[HarmonyPatch(typeof(Boosterpack), "Name", MethodType.Getter)]
		[HarmonyPatch(typeof(CardData), "Name", MethodType.Getter)]
		[HarmonyPrefix]
		static bool NameOverride(CardData __instance, ref string __result)
		{
			var ov = __instance.GetComponent<ModOverride>();
			if (ov is not null)
			{
				__result = ov.Name;
				return false;
			}
			return true;
		}

		[HarmonyPatch(typeof(CardData), "Description", MethodType.Getter)]
		[HarmonyPrefix]
		static bool DescriptionOverride(CardData __instance, ref string __result)
		{
			var ov = __instance.GetComponent<ModOverride>();
			if (ov is not null)
			{
				__result = ov.Description;
				return false;
			}
			return true;
		}

		// doing a postfix patch here because i cba to get this working properly yet
		/*[HarmonyPatch(typeof(Subprint), "StatusName", MethodType.Getter)]
		[HarmonyPostfix]
		static void StatusOverride(Subprint __instance, ref string __result)
		{
			BerryLoader.L.LogInfo(__result);
		}*/
	}
}