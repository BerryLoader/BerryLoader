using HarmonyLib;
using UnityEngine;

namespace BerryLoaderNS
{
	public static partial class Patches
	{
		[HarmonyPatch(typeof(SokLoc), "Awake")]
		[HarmonyPostfix]
		public static void SokLocAwake()
		{
			LocAPI.Injected = true;
			BerryLoader.L.LogInfo($"LoadLoc patch loading {LocAPI.Injectables.Count} strings");
			foreach (var data in LocAPI.Injectables)
				LocAPI.Proceed(data);
		}
	}
}