using HarmonyLib;
using System.Collections;
using UnityEngine;

namespace BerryLoaderNS
{
	public static partial class Patches
	{
		[HarmonyPatch(typeof(CreatePackLine), "Start")]
		[HarmonyPrefix]
		public static bool CPLStart(CreatePackLine __instance)
		{
			__instance.StartCoroutine(DelayCreate(__instance));
			return false;
		}

		public static IEnumerator DelayCreate(CreatePackLine pl)
		{
			yield return null; // 1 frame of delay fixes the pack positions
			pl.Create();
		}
	}
}