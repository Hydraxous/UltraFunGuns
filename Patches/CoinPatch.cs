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
        public static IEnumerable<CodeInstruction> InjectCoinTargeting(IEnumerable<CodeInstruction> instructions)
        {
            Debug.LogWarning($"Beginning compilation of {nameof(InjectCoinTargeting)}");
            CodeInstruction[] codeInstructions = instructions.ToArray();
            bool ilMatch = ILOrderMatch(codeInstructions, out int index, 0,
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

        private static bool ILOrderMatch(CodeInstruction[] instructions, out int index, int startIndex = 0, params OpCode[] orderMatch)
        {
            index = -1;
            int instructionLength = instructions.Length;
            int orderMatchLength = orderMatch.Length;

            for (int i = startIndex; i < instructions.Length; i++)
            {
                for(int j=0; j<orderMatch.Length; j++)
                {
                    if (instructions[i + j].opcode != orderMatch[j])
                        break;

                    if (j == orderMatchLength - 1)
                    {
                        index = i;
                        return true;
                    }
                }
            }

            return false;
        }

        private static MethodInfo evalMethod = typeof(CoinPatch).GetMethod(nameof(EvalCustomCoinTarget), BindingFlags.Static | BindingFlags.NonPublic);
        private static MethodInfo lineRenderer_setPosition = typeof(LineRenderer).GetMethod(nameof(LineRenderer.SetPosition), BindingFlags.Instance | BindingFlags.Public);


        [HarmonyPatch(typeof(Coin), nameof(Coin.ReflectRevolver)), HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> InjectInterfaceEvaluation(IEnumerable<CodeInstruction> instructions)
        {
            Debug.LogWarning($"Beginning compilation of {nameof(InjectInterfaceEvaluation)}");
            CodeInstruction[] codeInstructions = instructions.ToArray();

            for (int i = 0; i < codeInstructions.Length; i++)
            {
                if (codeInstructions[i].opcode == OpCodes.Callvirt && codeInstructions[i].OperandIs(lineRenderer_setPosition))
                {
                    if (ILOrderMatch(codeInstructions, out int index, i,
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
            uFGCoinTargets = uFGCoinTargets.OrderBy(x=>x.GetTargetPriority(coin));
            list.AddRange(uFGCoinTargets.Select(x=>x.GetCoinTargetPoint(coin)));
            Debug.LogWarning("Coins added");
        }

        private static void EvalCustomCoinTarget(Coin coin, LineRenderer beamLine, Transform transform)
        {
            Debug.LogWarning("EVAL CUSTOM COIN TARGET");
            RevolverBeam rb = beamLine.GetComponent<RevolverBeam>();

            if(transform.TryGetComponent<ICoinTarget>(out ICoinTarget coinTarget))
            {
                coinTarget.OnCoinReflect(coin, rb);
            }
        }

    }
}
