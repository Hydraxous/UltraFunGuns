using Discord;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraFunGuns.Patches
{
    [HarmonyPatch(typeof(DiscordController))]
    public static class FunkyDiscord
    {
        private static MapInfo lastMapInfo;

        [HarmonyPrefix]
        [HarmonyPatch("SendActivity")]
        public static bool Prefix(ref Activity ___cachedActivity)
        {
            string msg = 
                $"\nName: {___cachedActivity.Name}" +
                $"\nState: {___cachedActivity.State}" +
                $"\nLargeText: {___cachedActivity.Assets.LargeText}" +
                $"\nLargeImage: {___cachedActivity.Assets.LargeImage}" +
                $"\nSmallText: {___cachedActivity.Assets.SmallText}" +
                $"\nSmallImage: {___cachedActivity.Assets.SmallImage}" +
                $"\nTimeStampes: {___cachedActivity.Timestamps}" +
                $"\nDetails: {___cachedActivity.Details}";
            //HydraLogger.Log(msg, DebugChannel.Warning);
            if(lastMapInfo == null)
            {
                lastMapInfo = GameObject.FindObjectOfType<MapInfo>();
            }
            ___cachedActivity.State = (___cachedActivity.State.Contains("DIFFICULTY")) ? "DIFFICULTY: ULTRAKILL MUST DIE" : ___cachedActivity.State;
            ___cachedActivity.Assets.LargeText = ((___cachedActivity.Assets.LargeText.Contains("Custom") && lastMapInfo != null) ? lastMapInfo.mapName : ___cachedActivity.Assets.LargeText);
            return true;
        }
    }
}
