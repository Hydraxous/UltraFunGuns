using System;
using System.Collections.Generic;
using UnityEngine;

namespace UltraFunGuns
{
    public static class UKEvents
    {
        
        public static class Enemy
        {
            public static event Action<EnemyDeathEvent> OnEnemyDeath;

            private static List<EnemyIdentifier> deathEventQueue = new List<EnemyIdentifier>();

            //[HarmonyPatch(typeof(EnemyIdentifier), nameof(EnemyIdentifier.Death)), HarmonyPrefix]
            //static void Prefix(EnemyIdentifier __instance)
            //{
            //    if (__instance.dead)
            //        return;

            //    deathEventQueue.Add(__instance);
            //}

            //[HarmonyPatch(typeof(EnemyIdentifier), nameof(EnemyIdentifier.Death)), HarmonyPostfix]
            //static void Postfix(EnemyIdentifier __instance)
            //{
            //    if (!deathEventQueue.Contains(__instance))
            //        return;

            //    EnemyDeathEvent enemyDeathEvent = new EnemyDeathEvent()
            //    {
            //        EnemyIdentifier = __instance,
            //    };

            //    if(trackedHits.ContainsKey(__instance))
            //    {
            //        List<EnemyHitEvent> pastHits = trackedHits[__instance];
            //        if(pastHits.Count > 0)
            //            enemyDeathEvent.LastHit = pastHits[pastHits.Count];
            //    }
            //}

            private static Dictionary<EnemyIdentifier, List<EnemyHitEvent>> trackedHits = new Dictionary<EnemyIdentifier, List<EnemyHitEvent>>();

            //[HarmonyPatch(typeof(EnemyIdentifier), nameof(EnemyIdentifier.DeliverDamage)), HarmonyPostfix]
            //static void Postfix(EnemyIdentifier __instance, GameObject ___target, Vector3 ___force, Vector3 ___hitPoint, float ___multiplier, bool ___tryForExplode, float ___critMultiplier, GameObject ___sourceWeapon)
            //{
            //    EnemyHitEvent hitEvent = new EnemyHitEvent
            //    {
            //        target = ___target,
            //        force = ___force,
            //        hitPoint = ___hitPoint,
            //        multiplier = ___multiplier,
            //        tryForExplode = ___tryForExplode,
            //        critMultiplier = ___critMultiplier,
            //        sourceWeapon = ___sourceWeapon
            //    };

            //    if (!trackedHits.ContainsKey(__instance))
            //        trackedHits.Add(__instance, new List<EnemyHitEvent>());

            //    trackedHits[__instance].Add(hitEvent);
            //}

            public struct EnemyHitEvent
            {
                public GameObject target;
                public Vector3 force;
                public Vector3 hitPoint;
                public float multiplier;
                public bool tryForExplode;
                public float critMultiplier;
                public GameObject sourceWeapon;

                public bool IsValid()
                {
                    return target != null || sourceWeapon != null;
                }
            }

            public struct EnemyDeathEvent
            {
                public EnemyIdentifier EnemyIdentifier;
                public EnemyHitEvent LastHit;
            }
        }

        public static class Player
        {

        }

        public static class World
        {

        }
    }
}
