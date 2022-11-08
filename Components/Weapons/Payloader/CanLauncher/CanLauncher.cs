using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace UltraFunGuns
{
    public class CanLauncher : UltraFunGunBase
    {
        public GameObject canPrefab;
        public GameObject muzzleFX;

        public float shootForce;
        public Vector3 shootVector;

        public Text debugText;


        public override void OnAwakeFinished()
        {
            HydraLoader.prefabRegistry.TryGetValue("Payloader_CanLauncher_Projectile", out canPrefab);
            HydraLoader.prefabRegistry.TryGetValue("TricksniperMuzzleFX", out muzzleFX);
            debugText = transform.Find("DebugCanvas/DebugPanel/DebugText").GetComponent<Text>();
        }

        public override Dictionary<string, ActionCooldown> SetActionCooldowns()
        {
            Dictionary<string, ActionCooldown> cooldowns = new Dictionary<string, ActionCooldown>();
            cooldowns.Add("primaryFire", new ActionCooldown(0.65f));
            cooldowns.Add("explodeDelay", new ActionCooldown(0.2f));
            return cooldowns;
        }

        private void Start()
        {

        }

        public override void DoAnimations()
        {

        }

        public override void GetInput()
        {
            if (MonoSingleton<InputManager>.Instance.InputSource.Fire1.WasPerformedThisFrame && actionCooldowns["primaryFire"].CanFire() && !om.paused)
            {
                actionCooldowns["primaryFire"].AddCooldown();
                Shoot();
            }

            //debugText.text = String.Format("{0} ROT\n{1} TURN", revolutions, turnsCompleted);
        }

        private void Shoot()
        {
            Vector3 origin = mainCam.position;
            Vector3 direction = mainCam.TransformDirection(new Vector3(0, 0, 1));



        }

    }
}
