using HarmonyLib;

namespace BerryLoaderNS
{
	public static partial class Patches
	{
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
	}
}