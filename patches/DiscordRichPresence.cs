using HarmonyLib;
using System;

namespace BerryLoaderNS
{
	public static partial class Patches
	{
		[HarmonyPatch(typeof(WorldManager), "Update")]
		[HarmonyPostfix]
		public static void DCWMUpdate()
		{
			// if the discord client dies while the game is running, this will cause error spam in console
			if (DiscordAPI.client != null)
				DiscordAPI.client.RunCallbacks();
		}

		[HarmonyPatch(typeof(WorldManager), "Awake")]
		[HarmonyPostfix]
		public static void DCWMAwake()
		{
			DiscordAPI.StartTimestamp = null;
			var menuText = BerryLoader.configUseEmoji.Value ? "📋 Menu" : "In the menu";
			DiscordAPI.UpdateActivity(menuText);
		}

		[HarmonyPatch(typeof(WorldManager), "Play")]
		[HarmonyPatch(typeof(WorldManager), "GoToBoard")]
		[HarmonyPostfix]
		public static void UpdatePresence1()
		{
			if (DiscordAPI.StartTimestamp == null)
				DiscordAPI.StartTimestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
			DiscordAPI.UpdateActivity($"{DiscordAPI.GetBoardString(WorldManager.instance.CurrentBoard.Id)} | {DiscordAPI.GetMoon()} {WorldManager.instance.CurrentMonth}");
		}

		[HarmonyPatch(typeof(WorldManager), "EndOfMonth")]
		[HarmonyPrefix]
		public static void UpdatePresence2()
		{
			if (DiscordAPI.StartTimestamp == null)
				DiscordAPI.StartTimestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
			DiscordAPI.UpdateActivity($"{DiscordAPI.GetBoardString(WorldManager.instance.CurrentBoard.Id)} | {DiscordAPI.GetMoon()} {WorldManager.instance.CurrentMonth}");
		}
	}
}
