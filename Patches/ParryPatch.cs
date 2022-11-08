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
            ThrownDodgeball dodgeball;
            if(target.TryGetComponent<ThrownDodgeball>(out dodgeball))
            {
                __instance.anim.Play("Hook", 0, 0.065f);
                MonoSingleton<TimeController>.Instance.ParryFlash();
                dodgeball.ExciteBall(2);
                ___hitSomething = true;
                __result = true;
                return false;
            }

            CanProjectile canProjectile;
            if (target.TryGetComponent<CanProjectile>(out canProjectile))
            {
                if(canProjectile.Parry())
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
