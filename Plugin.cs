using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace BerryLoaderNS
{
	[BepInPlugin("BerryLoader", "BerryLoader", "0.1.0")]
	[BepInProcess("Stacklands.exe")]
	public class BerryLoader : BaseUnityPlugin
	{
		public static string VERSION = "0.1.0";
		public static BepInEx.Logging.ManualLogSource L;

		public static string berryDir;
		public static string modsDir;
		public static List<string> modDirs = new List<string>();
		public static List<ModManifest> modManifests = new List<ModManifest>();
		public static Dictionary<string, Type> modTypes = new Dictionary<string, Type>();
		public static List<BerryLoaderMod> modClasses = new List<BerryLoaderMod>();

		public static ConfigEntry<bool> configSkipIntro;
		public static ConfigEntry<bool> configCompactTooltips;
		public static ConfigEntry<bool> configDisablePauseText;

		public static Harmony HarmonyInstance;

		private void Awake()
		{
			HarmonyInstance = new Harmony("BerryLoader");
			L = Logger;
			configSkipIntro = Config.Bind("Patches", "SkipIntro", false, "Enable intro skip");
			configCompactTooltips = Config.Bind("Patches", "CompactTooltips", false, "Enable compact tooltips");
			configDisablePauseText = Config.Bind("Patches", "DisablePauseText", false, "Disable flashing pause text");
			L.LogInfo("BerryLoader is loaded..");

			HarmonyInstance.PatchAll(typeof(Patches));

			berryDir = Path.Combine(Directory.GetCurrentDirectory(), "BepInEx/Plugins/BerryLoader");
			modsDir = Path.Combine(Directory.GetCurrentDirectory(), "mods");

			L.LogInfo("loading manifests..");
			foreach (var directory in new DirectoryInfo(modsDir).GetDirectories())
			{
				modDirs.Add(directory.ToString());
				var modid = directory.Name;
				modManifests.Add(JsonConvert.DeserializeObject<ModManifest>(File.ReadAllText(Path.Combine(modsDir, modid, "manifest.json"))));
			}

			modManifests = DependencyHelper.GetValidModLoadOrder(modManifests);

			L.LogInfo("loading assemblies..");
			foreach (var manifest in modManifests)
			{
				// Stacklands/mods/<id>/<id>.dll
				L.LogInfo($"loading {manifest.id}.dll..");
				var mod = Assembly.Load(File.ReadAllBytes(Path.Combine(Directory.GetCurrentDirectory(), "mods", manifest.id, $"{manifest.id}.dll")));
				foreach (var t in ReflectionHelper.GetSafeTypes(mod))
				{
					if (typeof(CardData).IsAssignableFrom(t))
					{
						L.LogInfo($"Found CardData: {t}");
						InteractionAPI.CardDatas.Add(t);
					}
					if (t.BaseType == typeof(BerryLoaderMod))
					{
						L.LogInfo($"Found main class: {t.ToString()}");
						BerryLoaderMod c = (BerryLoaderMod)Activator.CreateInstance(t);
						c.manifest = manifest;
						modClasses.Add(c);
					}
					modTypes[t.ToString()] = t;
				}
			}

			foreach (var t in ReflectionHelper.GetSafeTypes(typeof(CardData).Assembly))
			{
				if (typeof(CardData).IsAssignableFrom(t))
				{
					InteractionAPI.CardDatas.Add(t);
				}
			}

			foreach (var mod in modClasses)
				mod.Init();

			InteractionAPI.Init();
			DiscordAPI.Init();
		}
	}
}
