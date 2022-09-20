using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    public class Focalyzer : UltraFunGunBase
    {
        public FocalyzerLaserController laser;
        public GameObject pylonPrefab;
        private Animator animator;

        bool throwingPylon = false;
        bool laserActive = false;



        private void Start()
        {
            HydraLoader.prefabRegistry.TryGetValue("FocalyzerPylon", out pylonPrefab);
            HydraLoader.prefabRegistry.TryGetValue("FocalyzerLaser", out GameObject laserPrefab);
            laser = GameObject.Instantiate<GameObject>(laserPrefab, Vector3.zero, Quaternion.identity).GetComponent<FocalyzerLaserController>();
            laser.focalyzer = this;
        }

        public override void GetInput()
        {
            if (MonoSingleton<InputManager>.Instance.InputSource.Fire1.IsPressed && !throwingPylon && actionCooldowns["fireLaser"].CanFire() && !om.paused)
            {
                laserActive = true;
                FireLaser();
            }else if (laserActive)
            {
                laserActive = false;
                actionCooldowns["fireLaser"].AddCooldown();
            }

            if (MonoSingleton<InputManager>.Instance.InputSource.Fire2.WasPerformedThisFrame && actionCooldowns["throwPylon"].CanFire())
            {
                if (!om.paused && laser.PylonCount() < 3 && !throwingPylon)
                {
                    ThrowPylon();
                }
            }
        }

        public override void DoAnimations()
        {
            laser.laserActive = laserActive;
        }

        public override void FireLaser()
        {
            RaycastHit hit;
            if (Physics.Raycast(mainCam.transform.position, mainCam.TransformDirection(0,0,1), out hit, 200.0f, 117460224))
            {
                
            }
        }

        public override void ThrowPylon()
        {
            throwingPylon = true;
            actionCooldowns["throwPylon"].AddCooldown();
            animator.Play("ThrowPylon");

        }

        public override Dictionary<string, ActionCooldown> SetActionCooldowns()
        {
            Dictionary<string, ActionCooldown> cooldowns = new Dictionary<string, ActionCooldown>();
            cooldowns.Add("fireLaser", new ActionCooldown(0.16f));
            cooldowns.Add("damageTick", new ActionCooldown(0.25f));
            cooldowns.Add("throwPylon", new ActionCooldown(1.0f));
            return cooldowns;
        }

        private void OnDisable()
        {
            laserActive = false;
            laser.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            laser.gameObject.SetActive(true);
        }
    }
}
