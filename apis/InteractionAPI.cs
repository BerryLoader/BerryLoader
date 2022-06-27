using HarmonyLib;
using System;
using System.Collections.Generic;

namespace BerryLoaderNS
{
	public static class InteractionAPI
	{
		public delegate void CanHaveCardDataHandler(CardData instance, CardData otherCard, ref bool? result);

		public static Dictionary<string, List<CanHaveCardDataHandler>> IdInteractions = new Dictionary<string, List<CanHaveCardDataHandler>>();
		public static Dictionary<CardType, List<CanHaveCardDataHandler>> CardTypeInteractions = new Dictionary<CardType, List<CanHaveCardDataHandler>>();
		public static List<Type> CardDatas = new List<Type>();

		public static void CreateIdInteraction(string id, CanHaveCardDataHandler f)
		{
			AddInteraction<string>(IdInteractions, id, f);
		}

		public static void CreateCardTypeInteraction(CardType t, CanHaveCardDataHandler f)
		{
			AddInteraction<CardType>(CardTypeInteractions, t, f);
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
			var funcs = new List<CanHaveCardDataHandler>();
			if (IdInteractions.ContainsKey(__instance.Id))
				funcs.AddRange(IdInteractions[__instance.Id]);
			if (CardTypeInteractions.ContainsKey(__instance.MyCardType))
				funcs.AddRange(CardTypeInteractions[__instance.MyCardType]);

			foreach (var f in funcs)
			{
				bool? res = null;
				f(__instance, otherCard, ref res);
				if (res != null)
				{
					__result = (bool)res;
					return false;
				}
			}
			return true;
		}

		private static void AddInteraction<T>(Dictionary<T, List<CanHaveCardDataHandler>> dict, T key, CanHaveCardDataHandler f)
		{
			if (!dict.ContainsKey(key))
				dict[key] = new List<CanHaveCardDataHandler>();
			dict[key].Add(f);
		}
	}
}