using HarmonyLib;
using System;
using System.Collections.Generic;

namespace BerryLoaderNS
{
	public static class DiscordAPI
	{
		public static Discord.Discord client;
		public static Discord.ActivityManager am;
		public static long? StartTimestamp = null;

		public static Dictionary<string, string> BoardLookup = new Dictionary<string, string>()
		{
			{"main", "On the mainland"},
			{"island", "On the island"}
		};

		public static void Init()
		{
			BerryLoader.L.LogInfo("initing discord..");
			try
			{
				client = new Discord.Discord(974015953782341692, (UInt64)Discord.CreateFlags.NoRequireDiscord);
				am = client.GetActivityManager();
			}
			catch (Discord.ResultException)
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
			if (StartTimestamp != null)
			{
				activity.Timestamps = new Discord.ActivityTimestamps
				{
					Start = (Int64)StartTimestamp
				};
			}
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

		public static string GetBoardString(string boardId) => BoardLookup.ContainsKey(boardId) ? BoardLookup[boardId] : $"Board: {boardId}";

		public static string GetMoon() => BerryLoader.configUseEmoji.Value ? "🌙" : "Moon";
	}
}