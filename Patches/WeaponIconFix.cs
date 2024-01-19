using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace UltraFunGuns.Patches
{
    //TODO also  fix red revolver beam patch bc it fucking broke bruh
    //VariationColoredRenders.Length is called in OnEnable. If the component is added programatically like in UltraFunGunBase, it will throw a null reference error. This fixes that.
    [HarmonyPatch(typeof(WeaponIcon), "OnEnable")]
    public static class WeaponIconFix
    {
        public static bool Prefix(ref Renderer[] ___variationColoredRenderers, ref Image[] ___variationColoredImages, WeaponIcon __instance)
        {
            if (__instance.weaponDescriptor != null)
                return true;

            if(__instance.TryGetComponent<IUFGWeapon>(out IUFGWeapon ufgBase))
            {
                UFGWeapon weaponInfo = ufgBase.GetWeaponInfo();

                __instance.weaponDescriptor = ScriptableObject.CreateInstance<WeaponDescriptor>();

                HydraLoader.dataRegistry.TryGetValue($"{weaponInfo.WeaponKey}_weaponIcon", out UnityEngine.Object weapon_weaponIcon);
                __instance.weaponDescriptor.icon = (Sprite)weapon_weaponIcon;

                HydraLoader.dataRegistry.TryGetValue($"{weaponInfo.WeaponKey}_glowIcon", out UnityEngine.Object weapon_glowIcon);
                __instance.weaponDescriptor.glowIcon = (Sprite)weapon_glowIcon;

                __instance.weaponDescriptor.variationColor = (WeaponVariant)weaponInfo.IconColor;

                if (__instance.weaponDescriptor.icon == null)
                {
                    HydraLoader.dataRegistry.TryGetValue("debug_weaponIcon", out UnityEngine.Object debug_weaponIcon);
                    __instance.weaponDescriptor.icon = (Sprite)debug_weaponIcon;
                }

                if (__instance.weaponDescriptor.glowIcon == null)
                {
                    HydraLoader.dataRegistry.TryGetValue("debug_glowIcon", out UnityEngine.Object debug_glowIcon);
                    __instance.weaponDescriptor.glowIcon = (Sprite)debug_glowIcon;
                }
            }

            ___variationColoredRenderers = (___variationColoredRenderers == null) ? new Renderer[0] : ___variationColoredRenderers;
            ___variationColoredImages = (___variationColoredImages == null) ? new Image[0] : ___variationColoredImages;
            return true;
        }
    }
}
