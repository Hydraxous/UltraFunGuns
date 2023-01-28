using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using HarmonyLib;

namespace UltraFunGuns
{
    [HarmonyPatch(typeof(Punch), "CheckForProjectile")]
    public static class ParryPatch
    {
        public static bool Prefix(Punch __instance, Transform target, bool __result, ref bool ___hitSomething)
        {

            if(!target.TryGetComponent<IUFGInteractionReceiver>(out IUFGInteractionReceiver ufgObject))
            {
                ufgObject = target.GetComponentInParent<IUFGInteractionReceiver>();
            }

            if(ufgObject != null)
            {
                if(ufgObject.Parried(MonoSingleton<CameraController>.Instance.cam.transform.forward))
                {
                    __instance.anim.Play("Hook", 0, 0.065f);
                    MonoSingleton<TimeController>.Instance.ParryFlash();
                    ___hitSomething = true;
                    __result = true;
                    return false;
                }
            }

            return true;
        }
    }
}
