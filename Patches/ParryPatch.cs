using HarmonyLib;
using UnityEngine;

namespace UltraFunGuns
{
    [HarmonyPatch]
    public static class ParryPatch
    {
        [HarmonyPatch(typeof(Punch), "TryParryProjectile"), HarmonyPrefix]
        public static bool Prefix(Punch __instance, Transform target, bool __result, bool canProjectileBoost)
        {
            if (!target.TryFindComponent<IParriable>(out IParriable parriable))
                return true;

            if (!parriable.Parry(CameraController.Instance.transform.position, CameraController.Instance.transform.forward))
                return true;

            __instance.anim.Play("Hook", 0, 0.065f);
            TimeController.Instance.ParryFlash();
            __result = true;
            return false;
        }
    }
}
