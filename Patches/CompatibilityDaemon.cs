using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UltraFunGuns;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UltraFunGuns
{
    public class CompatibilityDaemon : MonoBehaviour
    {
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            CompatibilityCheck.CheckCompatibility(CheckComplete);
        }

        private void CheckComplete()
        {
            if (!CompatibilityCheck.Incompatible)
            {
                Destroy(gameObject);
                return;
            }

            WeaponManager.OnWeaponsDeployed += SendCompatMessage;
        }

        private void SendCompatMessage(UFGWeapon[] weapons)
        {
            if (weapons.Length == 0)
                return;

            WeaponManager.RemoveAllWeapons();
            HudMessageReceiver.Instance.SendHudMessage($"<color=red>UltraFunGuns IS NOT COMPATIBLE WITH {CompatibilityCheck.IncompatibleOffender}</color>");
        }
    }
}

