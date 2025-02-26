using HarmonyLib;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    [HarmonyPatch(typeof(GunControl))]
    public static class GunControlPatch
    {
        [HarmonyPatch(nameof(GunControl.UpdateWeaponList)), HarmonyPrefix]
        public static void OnUpdateWeaponList(GunControl __instance, bool firstTime)
        {
            UltraFunGuns.Log.Log($"GunControl: Set weapons {firstTime}");
            if (__instance.slots == null)
                return;

            //Awful hack for a race condition present in GunControl and GunSetter.
            if (__instance.slots.Count < 6)
                return;

            WeaponManager.DeployWeapons(firstTime);
        }

        [HarmonyPatch(nameof(GunControl.UpdateWeaponList)), HarmonyPostfix]
        public static void OnUpdateWeaponList2(bool firstTime)
        {
            GunControl gc = GunControl.Instance;
            StringBuilder sb = new StringBuilder();
            UltraFunGuns.Log.Log($"FIRST :{firstTime}");
            for (int i = 0; i < gc.slots.Count; i++)
            {
                sb.AppendLine("--------------------");
                sb.AppendLine($"Slot {i}: {gc.slots[i].Count}");
                for (int j = 0; j < gc.slots[i].Count; j++)
                {
                    sb.AppendLine($"Weapon {j}: {gc.slots[i][j].name}");
                }

                UltraFunGuns.Log.Log(sb.ToString());
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
