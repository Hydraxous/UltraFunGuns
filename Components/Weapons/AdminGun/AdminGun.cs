using Mono.CompilerServices.SymbolWriter;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem.HID;

namespace UltraFunGuns
{
    [WeaponAbility("Full-Auto 16x.50 BMG", "Fire a continuous stream of 16 .50 BMG cartridges simultaneously.", 0, RichTextColors.red)]
    [WeaponAbility("Full-Auto Explosives", "Fire a continuous stream of explosive bolts.", 1, RichTextColors.red)]
    [FunGun("AdminGun","Admin Gun", 0, true, WeaponIconColor.Red)]
    public class AdminGun : UltraFunGunBase
    {
        //LoadedAssets
        [UFGAsset("Explosion.prefab", true)] private static GameObject explosion;
        [UFGAsset] public static AudioClip AdminGun_FireSound { get; private set; }
        [UFGAsset("Geme")] private static Material geme; 

        public int hitscans = 20;
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
                aimbot = !aimbot;
            }
        }

        private void OlGun()
        {

            HydraLogger.Log("Admin gun shooted", DebugChannel.Message);
            RaycastHit[] hits = Physics.RaycastAll(mainCam.position, mainCam.forward);
            foreach (RaycastHit hit in hits)
            {
                CheckHit(hit);
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
                GameObject.Instantiate(explosion, hit.point, Quaternion.identity);
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

        private void CheckHit(RaycastHit hit)
        {
            if(hit.transform.TryGetComponent<EnemyIdentifier>(out EnemyIdentifier eid))
            {
                eid.DeliverDamage(eid.gameObject, mainCam.forward * 10000, hit.point, 20000, true, 10000, gameObject);
            }

            if(hit.transform.TryGetComponent<EnemyIdentifierIdentifier>(out EnemyIdentifierIdentifier eidid))
            {
                eidid.eid.DeliverDamage(eidid.eid.gameObject, mainCam.forward * 10000, hit.point, 20000, true, 10000, gameObject);

            }
        }

        private void CreateBulletTrail(Vector3 startPosition, Vector3 endPosition, Vector3 normal)
        {
            if (Prefabs.BulletTrail == null)
            {
                return;
            }

            GameObject newBulletTrail = Instantiate<GameObject>(Prefabs.BulletTrail, endPosition, Quaternion.identity);
            newBulletTrail.transform.up = normal;
            LineRenderer line = newBulletTrail.GetComponent<LineRenderer>();
            Vector3[] linePoints = new Vector3[2] { startPosition, endPosition };
            line.SetPositions(linePoints);
        }

        private void OnEnable()
        {
            animator.Play("Equip", 0, 0);
        }
    }
}
