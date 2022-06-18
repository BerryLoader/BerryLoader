using System;
using HarmonyLib;

namespace BerryLoaderNS
{
	public static class DiscordAPI
	{
		public static Discord.Discord client;
		public static Discord.ActivityManager am;

		public static void Init()
		{
			BerryLoader.L.LogInfo("initing discord..");
			try
			{
				client = new Discord.Discord(974015953782341692, (UInt64)Discord.CreateFlags.NoRequireDiscord);
				am = client.GetActivityManager();
			}
			catch (Discord.ResultException e)
			{
				BerryLoader.L.LogInfo("discord is not running");
			}
			var activity = new Discord.Activity
			{
				Details = "In the menus",
				Assets = {
					LargeImage = "berryloader",
				},
				Instance = true
			};
			UpdateActivity(activity);
		}

		public static void UpdateActivity(string details = "", string state = "")
		{
			var activity = new Discord.Activity
			{
				Details = details,
				State = state,
				Assets = {
					LargeImage = "berryloader",
				},
				Instance = true
			};
			UpdateActivity(activity);
		}

		public static void UpdateActivity(Discord.Activity activity)
		{
			if (am != null)
			{
				am.UpdateActivity(activity, result =>
				{
					BerryLoader.L.LogInfo($"discord got result: {result}");
				});
			}
		}
	}
}