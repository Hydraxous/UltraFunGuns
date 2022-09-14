using BepInEx;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text;

namespace UltraFunGuns
{
    [BepInPlugin("Hydraxous.ULTRAKILL.UltraFunGuns", "UltraFunGuns", "1.0.0")]
    public class UltraFunGuns : BaseUnityPlugin
    {

        private void Awake()
        {
            if (HydraLoader.RegisterAll())
            {
                BindConfigs();
                Logger.LogInfo("UltraFunGuns Loaded.");
            }else
            {
                Logger.LogError("Unable to load mod components. Disabling.");
                gameObject.SetActive(false);
            }
        }

        private void BindConfigs()
        {

        }
    }
}