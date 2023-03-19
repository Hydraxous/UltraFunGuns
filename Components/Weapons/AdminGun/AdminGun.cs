using Mono.CompilerServices.SymbolWriter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem.HID;

namespace UltraFunGuns
{
    [WeaponAbility("Full-Auto 16x.50 AE", "Fire a continuous stream of 16 .50 AE cartridges simultaneously.", 0, RichTextColors.red)]
    [WeaponAbility("Full-Auto Explosives", "Fire a continuous stream of explosive bolts.", 1, RichTextColors.red)]
    [UFGWeapon("AdminGun","Sexyness", 0, true, WeaponIconColor.Red)]
    public class AdminGun : UltraFunGunBase
    {
        [UFGAsset] public static AudioClip AdminGun_FireSound { get; private set; }
        [UFGAsset("Wicked.prefab", true)] private static GameObject somethingWicked;
        [UFGAsset("GunClick1")] private static AudioClip switchFireModeSound;

        public int hitscans = 16;
        public float spread = 0.06f;

        private bool aimbot;

        private ActionCooldown primaryFire = new ActionCooldown(0.05f), secondaryFire = new ActionCooldown(0.05f);

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

            if (WeaponManager.SecretButton.WasPerformedThisFrame)
            {
                animator.Play("SelectFire", 0, 0);
                aimbot = !aimbot;
                switchFireModeSound.PlayAudioClip(firePoint.position, UnityEngine.Random.Range(0.8f, 1.1f), 1.0f, 0.0f);
            }

            if(Input.GetKeyDown(KeyCode.I))
            {
                if(HydraUtils.SphereCastMacro(mainCam.position,0.25f,mainCam.forward,300.0f, out RaycastHit hit))
                {
                    if(somethingWicked != null)
                    {
                        GameObject.Instantiate<GameObject>(somethingWicked, hit.point + Vector3.up, Quaternion.identity);
                    }
                }
            }
        }

        private void ShootGun()
        {
            if (aimbot)
            {
                if (HydraUtils.TryGetTarget(out Vector3 directionToTarget))
                {
                    HydraUtils.SetPlayerRotation(Quaternion.LookRotation(directionToTarget, Vector3.up));
                }
            }

            Vector3 lineStart = firePoint.position;
            for(int i = 0; i< hitscans; i++)
            {
                Ray newShot = GetRandomRaycast();

                if(HydraUtils.SphereCastAllMacro(newShot.origin, 0.15f, newShot.direction, Mathf.Infinity, out RaycastHit[] hits))
                {
                    hits = HydraUtils.SortRaycastHitsByDistance(hits);

                    for(int x=0; x < hits.Length; x++)
                    {
                        if (hits[x].collider.IsColliderEnemy(out EnemyIdentifier enemy))
                        {
                            if(enemy.enemyType == EnemyType.Wicked)
                            {
                                SonicReverberator.vineBoom_Loudest.PlayAudioClip(enemy.transform.position,1.3f, 1.0f, 0.7f);
                                if (Prefabs.SmackFX != null)
                                {
                                    Transform newFx = Instantiate<GameObject>(Prefabs.SmackFX, enemy.transform.position+Vector3.up*2.0f, Quaternion.identity).transform;
                                    newFx.localScale *= 3.0f;
                                }
                                Vector3 playerBump = NewMovement.Instance.transform.position - enemy.transform.position;
                                float dist = playerBump.magnitude;
                                dist = Mathf.Lerp(0.0f,200.0f,Mathf.InverseLerp(600.0f,0.0f,dist));
                                NewMovement.Instance.rb.velocity = playerBump.normalized*dist;
                                Destroy(enemy.gameObject);
                            }
                            else
                            {
                                enemy.DeliverDamage(hits[x].collider.gameObject, newShot.direction.normalized * 10000.0f, hits[x].point, 2.0f, false, 1.0f, gameObject);
                            }

                            Visualizer.DrawLine(1.0f, firePoint.position, hits[x].point);

                            /*
                            EnemyOverride enemyOverride = enemy.Override();
                            if (enemyOverride != null)
                            {
                                enemyOverride.SetRagdoll(true);
                                enemyOverride.Knockback(mainCam.forward * 50.0f);
                            }
                            */
                            //enemy.DeliverDamage(hit.collider.gameObject, newShot.direction * 5000f, hit.point, 2.0f, true, 2.0f, gameObject);
                        }

                        GameObject newHitDecal = GameObject.Instantiate(Prefabs.BulletImpactFX, hits[x].point + (hits[x].normal * 0.01f), Quaternion.identity);
                        newHitDecal.transform.parent = hits[x].collider.transform;
                        newHitDecal.transform.up = hits[x].normal;
                        Visualizer.DrawLine(1.0f, firePoint.position, hits[x].point);

                        if ((hits[x].collider.gameObject.layer == 24 || hits[x].collider.gameObject.layer == 25 || hits[x].collider.gameObject.layer == 8))
                        {
                            break;
                        }
                    }

                }

                //remove
                continue;

                if(HydraUtils.SphereCastMacro(newShot.origin, 0.15f, newShot.direction, Mathf.Infinity, out RaycastHit hit))
                {
                    if (hit.collider.IsColliderEnemy(out EnemyIdentifier enemy))
                    {
                        enemy.DeliverDamage(hit.collider.gameObject, newShot.direction.normalized * 10000.0f, hit.point, 2.0f, false, 1.0f, gameObject);

                        Visualizer.DrawLine(1.0f, firePoint.position, hit.point);

                        /*
                        EnemyOverride enemyOverride = enemy.Override();
                        if (enemyOverride != null)
                        {
                            enemyOverride.SetRagdoll(true);
                            enemyOverride.Knockback(mainCam.forward * 50.0f);
                        }
                        */
                        //enemy.DeliverDamage(hit.collider.gameObject, newShot.direction * 5000f, hit.point, 2.0f, true, 2.0f, gameObject);
                    }

                    GameObject newHitDecal = GameObject.Instantiate(Prefabs.BulletImpactFX, hit.point+(hit.normal*0.01f), Quaternion.identity);
                    newHitDecal.transform.parent = hit.collider.transform;
                    newHitDecal.transform.up = hit.normal;
                    Visualizer.DrawLine(1.0f, firePoint.position, hit.point);
                }
            }
            AdminGun_FireSound.PlayAudioClip(firePoint.position, UnityEngine.Random.Range(0.8f, 1.3f), 1.0f, 0.0f);
        }

        private void Boom()
        {
            Ray ray = mainCam.ToRay();
            if (HydraUtils.SphereCastMacro(ray.origin, 0.15f, ray.direction, Mathf.Infinity, out RaycastHit hit))
            {
                GameObject.Instantiate(Prefabs.UK_Explosion.Asset, hit.point, Quaternion.identity);
            }
            AdminGun_FireSound.PlayAudioClip(firePoint.position, UnityEngine.Random.Range(0.6f, 1.0f), 1.0f, 0.0f);
        }

        private Ray GetRandomRaycast()
        {
            Ray ray = new Ray();

            Vector3 randomDirection = UnityEngine.Random.insideUnitCircle*spread;
            randomDirection.z = 1;

            ray.origin = mainCam.position;
            ray.direction = mainCam.rotation * randomDirection;

            return ray;
        }

        private void OnEnable()
        {
            animator.Play("Equip", 0, 0);
        }
    }
}
