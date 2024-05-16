using HarmonyLib;
using System.Collections.Generic;

namespace UltraFunGuns.Patches
{
    public static class DebuggingPatches
    {
        [HarmonyPatch(typeof(GameStateManager))]
        public static class GameStateManagerPatch
        {
            [HarmonyPrefix]
            [HarmonyPatch(nameof(GameStateManager.RegisterState))]
            static bool Prefix1(GameStateManager __instance, GameState newState)
            {
                HydraLogger.Log("--Registered State--");
                HydraLogger.Log(FormatGameState(newState));
                return true;
            }

            [HarmonyPrefix]
            [HarmonyPatch("EvaluateState")]
            static bool Prefix2(GameStateManager __instance, Dictionary<string, GameState> ___activeStates)
            {
                HydraLogger.Log("--Evaluate State--");
                foreach (KeyValuePair<string, GameState> keyValuePair in ___activeStates)
                {
                    if (keyValuePair.Value != null)
                    {
                        HydraLogger.Log(FormatGameState(keyValuePair.Value));
                    }
                }
                return true;
            }

            private static string FormatGameState(GameState state)
            {
                if (state == null)
                {
                    return "NULL-STATE.";
                }

                string msg = $"GameState: {state.key} ({state.priority}) \n[{state.cursorLock}|{state.playerInputLock}|{state.cameraInputLock}]\n";

                if (state.trackedObjects != null)
                {
                    for (int i = 0; i < state.trackedObjects.Length; i++)
                    {
                        if (state.trackedObjects[i] != null)
                        {
                            msg += $"Tracked {i + 1}: {state.trackedObjects[i].name}\n";
                        }
                    }

                    return msg;
                }

                if (state.trackedObject != null)
                {
                    return msg + $"Tracked {state.trackedObject.name}\n";
                }

                return msg;
            }
        }
    }
}
