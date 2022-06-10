using HarmonyLib;
using UnityEngine;

namespace BerryLoaderNS
{
    public static partial class Patches
    {
        [HarmonyPatch(typeof(GameCanvas), "Awake")]
        [HarmonyPostfix]
        public static void GCAPost()
        {
            MenuAPI.Init();
            var m = new GameObject().AddComponent<CustomMenu>();
        }

        [HarmonyPatch(typeof(GameCanvas), "SetScreen")]
        [HarmonyPrefix]
        public static void GCSSPost(RectTransform screen)
        {
            foreach (var sc in MenuAPI.Screens)
            {
                if (sc != null)
                    sc.gameObject.SetActive(screen == sc);
            }
        }
    }
}