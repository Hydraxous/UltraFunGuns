using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    [WeaponAbility("Heavy Shell", "SHOOT WITH <color=orange>Fire 1</color>", 0, RichTextColors.aqua)]
    [UFGWeapon("UltraGun", "ULTRAGUN", 3, true, WeaponIconColor.Green)]
    public class UltraGun : UltraFunGunBase
    {

        [UFGAsset("UltraBullet")] public static GameObject UltraBulletPrefab { get; private set; }
        [UFGAsset("BigGun_DeniedBoost")] private static AudioClip boostDenied_SFX;
        [UFGAsset("BigGun_Fire")] private static AudioClip fire_SFX;
        
        
        private ActionCooldown primaryFire = new ActionCooldown(0.30f, true);
        private ActionCooldown secondaryBoost = new ActionCooldown(0.5f, true);


        public float powerRestoreRate = 50.0f;
        public float minPower = 10.0f;
        public float maxPower = 125.0f;

        private float minPowerCost = 30.0f;

        private float power;

        private AbilityMeter powerDisplay;

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

        public override void OnAwakeFinished()
        {
            powerDisplay = GetComponentInChildren<AbilityMeter>();
        }

        //fired bullets take fuel to fly.
        private void Start()
        {
            Power = maxPower;
            powerDisplay.gameObject.SetActive(false);
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
                firedBullets = firedBullets.Where(x => x != null).Where(y => !y.Falling).ToList();

                if (firedBullets.Count <= 0)
                {
                    boostDenied_SFX.PlayAudioClip(UnityEngine.Random.Range(0.89f, 1.11f));
                }else
                {
                    DropAllBullets();
                }
            }

            if (WeaponManager.SecretButton.WasPerformedThisFrame && !om.paused)
            {
                animator?.Play("Inspect", 0, 0);
                DivideBullets();
            }

            if(Input.GetKeyDown(KeyCode.LeftBracket))
            {
                --divisions;
            }else if(Input.GetKeyDown(KeyCode.RightBracket))
            {
                ++divisions;
            }


            Power += (ULTRAKILL.Cheats.NoWeaponCooldown.NoCooldown) ? maxPower : powerRestoreRate * Time.deltaTime;
        }

        private void LateUpdate()
        {
            powerDisplay?.SetAmount(Power/maxPower);
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
                bullet.SetOriginWeapon(this);
                bullet.SetDirection(ray.direction);
                firedBullets.Add(bullet);
                bullet.SetPower(GetPower(bullet));
            }

            if (!player.gc.onGround)
            {
                player.rb.velocity += (-mainCam.forward * bulletPower * 0.25f) * Mathf.Max(Mathf.Abs(viewDot), 0.45f);
            } else if (viewDot >= 0.75f)
            {
                player.rb.velocity += (Vector3.Reflect(mainCam.forward, Vector3.up)* bulletPower*0.45f);
            }
        }

        private void DropAllBullets()
        {
            firedBullets = firedBullets.Where(x => x != null).Where(y=>!y.Falling).ToList();
            for (int i = 0; i < firedBullets.Count; i++)
            {
                firedBullets[i].Fall();
            }
        }

        private int divisions = 3;

        private void DivideBullets()
        {
            firedBullets = firedBullets.Where(x => x != null).ToList();
            for (int i = 0; i < firedBullets.Count; i++)
            {
                firedBullets[i].Divide(divisions);
            }
        }

        public float GetPower(UltraBullet bullet)
        {
            firedBullets = firedBullets.Where(x => x != null).Where(y => !y.Falling).ToList();

            if (firedBullets.Contains(bullet))
            {
                return maxPower / firedBullets.Count;
            }

            //Not on the list, geddaahdahere
            return 0.0f;
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
