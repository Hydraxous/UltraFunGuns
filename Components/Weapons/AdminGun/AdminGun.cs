using Mono.CompilerServices.SymbolWriter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UltraFunGuns.Datas;
using UnityEngine;
using UnityEngine.InputSystem.HID;

namespace UltraFunGuns
{
    [WeaponAbility("Full-Auto 16x.50 AE", "Fire a continuous stream of 16 .50 AE cartridges simultaneously.", 0, RichTextColors.red)]
    [WeaponAbility("Full-Auto Explosives", "Fire a continuous stream of explosive bolts.", 1, RichTextColors.red)]
    [UFGWeapon("AdminGun","Ultra***", 0, true, WeaponIconColor.Red)]
    public class AdminGun : UltraFunGunBase
    {
        [UFGAsset] public static AudioClip AdminGun_FireSound { get; private set; }
        public static UKAsset<GameObject> somethingWicked = new UKAsset<GameObject>("Assets/Prefabs/Enemies/Wicked.prefab");
        [UFGAsset("GunClick1")] private static AudioClip switchFireModeSound;

        [Configgy.Configgable("Weapons/Ultra***")]
        private static int hitscans = 16;
        
        [Configgy.Configgable("Weapons/Ultra***")]
        private static float bulletSpread = 0.06f;

        [Configgy.Configgable("Weapons/Ultra***")]
        private static bool gamerMode;

        [Configgy.Configgable("Weapons/Ultra***")]
        private static float primaryFireCooldown = 0.05f;
        
        [Configgy.Configgable("Weapons/Ultra***")]
        private static float secondaryFireCooldown = 0.05f;

        private ActionCooldown primaryFire = new ActionCooldown(primaryFireCooldown), secondaryFire = new ActionCooldown(secondaryFireCooldown);

        public override void GetInput()
        {
            if(MonoSingleton<InputManager>.Instance.InputSource.Fire1.IsPressed && !om.paused && primaryFire.CanFire())
            {
                primaryFire.AddCooldown();
                animator.Play("Fire", 0, 0);
                ShootGun();
            }

            if (MonoSingleton<InputManager>.Instance.InputSource.Fire2.IsPressed && !om.paused && secondaryFire.CanFire())
            {
                secondaryFire.AddCooldown();
                animator.Play("Fire", 0, 0);
                Boom();
            }

            if (UFGInput.SecretButton.WasPeformed())
            {
                animator.Play("SelectFire", 0, 0);
                gamerMode = !gamerMode;
                switchFireModeSound.PlayAudioClip(firePoint.position, UnityEngine.Random.Range(0.8f, 1.1f), 1.0f, 0.0f);
            }
        }

        private void ShootGun()
        {
            if (gamerMode)
            {
                if (EnemyTools.TryGetTarget(out Vector3 directionToTarget))
                {
                    HydraUtils.SetPlayerRotation(Quaternion.LookRotation(directionToTarget, Vector3.up));
                }
            }

            Vector3 lineStart = firePoint.position;

            for(int i = 0; i< hitscans; i++)
            {
                FireBullet(GetRandomRaycast());
            }

            AdminGun_FireSound.PlayAudioClip(firePoint.position, UnityEngine.Random.Range(0.8f, 1.3f), 1.0f, 0.0f);
        }

