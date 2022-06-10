using HarmonyLib;
using UnityEngine;

namespace BerryLoaderNS
{
    public static partial class Patches
    {
        // ELITE INTRO SKIP HACK
        // FREE DOWNLOAD NO VIRUS WORKING 2023
        [HarmonyPatch(typeof(Sokpop.SokIntro), "Awake")]
        [HarmonyPrefix]
        static bool IntroAwake(Sokpop.SokIntro __instance)
        {
            if (BerryLoader.configSkipIntro.Value == true)
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(__instance.NextSceneName);
                return false;
            }
            return true;
        }
    }
}