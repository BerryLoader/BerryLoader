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

		// dumbest patch in the entire codebase. should be replaced with proper localization
		[HarmonyPatch(typeof(Subprint), "StatusName", MethodType.Getter)]
		[HarmonyPrefix]
		public static bool StatusOverride(Subprint __instance, ref string __result)
		{
			if (!SokLoc.instance.CurrentLocSet.ContainsTerm(__instance.StatusTerm))
			{
				__result = __instance.StatusTerm;
				return false;
			}
			return true;
		}
	}
}