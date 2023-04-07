using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    [UFGWeapon("UltraGun", "ULTRAGUN", 0, true, WeaponIconColor.Blue, false)]
    public class UltraGun : UltraFunGunBase
    {

        [UFGAsset("UltraBullet")] public static GameObject UltraBulletPrefab { get; private set; }
        private ActionCooldown primaryFire = new ActionCooldown(0.30f, true);


        public float powerRestoreRate = 50.0f;
        public float minPower = 10.0f;
        public float maxPower = 200.0f;

        private float minPowerCost = 30.0f;

        private float power;
        public float Power
        {
            get
            {
                return power;
            }

            set
            {
                power = Mathf.Clamp(value, minPower, maxPower);
            }
        }

        public override void GetInput()
        {
            if(InputManager.Instance.InputSource.Fire1.WasPerformedThisFrame && primaryFire.CanFire() && !om.paused)
            {
                primaryFire.AddCooldown();
                Fire();
            }

            Power += powerRestoreRate * Time.deltaTime;
        }

        private void Fire()
        {

            if (UltraBulletPrefab == null)
                return;

            float bulletPower = Power / 2.0f;
            if(bulletPower <= minPowerCost)
            {
                return;
            }

            SonicReverberator.vineBoom_Loud.PlayAudioClip();
            animator.Play("Fire", 0, 0);

            Ray ray = HydraUtils.GetProjectileAimVector(mainCam, firePoint, 0.85f, 20000f);

            if (Instantiate(UltraBulletPrefab, firePoint.position, Quaternion.identity).TryGetComponent<UltraBullet>(out UltraBullet bullet))
            {
                Power -= bulletPower;

                bullet.SetPower(bulletPower*1.5f);
                bullet.SetDirection(ray.direction);
            }
        }

        private void OnEnable()
        {
            animator.Play("Equip", 0, 0);
        }

        public override string GetDebuggingText()
        {
            string debug = base.GetDebuggingText();

            debug += $"POWER: {Power}";
            return debug;
        }
    }
}
