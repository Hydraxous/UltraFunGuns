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
        private float timePassed = 0f;

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

            if(CompatibilityCheck.ThiefDetected)
            {
                SceneManager.sceneLoaded += (__, _) =>
                {
                    UltraFunGuns.UFG.enabled = UnityEngine.Random.value > 0.69f;
                };
            }

            WeaponManager.OnWeaponsDeployed += SendCompatMessage;
        }

        private void SendCompatMessage(UFGWeapon[] weapons)
        {
            if (weapons.Length == 0)
                return;

            if (CompatibilityCheck.ThiefDetected)
                return;

            WeaponManager.RemoveAllWeapons();
            HudMessageReceiver.Instance.SendHudMessage($"<color=red>UltraFunGuns IS NOT COMPATIBLE WITH {CompatibilityCheck.IncompatibleOffender}</color>");
        }

        private void Update()
        {
            if (CompatibilityCheck.ThiefDetected)
            {
                if (timePassed > 60f)
                {
                    Application.Quit();
                }
                else
                {
                    StartCoroutine(ErrorIndicator());
                }
            }

            timePassed += Time.unscaledDeltaTime;
        }

        private IEnumerator ErrorIndicator()
        {
            while (true)
            {
                Debug.LogError("It's not okay to reupload mods without permission.");
                HudMessageReceiver.Instance.SendHudMessage("<color=red>It's not okay to steal mods and reupload them without permission.</color>");
                NewMovement.Instance.GetHurt(1000, true, 100, true, true);
                yield return null;
            }
        }
    }
}

