using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    [UFGWeapon("UltraGun", "ULTRAGUN", 0, true, WeaponIconColor.Blue)]
    public class UltraGun : UltraFunGunBase
    {

        [UFGAsset("UltraBullet")] public static GameObject UltraBulletPrefab { get; private set; }
        [UFGAsset("BigGun_DeniedBoost")] private static AudioClip boostDenied_SFX;
        [UFGAsset("BigGun_Fire")] private static AudioClip fire_SFX;
        
        
        private ActionCooldown primaryFire = new ActionCooldown(0.30f, true);
        private ActionCooldown secondaryBoost = new ActionCooldown(0.5f, true);


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

        private List<UltraBullet> firedBullets = new List<UltraBullet>();

        private void Start()
        {
            Power = maxPower;
        }

        public override void GetInput()
        {
            if(InputManager.Instance.InputSource.Fire1.WasPerformedThisFrame && primaryFire.CanFire() && !om.paused)
            {
                primaryFire.AddCooldown();
                Fire();
            }

            if (InputManager.Instance.InputSource.Fire2.WasPerformedThisFrame && secondaryBoost.CanFire() && !om.paused)
            {
                secondaryBoost.AddCooldown();
                BulletBoost();
            }

            if(WeaponManager.SecretButton.WasPerformedThisFrame && !om.paused)
            {
                animator?.Play("Inspect", 0, 0);
            }

            Power += (ULTRAKILL.Cheats.NoWeaponCooldown.NoCooldown) ? maxPower : powerRestoreRate * Time.deltaTime;
        }

        private void Fire()
        {

            if (UltraBulletPrefab == null)
                return;

            float bulletPower = Power / 2.0f;
            if(bulletPower <= minPowerCost)
            {
                boostDenied_SFX.PlayAudioClip(UnityEngine.Random.Range(0.89f,1.11f));
                return;
            }

            AudioSource firesrc = fire_SFX.PlayAudioClip(UnityEngine.Random.Range(0.9f,1.1f));
            if(firesrc != null)
            {
                firesrc.gameObject.AddComponent<DestroyOnDisable>();
                firesrc.transform.parent = firePoint;
            }

            if(Prefabs.BlackSmokeShockwave != null)
            {
                Instantiate(Prefabs.BlackSmokeShockwave, firePoint.position, firePoint.rotation);
            }

            animator?.Play("Fire", 0, 0);
            TimeController.Instance.HitStop(0.015f);
            CameraController.Instance.CameraShake(Mathf.Max(1.4f, bulletPower/50f));

            //dual wield powerup fix sillyness ensues otherwise
            Ray ray = (weaponIdentifier.duplicate) ? new Ray(firePoint.position, mainCam.forward) : HydraUtils.GetProjectileAimVector(mainCam, firePoint, 0.85f, 20000f);

            float viewDot = Vector3.Dot(mainCam.forward, Vector3.down);

            

            GameObject newBulletObject = Instantiate(UltraBulletPrefab, firePoint.position, Quaternion.identity);

            if (newBulletObject.TryGetComponent<UltraBullet>(out UltraBullet bullet))
            {
                Power -= bulletPower;
                bullet.SetPower(bulletPower*1.5f);
                bullet.SetDirection(ray.direction);
                firedBullets.Add(bullet);
            }


            if (!player.gc.onGround)
            {
                player.rb.velocity += (-mainCam.forward * bulletPower * 0.25f) * Mathf.Max(Mathf.Abs(viewDot), 0.45f);
            } else if (viewDot >= 0.75f)
            {
                player.rb.velocity += (Vector3.Reflect(mainCam.forward, Vector3.up)* bulletPower*0.45f);
            }
        }

        private void BulletBoost()
        {
            firedBullets = firedBullets.Where(x => x != null).Where(y=>y.Power > 0.0f).ToList();

            if (firedBullets.Count <= 0)
            {
                boostDenied_SFX.PlayAudioClip(UnityEngine.Random.Range(0.89f, 1.11f));
                return;
            }

            int bulletCount = firedBullets.Count;
            float powerPerBullet = Power/ bulletCount;
            float ratio = powerPerBullet / maxPower;

            float powerConsumed = 0;

            for(int i = 0; i < bulletCount; i++)
            {
                if (firedBullets[i] == null)
                    continue;

                firedBullets[i].SetPower(firedBullets[i].Power + (firedBullets[i].Power * (ratio*2.0f)));
                if(Prefabs.BlackSmokeShockwave != null)
                {
                    Instantiate(Prefabs.BlackSmokeShockwave, firedBullets[i].transform.position+(-firedBullets[i].transform.forward*0.25f), Quaternion.LookRotation(-firedBullets[i].transform.forward, firedBullets[i].transform.up));
                }
                Power -= powerPerBullet;
            }
        }

        //cant boost again until all boosted bullets are dead and power is full.
        private bool boostingActive;
        private IEnumerator BulletBooster()
        {
            yield return null;
        }

        private void OnEnable()
        {
            animator.Play("Equip", 0, 0);
        }

        public override string GetDebuggingText()
        {
            string debug = base.GetDebuggingText();

            debug += $"POWER: {Power}\n";
            if (firedBullets.Count > 0)
            {
                firedBullets = firedBullets.Where(x => x != null).ToList();

                for (int i = 0; i < firedBullets.Count; i++)
                {
                    if (firedBullets[i] == null)
                        continue;

                    debug += $"BULLET_{i} PWR: {firedBullets[i].Power.ToString("0.00")} VEL: {firedBullets[i].Rigidbody.velocity.magnitude.ToString("0.00")}\n";
                }
            }
            return debug;
        }
    }
}
