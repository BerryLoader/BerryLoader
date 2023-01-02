using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace BerryLoaderNS
{
	public static class ResourceHelper
	{
		// this maps card ids to audioclips, which are assigned to cards in a WorldManager.CreateCard patch (patches/WMCreate.cs)
		public static Dictionary<string, AudioClip> AudioClips = new Dictionary<string, AudioClip>();

		// adapted from #3, ty margmas!
		public static Sprite GetSprite(string path)
		{
			var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false); // this expands
			texture.LoadImage(File.ReadAllBytes(path));
			return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one / 2f);
		}

		public static IEnumerator GetAudioClip(string path, Action<AudioClip> callback, Action onError = null)
		{
			return GetAudioClip(path, AudioType.MPEG, callback, onError);
		}

		public static IEnumerator GetAudioClip(string path, AudioType type, Action<AudioClip> callback, Action onError = null)
		{
			using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(path, type))
			{
				www.timeout = 3;
				yield return www.SendWebRequest();

				if (www.result == UnityWebRequest.Result.ConnectionError)
				{
					BerryLoader.L.LogError($"error while loading audio: {www.error}");
					onError?.Invoke();
				}
				else
					callback(DownloadHandlerAudioClip.GetContent(www));
			}
		}
	}
}