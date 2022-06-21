using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace BerryLoaderNS
{
	public static class ResourceHelper
	{
		public static IEnumerator GetAudioClip(CardData card, string path)
		{
			using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.MPEG))
			{
				www.timeout = 3;
				yield return www.SendWebRequest();

				if (www.result == UnityWebRequest.Result.ConnectionError)
				{
					BerryLoader.L.LogError($"error while loading audio: {www.error}");
				}
				else
				{
					BerryLoader.L.LogInfo($"got audio for {card.Id}");
					card.PickupSound = DownloadHandlerAudioClip.GetContent(www);
				}
			}
		}
	}
}