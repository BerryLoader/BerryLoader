using HarmonyLib;
using System;
using System.Collections.Generic;

namespace BerryLoaderNS
{
	public static class InteractionAPI
	{
		public delegate void CanHaveCardDataHandler(CardData instance, CardData otherCard, ref bool? result);

		public static Dictionary<string, List<CanHaveCardDataHandler>> Interactions = new Dictionary<string, List<CanHaveCardDataHandler>>();
		public static List<Type> CardDatas = new List<Type>();

		public static void CreateInteraction(string id, CanHaveCardDataHandler f)
		{
			if (!Interactions.ContainsKey(id))
				Interactions[id] = new List<CanHaveCardDataHandler>();
			Interactions[id].Add(f);
		}

		public static void Init()
		{
			BerryLoader.L.LogInfo(CardDatas.Count);
			var prefix = new HarmonyMethod(typeof(InteractionAPI), "CanHaveCardPatch");
			foreach (var t in CardDatas)
			{
				BerryLoader.L.LogInfo($"Patching CanHaveCard on {t}");
				var chcm = AccessTools.DeclaredMethod(t, "CanHaveCard");
				if (chcm != null)
					BerryLoader.HarmonyInstance.Patch(chcm, prefix);
			}
		}

		public static bool CanHaveCardPatch(CardData __instance, CardData otherCard, ref bool __result)
		{
			if (Interactions.ContainsKey(__instance.Id))
			{
				BerryLoader.L.LogInfo(Interactions[__instance.Id].Count);
				foreach (var f in Interactions[__instance.Id])
				{
					bool? res = null;
					f(__instance, otherCard, ref res);
					if (res != null)
					{
						__result = (bool)res;
						return false;
					}
				}
			}
			return true;
		}
	}
}