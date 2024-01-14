using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    [HarmonyPatch]
    public static class RevolverBeamPatches
    {
        [HarmonyPatch(typeof(RevolverBeam), nameof(RevolverBeam.ExecuteHits)), HarmonyPrefix]
        public static bool OnExecuteHits(RevolverBeam __instance, ref RaycastHit currentHit)
        {
            if (currentHit.transform == null)
                return true;

            if (!currentHit.transform.TryGetComponent<IUFGBeamInteractable>(out IUFGBeamInteractable beamInteractable))
                return true;

            if (!beamInteractable.CanRevolverBeamHit(__instance, ref currentHit))
                return true;

            beamInteractable.OnRevolverBeamHit(__instance, ref currentHit);
            return false;
        }

        public static void SetShotHitPoint(this RevolverBeam instance, Vector3 point)
        {
            FieldInfo shotHitField = typeof(RevolverBeam).GetField("shotHitPoint", BindingFlags.NonPublic | BindingFlags.Instance);
            
            if (shotHitField == null)
            {
                throw new Exception("Could not find shotHitPoint field in RevolverBeam. Possible outdated UFG or Game Version.");
            }

            shotHitField.SetValue(instance, point);
        }
    }
}
