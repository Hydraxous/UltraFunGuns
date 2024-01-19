using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UltraFunGuns
{
    [HarmonyPatch]
    public static class CoinPatch
    {
        private static List<Coin> coins = new List<Coin>();

        public static void OnDelayReflectRevolver(Coin __instance)
        {
            coins.Remove(__instance);
        }

        public static Coin GetLastAdded()
        {
            if (coins == null)
                return null;

            return coins[coins.Count - 1];
        }

        [HarmonyPatch(typeof(Coin), nameof(Coin.ReflectRevolver))]
        public static void PreReflectRevolver(Coin __instance)
        {
            coins.Add(__instance);
        }

        private static MethodInfo coinMethod = typeof(CoinPatch).GetMethod(nameof(SupplyCoinList), BindingFlags.Static | BindingFlags.NonPublic);
        private static MethodInfo transformGetPositon = typeof(Transform).GetMethod("get_position", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        [HarmonyPatch(typeof(Coin), nameof(Coin.ReflectRevolver)), HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> InjectICoinTargetDiscovery(IEnumerable<CodeInstruction> instructions)
        {
            Debug.LogWarning($"Beginning compilation of {nameof(InjectICoinTargetDiscovery)}");
            CodeInstruction[] codeInstructions = instructions.ToArray();

            //Matches IL code for locating the injection point of our code.
            //In this case the code is injected right after the list of targets is populated with Cannonballs and Grenades.
            bool ilMatch = ILTools.ILOrderMatch(codeInstructions, out int index, 0,
                OpCodes.Callvirt,
                OpCodes.Stloc_2,
                OpCodes.Ldloc_S,
                OpCodes.Callvirt,
                OpCodes.Stloc_S);


            for (int i = 0; i < codeInstructions.Length; i++)
            {
                if (i == index)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldloc_S, 16);
                    yield return new CodeInstruction(OpCodes.Call, coinMethod);
                    Debug.LogWarning("Complilation Sucess!");

                }

                yield return codeInstructions[i];
            }
        }

        

        private static MethodInfo evalMethod = typeof(CoinPatch).GetMethod(nameof(EvalCustomCoinTarget), BindingFlags.Static | BindingFlags.NonPublic);
        private static MethodInfo lineRenderer_setPosition = typeof(LineRenderer).GetMethod(nameof(LineRenderer.SetPosition), BindingFlags.Instance | BindingFlags.Public);


        [HarmonyPatch(typeof(Coin), nameof(Coin.ReflectRevolver)), HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> InjectICoinTargetEvaluation(IEnumerable<CodeInstruction> instructions)
        {
            Debug.LogWarning($"Beginning compilation of {nameof(InjectICoinTargetEvaluation)}");
            CodeInstruction[] codeInstructions = instructions.ToArray();

            for (int i = 0; i < codeInstructions.Length; i++)
            {
                if (codeInstructions[i].opcode == OpCodes.Callvirt && codeInstructions[i].OperandIs(lineRenderer_setPosition))
                {
                    //Matches IL code for locating the injection point of our code.
                    //In this case the code is injected right before the check to explode Cannonballs and Grenades.
                    if (ILTools.ILOrderMatch(codeInstructions, out int index, i,
                        OpCodes.Callvirt,
                        OpCodes.Ldloc_S,
                        OpCodes.Ldc_I4_1,
                        OpCodes.Ldloc_0,
                        OpCodes.Callvirt,
                        OpCodes.Callvirt,
                        OpCodes.Callvirt,
                        OpCodes.Ldloc_S,
                        OpCodes.Ldloca_S,
                        OpCodes.Callvirt))
                    {
                        if (index == i)
                        {
                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Ldloc_S, 27);
                            yield return new CodeInstruction(OpCodes.Ldloc_S, 17);
                            yield return new CodeInstruction(OpCodes.Call, evalMethod);
                            Debug.LogWarning("Complilation Sucess!");
                        }
                    }
                }

                yield return codeInstructions[i];
            }
        }

        private static void SupplyCoinList(Coin coin, List<Transform> list)
        {
            IEnumerable<ICoinTarget> uFGCoinTargets = GameObject.FindObjectsOfType<MonoBehaviour>().Where(x => typeof(ICoinTarget).IsAssignableFrom(x.GetType())).Select(x => (ICoinTarget)x);
            uFGCoinTargets = uFGCoinTargets.Where(x => x.CanBeCoinTargeted(coin));
            uFGCoinTargets = uFGCoinTargets.OrderBy(x=>x.GetCoinTargetPriority(coin));
            list.AddRange(uFGCoinTargets.Select(x=>x.GetCoinTargetPoint(coin)));
        }

        //Currently this is only called if the coin is shot with the base revolver. TODO add railgun support
        private static void EvalCustomCoinTarget(Coin coin, LineRenderer beamLine, Transform transform)
        {
            //Debug.LogWarning("EVAL CUSTOM COIN TARGET");
            RevolverBeam rb = beamLine.GetComponent<RevolverBeam>();

            if(transform.TryGetComponent<ICoinTarget>(out ICoinTarget coinTarget))
            {
                coinTarget.OnCoinReflect(coin, rb);
            }
        }

    }
}
