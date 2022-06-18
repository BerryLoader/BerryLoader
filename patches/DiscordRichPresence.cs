using HarmonyLib;

namespace BerryLoaderNS
{
	public static partial class Patches
	{
		[HarmonyPatch(typeof(WorldManager), "Update")]
		[HarmonyPostfix]
		public static void DCWMUpdate()
		{
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
			DiscordAPI.UpdateActivity("Stacking cards", $"Moon {WorldManager.instance.CurrentMonth}");
		}

		[HarmonyPatch(typeof(WorldManager), "EndOfMonth")]
		[HarmonyPrefix]
		public static void DCWMEndOfMonth()
		{
			DiscordAPI.UpdateActivity("Stacking cards", $"Moon {WorldManager.instance.CurrentMonth}");
		}
	}
}