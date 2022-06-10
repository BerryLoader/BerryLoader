using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace BerryLoaderNS
{
    public static class ResourceHelper
    {
        public static IEnumerator GetAudioClip(CardData card, string name)
        {
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(Path.Combine(Directory.GetCurrentDirectory(), "BepInEx/plugins/BerryLoader/Sounds", name), AudioType.MPEG))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.ConnectionError)
                {
                    BerryLoader.L.LogError(www.error);
                }
                else
                {
                    card.PickupSound = DownloadHandlerAudioClip.GetContent(www);
                }
            }
        }
    }
}