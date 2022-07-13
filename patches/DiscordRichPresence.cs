using HarmonyLib;

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
			DiscordAPI.UpdateActivity("In the menus");
		}

		[HarmonyPatch(typeof(WorldManager), "Play")]
		[HarmonyPostfix]
		public static void DCWMPlay()
		{
			DiscordAPI.UpdateActivity(DiscordAPI.GetBoardString(WorldManager.instance.CurrentBoard.Id), $"Moon {WorldManager.instance.CurrentMonth}");
		}

		[HarmonyPatch(typeof(WorldManager), "EndOfMonth")]
		[HarmonyPrefix]
		public static void DCWMEndOfMonth()
		{
			DiscordAPI.UpdateActivity(DiscordAPI.GetBoardString(WorldManager.instance.CurrentBoard.Id), $"Moon {WorldManager.instance.CurrentMonth}");
		}

		[HarmonyPatch(typeof(WorldManager), "GoToBoard")]
		[HarmonyPostfix]
		public static void DCWMGTB()
		{
			DiscordAPI.UpdateActivity(DiscordAPI.GetBoardString(WorldManager.instance.CurrentBoard.Id), $"Moon {WorldManager.instance.CurrentMonth}");
		}
	}
}
