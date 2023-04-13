using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UltraFunGuns.Util;
using UnityEngine;
using System.IO;

namespace UltraFunGuns.Patches
{
    [HarmonyPatch(typeof(PostProcessV2_Handler), "Start")]
    public static class RadiantPatch
    {

        public static bool Prefix(PostProcessV2_Handler __instance)
        {

            if(TextureLoader.TryLoadTexture(Path.Combine(TextureLoader.GetTextureFolder(), "trans.png"), out Texture2D trans))
            {
                if(__instance.radiantBuff != null)
                {
                    HydraLogger.Log("Replaced buff texture", DebugChannel.Error);
                    __instance.radiantBuff.SetTexture("_BuffTex", trans);
                }
                __instance.buffTex = trans;
            }
            return true;

        }
    }
}
