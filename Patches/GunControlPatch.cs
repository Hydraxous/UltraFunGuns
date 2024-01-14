using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using HarmonyLib;

namespace UltraFunGuns
{
    [HarmonyPatch(typeof(GunControl))]
    public static class GunControlPatch
    {
        [HarmonyPatch(nameof(GunControl.UpdateWeaponList)), HarmonyPrefix]
        public static void OnUpdateWeaponList(bool firstTime)
        {
            HydraLogger.Log($"GunControl: Set weapons {firstTime}");
            WeaponManager.DeployWeapons(firstTime);
        }

        [HarmonyPatch(nameof(GunControl.UpdateWeaponList)), HarmonyPostfix]
        public static void OnUpdateWeaponList2(bool firstTime)
        {
            GunControl gc = GunControl.Instance;
            StringBuilder sb = new StringBuilder();
            Debug.Log($"FIRST :{firstTime}");
            for (int i = 0; i < gc.slots.Count; i++)
            {
                sb.AppendLine("--------------------");
                sb.AppendLine($"Slot {i}: {gc.slots[i].Count}");
                for (int j = 0; j < gc.slots[i].Count; j++)
                {
                    sb.AppendLine($"Weapon {j}: {gc.slots[i][j].name}");
                }

                Debug.Log(sb.ToString());
                sb.Clear();
            }

            foreach (var sd in gc.slotDict)
            {
                sb.Clear();
                sb.AppendLine("--------------------");
                sb.AppendLine($"SD: K:{sd.Key.name} V:{sd.Value}");
            }
        }
    }
}
