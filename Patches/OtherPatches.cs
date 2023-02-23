using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using UnityEngine;

namespace UltraFunGuns
{
    [HarmonyPatch(typeof(StyleHUD))]
    [HarmonyPatch(nameof(StyleHUD.GetFreshnessState))]
    public static class StyleHUD_Annoyance_Patch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            int startIndex = -1;

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == System.Reflection.Emit.OpCodes.Ldstr)
                {
                    HydraLogger.Log(">!!!", DebugChannel.Fatal);
                    var strOperand = codes[i].operand as string;
                    if (strOperand == "Current weapon not in StyleHUD weaponFreshness dict!!!")
                    {
                        HydraLogger.Log("FOUND IT.", DebugChannel.Fatal);
                        startIndex = i;
                        break;
                    }
                    else
                    {
                        HydraLogger.Log(">NOT STRING", DebugChannel.Fatal);
                    }
                }
            }

            if (startIndex > -1)
            {
                codes.RemoveRange(startIndex, 2);
                HydraLogger.Log(">REMOVED CODE INDICIES", DebugChannel.Fatal);

            }

            return codes.AsEnumerable();
        }
    }
}
