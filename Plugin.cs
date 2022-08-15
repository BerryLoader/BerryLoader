using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace BerryLoaderNS
{
	[BepInPlugin("BerryLoader", "BerryLoader", BerryLoader.VERSION)]
	[BepInProcess("Stacklands.exe")]
	public class BerryLoader : BaseUnityPlugin
	{
		public const string VERSION = "0.5.0";
		public static BepInEx.Logging.ManualLogSource L;

		public static List<string> modDirs = new List<string>();
		public static Dictionary<string, Type> modTypes = new Dictionary<string, Type>();

		public static ConfigEntry<bool> configSkipIntro;
		public static ConfigEntry<bool> configCompactTooltips;
		public static ConfigEntry<bool> configDisablePauseText;
		public static ConfigEntry<bool> configUseEmoji;
		public static ConfigEntry<bool> configVerboseLogging;

		public static Harmony HarmonyInstance;

		private static bool Inited = false;

		private void Awake()
		{
			HarmonyInstance = new Harmony("BerryLoader");
			L = Logger;
			configSkipIntro = Config.Bind("Patches", "SkipIntro", false, "Enable intro skip");
			configCompactTooltips = Config.Bind("Patches", "CompactTooltips", false, "Enable compact tooltips");
			configDisablePauseText = Config.Bind("Patches", "DisablePauseText", false, "Disable flashing pause text");
			configUseEmoji = Config.Bind("Patches", "UseEmoji", true, "Use emojis in the Discord rich presence");
			configVerboseLogging = Config.Bind("Logging", "VerboseLogging", false, "Enable more verbose logging, useful for debugging");
			L.LogInfo("BerryLoader is loaded..");

			HarmonyInstance.PatchAll(typeof(BerryLoader));
			HarmonyInstance.PatchAll(typeof(Patches));
		}

		[HarmonyPatch(typeof(WorldManager), "Awake")]
		[HarmonyPrefix]
		static void BLInit()
		{
			if (Inited) return;
			Inited = true;

			BerryLoader.L.LogInfo("Loading BIE plugins..");
			List<BaseUnityPlugin> berryPlugins = BepInEx.Bootstrap.Chainloader.PluginInfos
				.Where(DependencyHelper.IsDependentOnBerryLoader)
				.Select(x => x.Value.Instance)
				.ToList();

			List<Type> foundStatusEffects = new List<Type>();

			foreach (var plugin in berryPlugins)
			{
				L.LogInfo($"found BIE plugin: {plugin.Info.Metadata.Name} | Directory: {Directory.GetParent(plugin.Info.Location)}");
				modDirs.Add(Directory.GetParent(plugin.Info.Location).ToString());
				var ass = plugin.GetType().Assembly;
				foreach (var t in ReflectionHelper.GetSafeTypes(ass))
				{
					if (typeof(CardData).IsAssignableFrom(t))
					{
						L.LogInfo($"Found CardData: {t}");
						InteractionAPI.CardDatas.Add(t);
						modTypes[t.ToString()] = t;
					}
					if (t.IsSubclassOf(typeof(StatusEffect)))
					{
						L.LogInfo($"Found StatusEffect: {t}");
						foundStatusEffects.Add(t);
					}
				}
			}

			foreach (var t in ReflectionHelper.GetSafeTypes(typeof(CardData).Assembly))
			{
				if (typeof(CardData).IsAssignableFrom(t))
				{
					InteractionAPI.CardDatas.Add(t);
					modTypes[$"Stacklands.{t.ToString()}"] = t;
				}
				if (t.IsSubclassOf(typeof(StatusEffect)))
				{
					foundStatusEffects.Add(t);
				}
			}

			// StatusEffect.InitStatusEffectTypes only searches the GameScripts assembly, so this is required for the game to recognize modded status effects
			typeof(StatusEffect).GetField("statusEffectTypes", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, foundStatusEffects);

			InteractionAPI.Init();
			DiscordAPI.Init();
		}
	}
}
