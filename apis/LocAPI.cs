using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace BerryLoaderNS
{
	public static class LocAPI
	{
		public static bool Injected = false;
		public static List<string> Injectables = new List<string>();

		public static void LoadTsvFromFile(string path)
		{
			var data = File.ReadAllText(path);
			Proceed(data);
		}

		public static void LoadTsvFromGoogleSheets(string url)
		{
			BerryLoader.instance.StartCoroutine(GetRequest(url, ((string data) =>
			{
				Proceed(data);
			})));
		}

		internal static void Proceed(string data)
		{
			if (!Injected)
			{
				Injectables.Add(data);
				return;
			}
			string[][] locTable = ParseTableFromTsv(data);
			SokLoc.instance.LanguageChanged += (() => { LoadLoc(locTable); });
			LoadLoc(locTable);
		}

		private static void LoadLoc(string[][] locTable)
		{
			int languageColumnIndex = GetLanguageColumnIndex(locTable, SokLoc.instance.CurrentLocSet.Language);
			if (languageColumnIndex == -1)
			{
				return;
			}
			for (int i = 1; i < locTable.Length; i++)
			{
				string text = locTable[i][0];
				string fullText = locTable[i][languageColumnIndex];
				text = text.Trim();
				text = text.ToLower();
				if (!string.IsNullOrEmpty(text))
				{
					SokTerm sokTerm = new SokTerm(SokLoc.instance.CurrentLocSet, text, fullText);
					if (SokLoc.instance.CurrentLocSet.TermLookup.ContainsKey(text)) // 1 check should be enough since they arent set seperately
						continue;
					SokLoc.instance.CurrentLocSet.AllTerms.Add(sokTerm);
					SokLoc.instance.CurrentLocSet.TermLookup.Add(text, sokTerm);
				}
			}
		}

		private static string[][] ParseTableFromTsv(string tsv)
		{
			tsv = tsv.TrimEnd('\n');
			string[] array = tsv.Split('\n');
			string[][] array2 = new string[array.Length][];
			for (int i = 0; i < array.Length; i++)
			{
				string text = array[i];
				text = text.Replace("\r", "");
				array2[i] = text.Split('\t');
			}
			BerryLoader.L.LogInfo($"Loaded {array.Length - 1} rows");
			return array2;
		}

		private static IEnumerator GetRequest(string url, Action<string> callback)
		{
			using (UnityWebRequest req = UnityWebRequest.Get(url))
			{
				req.timeout = 5;

				yield return req.SendWebRequest();

				if (req.result == UnityWebRequest.Result.Success)
					callback.Invoke(req.downloadHandler.text);
				else BerryLoader.L.LogError($"request failed ({req.result}) for url {url}");
			}
		}

		public static int GetLanguageColumnIndex(string[][] locTable, string language)
		{
			for (int i = 0; i < locTable[0].Length; i++)
			{
				if (locTable[0][i] == language)
				{
					return i;
				}
			}
			BerryLoader.L.LogError("No column exists for " + language);
			return -1;
		}
	}
}
