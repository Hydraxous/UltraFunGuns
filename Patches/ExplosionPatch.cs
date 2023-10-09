﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraFunGuns.Patches
{
    [HarmonyPatch(typeof(Explosion))]
    public static class ExplosionPatch
    {
        [HarmonyPatch("Collide"), HarmonyPostfix]
        public static void PostCollide(Explosion __instance, Collider other, List<Collider> ___hitColliders)
        {
            if (__instance.harmless)
                return;

            if (other == null)
                return;

            if (___hitColliders.Contains(other))
                return;

            if (other.TryGetComponent<IExplodable>(out IExplodable explodable))
                explodable.Explode(__instance);
        }

    }
}