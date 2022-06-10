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
            client = new Discord.Discord(974015953782341692, (UInt64)Discord.CreateFlags.NoRequireDiscord);
            am = client.GetActivityManager();
            var activity = new Discord.Activity
            {
                Details = "In the menus",
                Assets = {
                    LargeImage = "berryloader",
                },
                Instance = true
            };
            am.UpdateActivity(activity, result =>
            {
                BerryLoader.L.LogInfo($"discord got result: {result}");
            });
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
            am.UpdateActivity(activity, result =>
            {
                BerryLoader.L.LogInfo($"discord got result: {result}");
            });
        }

        public static void UpdateActivity(Discord.Activity activity)
        {
            am.UpdateActivity(activity, result =>
            {
                BerryLoader.L.LogInfo($"discord got result: {result}");
            });
        }
    }
}