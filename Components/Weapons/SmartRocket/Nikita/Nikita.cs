using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    [UFGWeapon("Nikita", "Nikita", 0, true, WeaponIconColor.Green, false)]
    public class Nikita : UltraFunGunBase
    {
        [UFGAsset("NikitaRocket")] private static GameObject rocketPrefab;
        private NikitaRocket activeRocket;

        private ActionCooldown rocketCooldown = new ActionCooldown(1.4f, true);

        private Quaternion playerLook;

        public bool RocketActive => activeRocket != null;

        public override void GetInput()
        {
            if(InputManager.Instance.InputSource.Fire1.WasPerformedThisFrame && !om.paused)
            {
                if (activeRocket == null)
                {
                    if (rocketCooldown.CanFire())
                        FireRocket();
                }else
                {
                    ControlRocket();
                }
            }

            if(activeRocket)
            {
                if(activeRocket.Controlled)
                {
                    HydraUtils.SetPlayerRotation(playerLook);
                    //Stop player movement.
                }
            }
        }

        private void FireRocket()
        {
            if (rocketPrefab == null)
                return;

            playerLook = CameraController.Instance.transform.rotation;

            rocketCooldown.AddCooldown();
            activeRocket = GameObject.Instantiate(rocketPrefab, (mainCam.transform.forward*0.25f)+mainCam.transform.position, mainCam.transform.rotation).GetComponent<NikitaRocket>();
        }

        private void ControlRocket()
        {
            if (activeRocket == null)
                return;

            if (activeRocket.Controlled)
                return;

            playerLook = CameraController.Instance.transform.rotation;
            activeRocket.SetControlState(true, true);
        }

        public override string GetDebuggingText()
        {
            string debug = base.GetDebuggingText();
            debug += $"ROCKET_ACTIVE: {RocketActive}\n";
            if(RocketActive)
            {
                debug += $"ROCKET_SPEED: {activeRocket.currentSpeed}\n";
                debug += $"ROCKET_CONTROL: {activeRocket.Controlled}\n";
                debug += $"R_LOOK: {activeRocket.lookInput}\n";
                debug += $"R_MOVE: {activeRocket.moveInput}\n";
            }
            return debug;
        }
    }
}