        private void FireBullet(Ray newShot, bool reflection = false)
        {
            if (!HydraUtils.SphereCastAllMacro(newShot.origin, 0.15f, newShot.direction, Mathf.Infinity, out RaycastHit[] hits))
            {
                return;
            }

            hits = HydraUtils.SortRaycastHitsByDistance(hits);

            int lastHit = 0;

            for (int x = 0; x < hits.Length; x++)
            {
                if (CheckCoin(hits[x]) && !reflection)
                {
                    Destroy(hits[x].collider.gameObject);
                    CoinReflect(hits[x].transform.position);
                    lastHit = x;
                    break;
                }

                if (hits[x].collider.gameObject.TryGetComponent<IUFGInteractionReceiver>(out IUFGInteractionReceiver receiver))
                {
                    lastHit = x;

                    UFGInteractionEventData eventData = new UFGInteractionEventData()
                    {
                        tags = new string[] { "shot", "god" },
                        direction = newShot.direction,
                        interactorPosition = newShot.origin,
                        invokeType = typeof(AdminGun),
                        power = 1000
                    };

                    if (receiver.Interact(eventData) && !gamerMode)
                    {
                        break;
                    }

                }

                if (hits[x].collider.TryGetComponent<Breakable>(out Breakable breakable))
                {
                    breakable.Break();
                }

                if (hits[x].collider.TryGetComponent<Glass>(out Glass glass))
                {
                    glass.Shatter();
                }

                if ((hits[x].collider.gameObject.layer == 24 || hits[x].collider.gameObject.layer == 25 || hits[x].collider.gameObject.layer == 8))
                {
                    lastHit = x;
                    if (!gamerMode)
                        break;
                }

                if (hits[x].collider.IsColliderEnemy(out EnemyIdentifier enemy))
                {
                    lastHit = x;
                    if (enemy.enemyType == EnemyType.Wicked)
                    {
                        SonicReverberator.vineBoom_Loudest.PlayAudioClip(enemy.transform.position, 1.3f, 1.0f, 0.7f);
                        if (Prefabs.SmackFX != null)
                        {
                            Transform newFx = Instantiate<GameObject>(Prefabs.SmackFX, enemy.transform.position + Vector3.up * 2.0f, Quaternion.identity).transform;
                            newFx.localScale *= 3.0f;
                        }
                        Vector3 playerBump = NewMovement.Instance.transform.position - enemy.transform.position;
                        float dist = playerBump.magnitude;
                        dist = Mathf.Lerp(0.0f, 200.0f, Mathf.InverseLerp(600.0f, 0.0f, dist));
                        NewMovement.Instance.rb.velocity = playerBump.normalized * dist;
                        Destroy(enemy.gameObject);
                    }
                    else
                    {
                        Action styleCallback = new Action(() => { WeaponManager.AddStyle((gamerMode) ? -500 : 0, "admingunkill", gameObject, enemy); });
                        enemy.gameObject.EnsureComponent<EnemyOverride>().AddDeathCallback(styleCallback);
                        enemy.DeliverDamage(hits[x].collider.gameObject, newShot.direction.normalized * 10000.0f, hits[x].point, 2.0f, false, 1.0f, gameObject);
                    }

                    Visualizer.DrawLine(1.0f, firePoint.position, hits[x].point);
                }
            }

            if(reflection)
            {
                HydraUtils.CreateBulletTrail(newShot.origin, hits[lastHit].point, hits[lastHit].normal);
                return;
            }

            GameObject newHitDecal = GameObject.Instantiate(Prefabs.BulletImpactFX, hits[lastHit].point + (hits[lastHit].normal * 0.01f), Quaternion.identity);
            newHitDecal.transform.parent = hits[lastHit].collider.transform;
            newHitDecal.transform.up = hits[lastHit].normal;
            Visualizer.DrawLine(1.0f, firePoint.position, hits[lastHit].point);
        }

        private bool CheckCoin(RaycastHit hit)
        {
            if(!hit.collider.gameObject.TryGetComponent<Coin>(out Coin coin))
            {
                coin = hit.collider.GetComponentInChildren<Coin>();
                if(coin == null)
                    return false;
            }

            if(coin.shot)
            {
                return false;
            }

            return true;
        }


        private void CoinReflect(Vector3 coinPosition)
        {
            EnemyIdentifier[] enemies = EnemyTracker.Instance.GetCurrentEnemies().Where(x=>(!x.dead && !x.blessed)).ToArray();

            Instantiate(Prefabs.ShittyExplosionFX, coinPosition, Quaternion.identity);
            Prefabs.ShittyExplosionSound?.PlayAudioClip(coinPosition, 1.3f, 1.0f, 0.7f);

            for (int i=0; i<enemies.Length; i++)
            {
                Ray fireRay = new Ray(coinPosition, enemies[i].GetTargetPoint() - coinPosition);
                FireBullet(fireRay, true);
            }
        }

        private void Boom()
        {
            Ray ray = mainCam.ToRay();
            if (HydraUtils.SphereCastMacro(ray.origin, 0.15f, ray.direction, Mathf.Infinity, out RaycastHit hit))
            {
                if (CheckCoin(hit))
                {
                    EnemyIdentifier[] enemies = EnemyTracker.Instance.GetCurrentEnemies().Where(x => (!x.dead && !x.blessed)).ToArray();

                    Vector3 coinPosition = hit.collider.transform.position;
                    Instantiate(Prefabs.ShittyExplosionFX, coinPosition, Quaternion.identity);
                    Prefabs.ShittyExplosionSound?.PlayAudioClip(coinPosition, 1.3f, 1.0f, 0.7f);

                    for (int i = 0; i < enemies.Length; i++)
                    {
                        Vector3 enemyPos = enemies[i].GetTargetPoint();
                        Vector3 direction = enemyPos - coinPosition;
                        if (!Physics.Raycast(coinPosition, direction, out RaycastHit explosionHit, Mathf.Infinity, LayerMaskDefaults.Get(LMD.EnemiesAndEnvironment)))
                            continue;

                        GameObject.Instantiate(Prefabs.UK_Explosion.Asset, explosionHit.point, Quaternion.identity);
                    }
                }
                else
                {
                    GameObject.Instantiate(Prefabs.UK_Explosion.Asset, hit.point, Quaternion.identity);
                }
            }

            AdminGun_FireSound.PlayAudioClip(firePoint.position, UnityEngine.Random.Range(0.6f, 1.0f), 1.0f, 0.0f);
        }

        private Ray GetRandomRaycast()
        {
            Ray ray = new Ray();

            Vector3 randomDirection = UnityEngine.Random.insideUnitCircle*bulletSpread;
            randomDirection.z = 1;

            ray.origin = mainCam.position;
            ray.direction = mainCam.rotation * randomDirection;

            return ray;
        }

        private void OnEnable()
        {
            animator.Play("Equip", 0, 0);
        }

        public override string GetDebuggingText()
        {
            string debug = base.GetDebuggingText();
            debug += $"GAMER_MODE: {gamerMode}\n";
            return debug;
        }
    }
}
