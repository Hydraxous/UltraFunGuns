using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.SceneManagement;

namespace UltraFunGuns
{
    [BepInPlugin("Hydraxous.ULTRAKILL.UltraFunGuns", "UltraFunGuns", "1.0.0")]
    public class UltraFunGuns : BaseUnityPlugin
    {
        UltraFunGunsPatch gunPatch;

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

        private void CheckWeapons()
        {
            if (gunPatch != null)
            {
                return;
            }

            //Check monosingleton of guncontrol contains the component UltraFunGunsPatch, if it doesnt add it. Else do nothing lmao
            GunControl gc = GameObject.FindObjectOfType<GunControl>();
            if (!gc.TryGetComponent<UltraFunGunsPatch>(out UltraFunGunsPatch ultraFGPatch))
            {
                gunPatch = gc.gameObject.AddComponent<UltraFunGunsPatch>();
            }
        }

        private bool InLevel()
        {
            string sceneName = SceneManager.GetActiveScene().name;
            if (sceneName == "Intro" || sceneName == "Main Menu")
            {
                return false;
            }
            return true;
        }

        private void BindConfigs()
        {

        }

        private void Update()
        {
            CheckWeapons();
        }
    }
}