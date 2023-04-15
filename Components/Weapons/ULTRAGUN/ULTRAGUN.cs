using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    [WeaponAbility("Heavy Shell", "SHOOT WITH <color=orange>Fire 1</color>", 0, RichTextColors.aqua)]
    [WeaponAbility("Barrage", "Hold <color=orange>Fire 2</color> and release for a barrage of shells.", 1, RichTextColors.lime)]
    [WeaponAbility("TX-Fuel", "The last shell fired will maintain it's initial speed.", 2, RichTextColors.yellow)]
    [UFGWeapon("UltraGun", "ULTRAGUN", 3, true, WeaponIconColor.Green)]
    public class UltraGun : UltraFunGunBase
    {

        [UFGAsset("UltraBullet")] public static GameObject UltraBulletPrefab { get; private set; }
        [UFGAsset("BigGun_DeniedBoost")] private static AudioClip boostDenied_SFX;
        [UFGAsset("BigGun_Fire")] private static AudioClip fire_SFX;
        
        
        private ActionCooldown primaryFire = new ActionCooldown(0.5f, true);
        private ActionCooldown secondaryBoost = new ActionCooldown(0.5f, true);


        public float powerRestoreRate = 50.0f;
        public float minPower = 10.0f;
        public float maxPower = 80.0f;

        private float minPowerCost = 30.0f;

        private float power;

        private float barrageChargeMin = 0.0f, barrageChargeMax = 1.15f, barrageChargeMultiplier = 1.0f;
        private float barrageCharge;

        private int barrageAmount = 3;

        private float barrageFireDelay = 0.15f;

        private AbilityMeter powerDisplay;
        private Vibrate vibrate;
        private AudioSource chargingAudioSrc;
        [UFGAsset("Charge_Loop_Alternate")] private static AudioClip chargingLoopClip;

        private bool canBarrage => barrageCharge > (barrageChargeMax / barrageAmount) && !barraging;

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

        private UltraBullet lastBullet;

        //fired bullets take fuel to fly.
        private void Start()
        {
            chargingAudioSrc = gameObject.AddComponent<AudioSource>();
            chargingAudioSrc.clip = chargingLoopClip;
            chargingAudioSrc.playOnAwake = true;
            chargingAudioSrc.loop = true;
            chargingAudioSrc.Play();
            vibrate = gameObject.GetComponentInChildren<Vibrate>();
            powerDisplay = GetComponentInChildren<AbilityMeter>();
            Power = maxPower;
        }

        public override void GetInput()
        {
            if (om.paused || barraging)
                return;

            if(InputManager.Instance.InputSource.Fire1.WasPerformedThisFrame && (primaryFire.CanFire() || canBarrage))
            {
                if(canBarrage)
                {
                    Barrage();
                    return;
                }
                primaryFire.AddCooldown();
                Ray ray = (IsDuplicate) ? new Ray(firePoint.position, mainCam.forward) : HydraUtils.GetProjectileAimVector(mainCam, firePoint, 0.85f, 20000f);
                Fire(ray.direction);
            }

            if (InputManager.Instance.InputSource.Fire2.IsPressed)
            {
                barrageCharge = Mathf.Clamp(barrageCharge + Time.deltaTime * barrageChargeMultiplier, barrageChargeMin, barrageChargeMax);
            }
            else if (canBarrage)
            {
                Barrage();
            }
            else
            {
                barrageCharge = Mathf.Clamp(barrageCharge - (Time.deltaTime * barrageChargeMultiplier), barrageChargeMin, barrageChargeMax);
            }

            if (WeaponManager.SecretButton.WasPerformedThisFrame && !om.paused)
            {
                animator?.Play("Inspect", 0, 0);
                //DivideBullets();
            }

            //Power += (ULTRAKILL.Cheats.NoWeaponCooldown.NoCooldown) ? maxPower : powerRestoreRate * Time.deltaTime;
        }

        private void LateUpdate()
        {
            if (barrageAmount <= 0)
                barrageAmount = 1;

            float chargeInterval = (barrageCharge / barrageChargeMax);

            //powerDisplay?.SetAmount(fill/ratio);
            powerDisplay?.SetAmount((chargeInterval));
            if (vibrate != null)
            {
                vibrate.intensity = Mathf.Lerp(0.0f,0.015f, chargeInterval);
            }

            animator?.SetBool("Cooldown", (!primaryFire.CanFire()));

            if(chargingAudioSrc != null)
            {
                chargingAudioSrc.pitch = Mathf.Lerp(0.0f, 0.9f, chargeInterval);
                chargingAudioSrc.volume = (chargeInterval > 0.0f) ? 1 : 0;
            }
        }

        private UltraBullet Fire(Vector3 aimDirection)
        {

            if (UltraBulletPrefab == null)
                return null;

            float bulletPower = Power / 2.0f;

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

            float viewDot = Vector3.Dot(mainCam.forward, Vector3.down);

            

            GameObject newBulletObject = Instantiate(UltraBulletPrefab, firePoint.position, Quaternion.identity);

            if (newBulletObject.TryGetComponent<UltraBullet>(out UltraBullet bullet))
            {
                bullet.SetOriginWeapon(this);
                bullet.SetDirection(aimDirection);
                firedBullets.Add(bullet);
                lastBullet = bullet;
                bullet.SetPower(GetPower(bullet));
            }

            if (!player.gc.onGround)
            {
                player.rb.velocity += (-mainCam.forward * bulletPower * 0.25f) * Mathf.Max(Mathf.Abs(viewDot), 0.45f);
            } else if (viewDot >= 0.75f)
            {
                player.rb.velocity += (Vector3.Reflect(mainCam.forward, Vector3.up)* bulletPower*0.45f);
            }

            return bullet;
        }

        private void Barrage()
        {
            if(!barraging)
            {
                barraging = true;
                StartCoroutine(DoBarrage());
            }
        }

        private bool barraging;

        private IEnumerator DoBarrage()
        {
            int barrageCount = Mathf.RoundToInt((float)barrageAmount * (barrageCharge / barrageChargeMax));

            List<Collider> bulletColliders = new List<Collider>();

            for (int i = 0; i < barrageCount; i++)
            {
                UltraBullet bullet = Fire(mainCam.forward + ((mainCam.rotation * UnityEngine.Random.insideUnitCircle) * 0.085f));
                bullet.SetPower(125f);
                bullet.SetPower(3f);//Want velocity but to fall, its jank idc.
                bullet.SetOriginWeapon(null);
                barrageCharge = Mathf.Clamp( barrageCharge-(barrageChargeMax / barrageAmount), barrageChargeMin, barrageChargeMax);
                Collider[] col = bullet.GetComponentsInChildren<Collider>();
                for(int x=0; x<col.Length; x++)
                {
                    if (col[x] == null)
                        continue;

                    foreach(Collider shotCollider in bulletColliders)
                    {
                        if (shotCollider == null)
                            continue;

                        Physics.IgnoreCollision(shotCollider, col[x], true);
                    }
                }

                bulletColliders.AddRange(col);
                yield return new WaitForSeconds(barrageFireDelay);
            }

            primaryFire.AddCooldown(primaryFire.FireDelay * 2.0f);
            barraging = false;

            yield return new WaitForSeconds(0.31666f);
            animator.Play("Cooldown", 0, 0);
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
            if (bullet != lastBullet)
                return 0.0f;

            return maxPower;

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

        private void OnDisable()
        {
            barrageCharge = barrageChargeMin;
            barraging = false;
        }

        public override string GetDebuggingText()
        {
            string debug = base.GetDebuggingText();

            debug += $"POWER: {Power}\n";
            debug += $"BARRAGE: {barrageCharge}\n";
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
