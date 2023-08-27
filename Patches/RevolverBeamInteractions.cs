using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraFunGuns.Patches
{
    [HarmonyPatch(typeof(RevolverBeam))]
    public static class RevolverBeamInteractions
    {

        [HarmonyPatch("Shoot"), HarmonyPrefix]
        public static bool OnFirePrefix(RevolverBeam __instance)
        {
            //impl
            return true;
        }

        //TODO patch PierceShotCheck to fix bad beam code

        [HarmonyPatch(nameof(RevolverBeam.ExecuteHits)), HarmonyPrefix]
        public static void ExecuteHitsPrefix(RevolverBeam __instance, RaycastHit currentHit)
        {
            HydraLogger.Log($"ExecHitsPatch Run", DebugChannel.User);

            try
            {
                if (currentHit.transform.TryFindComponent<IUFGInteractionReceiver>(out IUFGInteractionReceiver ufgObject))
                {
                    ufgObject.Shot(__instance.beamType);
                }
            }
            catch (System.Exception e)
            {
                HydraLogger.Log($"Revolver Hit Error: {e.Message}\n{e.StackTrace}", DebugChannel.Error);
            }

        }


        private static bool ContinueShot(RevolverBeam beamSource)
        {
            if (!Physics.Raycast(beamSource.transform.position, beamSource.transform.forward, out RaycastHit hit, Mathf.Infinity, (int)LMD.EnemiesAndEnvironment)) //TODO fix
                return true;

            if (hit.transform.gameObject.layer != 10 && hit.transform.gameObject.layer != 11)
                return true;

            if (hit.transform.gameObject.CompareTag("Breakable"))
                return true;

            if (beamSource.bodiesPierced >= beamSource.hitAmount)
                return true;
         
            if (!hit.transform.TryFindComponent<IUFGBeamInteractable>(out IUFGBeamInteractable beamInteractable))
                return true;

            return !beamInteractable.OnBeamHit(beamSource);
        }

        private static bool OnBeamFired(RevolverBeam beam)
        {
            //impl
            return true;
        }
    }
}
