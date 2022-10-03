using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using HarmonyLib;

namespace UltraFunGuns
{
    //[HarmonyPatch(typeof(ShopZone), "Start")]
    public static class WeaponShopPatch
    {
        public static void Postfix(ShopZone __instance)
        {
            /*PLAN
             * 1. Make tip box smaller -> set tip box anchorMin.y to 0.2f
             * 2. Create button for UFG menu under the Main Menu as a parent.-> 
             Meh...
             */
        }


    }

    [HarmonyPatch(typeof(GunSetter),"ResetWeapons")]
    public static class GunSetterPatch
    {
        public static void Postfix(bool firstTime)
        {
            try
            {
                GameObject.FindObjectOfType<UFGWeaponManager>().DeployWeapons(firstTime);
            }
            catch (System.Exception e)
            {
                Debug.Log("UFG: Weapon deployment attempted.");
            }
            
        }

    }
}
