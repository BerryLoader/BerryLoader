using HarmonyLib;
using System.Reflection;

namespace BerryLoaderNS
{
	public static partial class Patches
	{
		// TODO: implement translation system for the entire override system
		[HarmonyPatch(typeof(Boosterpack), "Name", MethodType.Getter)]
		[HarmonyPatch(typeof(CardData), "Name", MethodType.Getter)]
		[HarmonyPrefix]
		static bool NameOverride(CardData __instance, ref string __result)
		{
			var ov = __instance.GetComponent<ModOverride>();
			if (ov is not null && !string.IsNullOrEmpty(ov.Name))
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
			string descriptionOverride = (string)typeof(CardData).GetField("descriptionOverride", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
			if (!string.IsNullOrEmpty(descriptionOverride))
			{
				__result = descriptionOverride;
				return false;
			}
			var ov = __instance.GetComponent<ModOverride>();
			if (ov is not null && !string.IsNullOrEmpty(ov.Name))
			{
				__result = ov.Description;
				return false;
			}
			return true;
		}
	}
}