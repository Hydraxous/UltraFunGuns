using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraFunGuns.Patches
{
    [HarmonyPatch(typeof(StyleHUD))]
    public static class StyleHUDPatch
    {
        [HarmonyPatch(nameof(StyleHUD.AddPoints)), HarmonyPrefix]
        public static void OnPreAddPoints(int points, string pointID, GameObject sourceWeapon = null, EnemyIdentifier eid = null, int count = -1, string prefix = "", string postfix = "")
        {
            Debug.LogWarning($"{points}, {pointID}, {((sourceWeapon != null) ? sourceWeapon.name : "NULL")}, {((eid != null) ? eid.name : "NULL")}, {count}, {prefix}, {postfix}");
        }

        [HarmonyPatch(nameof(StyleHUD.DecayFreshness)), HarmonyPrefix]
        public static void OnPreDecay(GameObject sourceWeapon, string pointID, bool boss, Dictionary<GameObject, float> ___weaponFreshness)
        {
            Debug.LogWarning($"{((sourceWeapon != null) ? sourceWeapon.name : "NULL")}, {pointID}, {boss}");
            Debug.LogWarning($"WeaponFresh is {((___weaponFreshness != null) ? "not null" : "NULL!!!!")}");

        }
    }
}
