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
            if (!target.TryFindComponent<IParriable>(out IParriable parriable))
                return true;

            if (!parriable.Parry(CameraController.Instance.transform.position, CameraController.Instance.transform.forward))
                return true;

            __instance.anim.Play("Hook", 0, 0.065f);
            MonoSingleton<TimeController>.Instance.ParryFlash();
            ___hitSomething = true;
            __result = true;
            return false;
        }
    }
}
