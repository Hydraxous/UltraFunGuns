using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    public abstract class UltraFunGunBase : MonoBehaviour
    {
        public Dictionary<string, ActionCooldown> actionCooldowns;

        public Transform mainCam, firePoint;
        public OptionsManager om;
        public NewMovement player;
        public WeaponIcon weaponIcon;

        public abstract void DoAnimations();

        private void Awake()
        {
            actionCooldowns = SetActionCooldowns();
            mainCam = MonoSingleton<CameraController>.Instance.transform;
            om = MonoSingleton<OptionsManager>.Instance;
            player = MonoSingleton<NewMovement>.Instance;
            foreach (Transform transf in gameObject.GetComponentsInChildren<Transform>(true))
            {
                if (transf.name == "firePoint")
                {
                    firePoint = transf;
                    break;
                }
            }

            HydraLoader.dataRegistry.TryGetValue(String.Format("{0}_weaponIcon", gameObject.name), out UnityEngine.Object weapon_weaponIcon);
            weaponIcon.weaponIcon = (Sprite) weapon_weaponIcon;

            HydraLoader.dataRegistry.TryGetValue(String.Format("{0}_glowIcon", gameObject.name), out UnityEngine.Object weapon_glowIcon);
            weaponIcon.glowIcon = (Sprite) weapon_glowIcon;

            weaponIcon.variationColor = 0; //TODO find a way to fix this

            if (weaponIcon.weaponIcon == null || weaponIcon.glowIcon == null)
            {
                HydraLoader.dataRegistry.TryGetValue("", out UnityEngine.Object debug_Icon);
                weaponIcon.weaponIcon = (Sprite)debug_Icon;
                weaponIcon.glowIcon = (Sprite)debug_Icon;
            }
        }
        
        private void Update()
        {
            GetInput();
            DoAnimations();
        }

        //Example input function call this in update.
        public virtual void GetInput()
        {
            if (MonoSingleton<InputManager>.Instance.InputSource.Fire1.WasPerformedThisFrame && actionCooldowns["primaryFire"].CanFire() && !om.paused)
            {
                actionCooldowns["primaryFire"].AddCooldown();
                FireLaser();
            }

            if (MonoSingleton<InputManager>.Instance.InputSource.Fire2.WasPerformedThisFrame && actionCooldowns["secondaryFire"].CanFire() && !om.paused)
            {
                actionCooldowns["secondaryFire"].AddCooldown();
                ThrowPylon();
            }
        }


        //Implement the cooldowns here.
        public virtual Dictionary<string, ActionCooldown> SetActionCooldowns()
        {
            Dictionary<string, ActionCooldown> cooldowns = new Dictionary<string, ActionCooldown>();
            cooldowns.Add("primaryFire", new ActionCooldown(1.0f));
            cooldowns.Add("secondaryFire", new ActionCooldown(1.0f));
            return cooldowns;
        }

        public virtual void FireLaser()
        {
            Debug.Log("Fired Primary! (not implemented)");
        }

        public virtual void ThrowPylon()
        {
            Debug.Log("Fired Secondary! (not implemented)");
        }

        public class ActionCooldown
        {
            float timeToFire;
            float fireDelay;
            public bool noCooldown;

            public ActionCooldown(float delay = 1f)
            {
                timeToFire = 0.0f;
                this.noCooldown = (delay <= 0.0f);
                this.fireDelay = delay;
            }            

            public void AddCooldown()
            {
                timeToFire = fireDelay + Time.time;
            }

            public void AddCooldown(float delayInSeconds)
            {
                timeToFire = delayInSeconds + Time.time;
            }

            public bool CanFire()
            {
                if(timeToFire < Time.time || noCooldown)
                {
                    return true;
                }
                return false;
            }
        }

    }
}
