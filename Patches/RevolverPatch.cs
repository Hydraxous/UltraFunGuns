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

                CanProjectile hitCan = currentHit.transform.GetComponentInParent<CanProjectile>();
                if (hitCan != null)
                {
                    Debug.Log("Shot thingy with thingy");
                    switch (__instance.beamType)
                    {
                        case BeamType.Railgun:
                            hitCan.Explode(Vector3.up,3);
                            break;
                        case BeamType.Revolver:
                            hitCan.Bounce();
                            break;
                        case BeamType.MaliciousFace:
                            hitCan.Explode(Vector3.up, 3);
                            break;
                        case BeamType.Enemy:
                            hitCan.Bounce();
                            break;
                    }
                }

                RemoteBombExplosive remoteBombExplosive = currentHit.transform.GetComponentInParent<RemoteBombExplosive>();
                if (remoteBombExplosive != null)
                {

                    switch (__instance.beamType)
                    {
                        case BeamType.Railgun:
                            remoteBombExplosive.Detonate(true);
                            break;
                        case BeamType.Revolver:
                            remoteBombExplosive.Detonate(false);
                            break;
                        case BeamType.MaliciousFace:
                            remoteBombExplosive.Detonate(true);
                            break;
                        case BeamType.Enemy:
                            remoteBombExplosive.Detonate(true);
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
