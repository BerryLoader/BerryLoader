using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace BerryLoaderNS
{
	public static class ResourceHelper
	{
		public static Dictionary<string, AudioClip> AudioClips = new Dictionary<string, AudioClip>();

		public static IEnumerator GetAudioClip(string key, string path)
		{
			if (AudioClips.ContainsKey(key))
				yield break;
			//BerryLoader.L.LogInfo($"attempting to load {path} for {key}");
			using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.MPEG))
			{
				www.timeout = 3;
				yield return www.SendWebRequest();

				if (www.result == UnityWebRequest.Result.ConnectionError)
					BerryLoader.L.LogError($"error while loading audio: {www.error}");
				else
				{
					BerryLoader.L.LogInfo($"got audio for {key}");
					AudioClips.Add(key, DownloadHandlerAudioClip.GetContent(www));
				}
			}
		}

		public static Sprite LoadSprite(string modDir, string spriteName)
		{
			string path = Path.Combine(modDir, "Images", spriteName);

			if (File.Exists(path))
			{
				var texture = new Texture2D(2, 2);
				texture.LoadImage(File.ReadAllBytes(path));
				return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one / 2f);
			}

			var sprite = Resources.FindObjectsOfTypeAll<Sprite>().FirstOrDefault(i => i.name == spriteName);

			if (sprite != null)
			{
				return sprite;
			}

			BerryLoader.L.LogError($"Failed to load sprite: {spriteName}");
			return null;
		}
	}
}
