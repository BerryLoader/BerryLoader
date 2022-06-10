using HarmonyLib;
using System;
using System.Linq;

namespace BerryLoaderNS
{
	public static partial class Patches
	{
		[HarmonyPatch(typeof(Boosterpack), "GetSummaryFromAllCards")]
		[HarmonyPostfix]
		public static void BPGSFAC(ref string __result)
		{
			if (BerryLoader.configCompactTooltips.Value == true)
			{
				var list = __result.Split('•').Select(str => str.Trim());
				__result = $"{list.First()} {String.Join(", ", list.Skip(1))}";
			}
		}

		[HarmonyPatch(typeof(BuyBoosterBox), "GetTooltipText")]
		[HarmonyPostfix]
		public static void BBBGTT(BuyBoosterBox __instance, ref string __result)
		{
			if (BerryLoader.configCompactTooltips.Value == true)
			{
				var sp = __result.Split('\n');
				if (sp.Length > 1)
					__result = sp[2];
			}
		}

		[HarmonyPatch(typeof(Combatable), "GetCombatDescription")]
		[HarmonyPrefix]
		public static bool CGCD(Combatable __instance, ref string __result)
		{
			if (BerryLoader.configCompactTooltips.Value == true)
			{
				__result = $"S: {GameCanvas.FormatTime(__instance.AttackTime)} | C: {__instance.HitChance}% | D: {__instance.MinDamage}-{__instance.MaxDamage}";
				return false;
			}
			return true;
		}
	}
}