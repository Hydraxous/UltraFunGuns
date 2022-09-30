using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using UnityEngine;

namespace UltraFunGuns
{
    [HarmonyPatch(typeof(RevolverBeam), "ExecuteHits")]
    public static class ExecuteHitsPatch
    {
        public static void Prefix(RevolverBeam __instance, RaycastHit currentHit)
        {
            try
            {
                ThrownEgg hitEgg = currentHit.transform.GetComponentInParent<ThrownEgg>();
                if (hitEgg != null)
                {
                    hitEgg.Explode();
                }

                ThrownDodgeball hitDodgeball = currentHit.transform.GetComponentInParent<ThrownDodgeball>();
                if (hitDodgeball != null)
                {
                    switch (__instance.beamType)
                    {
                        case BeamType.Railgun:
                            hitDodgeball.ExciteBall(6);
                            break;

                        case BeamType.Revolver:
                            hitDodgeball.ExciteBall();
                            break;

                        case BeamType.MaliciousFace:
                            hitDodgeball.ExciteBall(2);
                            break;

                        case BeamType.Enemy:
                            break;
                    }

                }
            }
            catch(System.Exception e)
            {

            }
            
        }

    }
}
