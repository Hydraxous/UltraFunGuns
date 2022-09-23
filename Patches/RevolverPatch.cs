using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using UnityEngine;

namespace UltraFunGuns
{
    [HarmonyPatch(typeof(RevolverBeam), "ExecuteHits")]
    public static class BeamPatch
    {
        public static void Prefix(RaycastHit currentHit)
        {
            ThrownEgg hitEgg = currentHit.transform.GetComponentInParent<ThrownEgg>();
            if (hitEgg != null)
            {
                hitEgg.Explode();
            }
        }

    }
}
