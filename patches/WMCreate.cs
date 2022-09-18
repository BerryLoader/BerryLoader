using HarmonyLib;
using System;
using UnityEngine;

namespace BerryLoaderNS
{
	public static partial class Patches
	{
		[HarmonyPatch(typeof(WorldManager), "CreateBoosterpack")]
		[HarmonyPostfix]
		static void WMCBP(ref Boosterpack __result)
		{
			__result.gameObject.SetActive(true);
		}

		[HarmonyPatch(typeof(WorldManager), "CreateCard", new Type[] { typeof(Vector3), typeof(CardData), typeof(bool), typeof(bool), typeof(bool) })]
		[HarmonyPostfix]
		static void WMCC(ref CardData __result)
		{
			if (__result == null)
				return;
			var card = __result;
			if (card.gameObject.GetComponent<ModOverride>() != null && card.gameObject.GetComponent<Blueprint>() != null)
				card.gameObject.GetComponent<ModOverride>().Description = card.gameObject.GetComponent<Blueprint>().GetText();
			if (ResourceHelper.AudioClips.ContainsKey(card.Id))
			{
				BerryLoader.L.LogInfo($"setting audio for {card.Id} ({card.PickupSoundGroup})");
				card.PickupSound = ResourceHelper.AudioClips[card.Id];
			}
			card.gameObject.SetActive(true);
		}
	}
}