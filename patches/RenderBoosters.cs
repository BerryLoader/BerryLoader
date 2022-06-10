using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BerryLoaderNS
{
	public static partial class Patches
	{
		[HarmonyPatch(typeof(BuyBoosterBox), "Update")]
		[HarmonyPrefix]
		static void BBBUpdate(BuyBoosterBox __instance)
		{
			if (!!!BerryLoader.RenderBoosters && __instance.gameObject.name == "Humble Beginnings")
			{
				BerryLoader.RenderBoosters = true;
				var original = __instance.gameObject;
				var packLine = original.transform.parent;
				foreach (ModBoosterpack bp in BerryLoader.BoosterpackInjectables)
				{
					var n = MonoBehaviour.Instantiate(original, new Vector3(-3.6f, 0, 2.18f), original.transform.localRotation, packLine);
					//n.transform.localPosition = new Vector3(-3.6f, 0, 0);
					n.gameObject.name = bp.name;
					n.GetComponent<BuyBoosterBox>().BoosterId = bp.id;
					n.GetComponent<BuyBoosterBox>().Cost = bp.cost;
				}

				// the following 10 lines of code took an entire afternoon. im tired and dumb.
				var order = packLine.GetComponentsInChildren<BuyBoosterBox>().OrderBy(x => x.Cost).Select(x => x.BoosterId).ToList();
				double total = 8 + BerryLoader.BoosterpackInjectables.Count();
				var upper = 0.8 * ((total - 1) / 2);
				var lower = -1 * upper;
				var coords = new List<float>();
				for (var i = lower; i <= upper + 0.1f; i += 0.8f) // I FUCKING HATE FLOATS
					coords.Add((float)Math.Round(i, 1));

				// cba to modify the actual order of the children
				// first is "Selling Cards"
				for (var i = 1; i < total; i++)
				{
					var child = packLine.transform.GetChild(i);
					// this is fucking dumb
					child.transform.localPosition = new Vector3(coords[order.FindIndex(x => child.GetComponent<BuyBoosterBox>().BoosterId == x) + 1], 0, 0);
				}
				packLine.transform.GetChild(0).transform.localPosition = new Vector3((float)lower, 0, 0);
			}
		}
	}
}