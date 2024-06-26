﻿using Configgy;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UltraFunGuns
{
    [WeaponAbility("Finger Blast", "Press <color=orange>Fire 1</color> to shoot an explosive bolt from your finger that can penetrate enemies.",0,RichTextColors.aqua)]
    [WeaponAbility("Air-Denial", "Weapon can destroy projectiles.",1,RichTextColors.yellow)]
    [UFGWeapon("FingerGun","Hand Gun", 0, true, WeaponIconColor.Red)]
    public class FingerGun : UltraFunGunBase
    {
        [UFGAsset("BulletPierceTrail")] private static GameObject bulletTrailPrefab;
        [UFGAsset("FingerGun_ImpactExplosion")] private static GameObject hitExplodeFX;

        private int currentAmmo = 8;
        public int CurrentAmmo
        {
            get { return currentAmmo; }

            set
            {
                currentAmmo = value;
                if (ammoCounter != null)
                {
                    ammoCounter.text = currentAmmo.ToString();
                }
            }
        }


        [Configgable("Weapons/Handgun")]
        public static int maxAmmo = 8;

        [Configgable("Weapons/Handgun")]
        public static int penetrations = 4;

        [Configgable("Weapons/Handgun")]
        public static float forceMultiplier = 5.0f;


        [Configgable("Weapons/Handgun")]
        public static float damageMultipler = 2.2f;

        [Configgable("Weapons/Handgun")]
        public static float explosionRadius = 2.5f;
        
        [Configgable("Weapons/Handgun")]
        public static float explosionDamageMultiplier = 0.5f;

        private bool reloading = false;
        private bool shooting = false;


        [Configgable("Weapons/Handgun")]
        public static float hitRayWidth = 0.5f;

        [Configgable("Weapons/Handgun")]
        private static float maxRange = 350.0f;

        private Text ammoCounter;

        public override void OnAwakeFinished()
        {
            AddSFX("BangSound", "GunReady", "Reload", "Kabooma");
            ammoCounter = transform.GetComponentInChildren<Text>();

        }

        public override void GetInput()
        {
            if (CurrentAmmo < 1 && !reloading && !shooting)
            {
                StartCoroutine(Reload());
            }

            if (MonoSingleton<InputManager>.Instance.InputSource.Fire1.WasPerformedThisFrame && !om.paused && !reloading && !shooting && (CurrentAmmo > 0 || ULTRAKILL.Cheats.NoWeaponCooldown.NoCooldown))
            {
                StartCoroutine(Shoot());
            }

            if (MonoSingleton<InputManager>.Instance.InputSource.Fire2.WasPerformedThisFrame && !om.paused && !reloading && !shooting && CurrentAmmo < maxAmmo)
            {
                StartCoroutine(Reload());
            }

        }

        IEnumerator Shoot()
        {
            shooting = true;
            animator.Play("Shoot", 0, 0.0f);
            //bang.pitch = UnityEngine.Random.Range(0.85f, 1.0f); TODO
            //bang.Play();
            PlaySFX("BangSound", 0.85f, 1.0f);
            if(!ULTRAKILL.Cheats.NoWeaponCooldown.NoCooldown)
                --CurrentAmmo;

            Ray direction = new Ray();
            direction.origin = mainCam.transform.position;
            direction.direction = mainCam.TransformDirection(0, 0, 1);

            bool penetration = true;

            List<EnemyIdentifier> hitEnemies = new List<EnemyIdentifier>();

            RaycastHit[] hits = Physics.SphereCastAll(direction, hitRayWidth, maxRange, LayerMask.GetMask("Limb", "BigCorpse", "Outdoors", "Environment", "Default", "Projectile"));
            hits = HydraUtils.SortRaycastHitsByDistance(hits);
            if (hits.Length > 0)
            {
                if (!(hits.Length == 1 && hits[0].collider.gameObject.name == "CameraCollisionChecker"))
                {
                    int endingHit = 0;
                    hits = HydraUtils.SortRaycastHitsByDistance(hits);
                    for (int i = 0; i < hits.Length; i++)
                    {
                        Ray hitRay = new Ray();
                        hitRay.origin = mainCam.transform.position;
                        hitRay.direction = hits[i].point - mainCam.transform.position;

                        if (hitEnemies.Count > penetrations)
                        {
                            penetration = false;
                        }

                        endingHit = i;

                        if ((hits[i].collider.gameObject.layer == 24 || hits[i].collider.gameObject.layer == 25 || hits[i].collider.gameObject.layer == 8))
                        {
                            break;
                        }

                        if (hits[i].collider.gameObject.TryGetComponent<EnemyIdentifierIdentifier>(out EnemyIdentifierIdentifier enemyIDID))
                        {
                            if (!hitEnemies.Contains(enemyIDID.eid))
                            {
                                hitEnemies.Add(enemyIDID.eid);
                                enemyIDID.eid.DeliverDamage(hits[i].collider.gameObject, hitRay.direction * forceMultiplier, hits[i].point, damageMultipler, true, 0.0f, this.gameObject);
                                GoBoom(hits[i]);
                            }
                            if (!penetration)
                            {
                                break;
                            }
                        }
                        else if (hits[i].collider.gameObject.TryGetComponent<EnemyIdentifier>(out EnemyIdentifier enemyID))
                        {
                            if (!hitEnemies.Contains(enemyID))
                            {
                                hitEnemies.Add(enemyID);
                                enemyID.DeliverDamage(hits[i].collider.gameObject, hitRay.direction * forceMultiplier, hits[i].point, damageMultipler, true, 0.0f, this.gameObject);
                                GoBoom(hits[i]);
                            }
                            if (!penetration)
                            {
                                break;
                            }
                        }

                        if (hits[i].collider.gameObject.TryGetComponent<Breakable>(out Breakable breakable))
                        {
                            breakable.Break();
                            if (!penetration)
                            {
                                break;
                            }
                        }

                        if (hits[i].collider.gameObject.TryGetComponent<IUFGInteractionReceiver>(out IUFGInteractionReceiver ufgInteractable))
                        {
                            ufgInteractable.Interact(new UFGInteractionEventData()
                            {
                                tags = new string[] {"explode", "shot", "heavy"},
                                direction = hitRay.direction,
                                interactorPosition = hitRay.origin,
                                power = 2.0f,
                                invokeType = GetType()
                            });
                        }

                        if (hits[i].collider.gameObject.TryGetComponent<ThrownDodgeball>(out ThrownDodgeball dodgeBall))
                        {
                            dodgeBall.ExciteBall(1);
                            if (!penetration)
                            {
                                break;
                            }
                        }

                        if (hits[i].collider.gameObject.TryGetComponent<Grenade>(out Grenade grenade))
                        {
                            MonoSingleton<TimeController>.Instance.ParryFlash();
                            grenade.Explode();
                            if (!penetration)
                            {
                                break;
                            }
                        }

                        if (hits[i].collider.gameObject.TryGetComponent<Projectile>(out Projectile projectile))
                        {
                            projectile.Explode();
                            if (!projectile.friendly)
                            {
                                MonoSingleton<TimeController>.Instance.ParryFlash();
                                MonoSingleton<StyleHUD>.Instance.AddPoints(10, "hydraxous.ultrafunguns.fingergunprojhit", this.gameObject, null);
                            }
                            if (!penetration)
                            {
                                break;
                            }
                        }
                    }
                    CreateBulletTrail(firePoint.position, hits[endingHit].point, hits[endingHit].normal, hits[endingHit].collider.transform);
                }
                else//todo clean this.
                {
                    Ray missray = new Ray();
                    missray.origin = mainCam.transform.position;
                    missray.direction = mainCam.TransformDirection(0, 0, 1);

                    Vector3 endPoint = missray.GetPoint(maxRange);
                    CreateBulletTrail(firePoint.position, endPoint, missray.direction * -1, null);
                }
            }
            else
            {
                Ray missray = new Ray();
                missray.origin = mainCam.transform.position;
                missray.direction = mainCam.TransformDirection(0, 0, 1);

                Vector3 endPoint = missray.GetPoint(maxRange);
                CreateBulletTrail(firePoint.position, endPoint, missray.direction * -1, null);
            }

            List<EnemyIdentifier> hitListCopy = new List<EnemyIdentifier>();
            foreach (EnemyIdentifier enemee in hitEnemies)
            {
                if (!enemee.dead && !enemee.exploded)
                {
                    hitListCopy.Add(enemee);
                }
            }
            hitEnemies = hitListCopy;

            if (hitEnemies.Count > penetrations)
            {
                //kabooma.Play(); TODO
                PlaySFX("Kabooma");
                MonoSingleton<TimeController>.Instance.HitStop(0.10f);
                MonoSingleton<StyleHUD>.Instance.AddPoints(250, "hydraxous.ultrafunguns.fingergunfullpenetrate", this.gameObject, null);
            }
            else if (hitEnemies.Count > 0)
            {
                MonoSingleton<StyleHUD>.Instance.AddPoints(10 * hitEnemies.Count, "hydraxous.ultrafunguns.fingergunhit", this.gameObject, null, hitEnemies.Count);
            }

            yield return new WaitForSeconds(0.36f);
            //readyClick.pitch = UnityEngine.Random.Range(0.95f, 1.0f); TODO
            //readyClick.Play();
            PlaySFX("GunReady", 0.95f, 1.0f);
            shooting = false;
        }

        IEnumerator Reload()
        {
            reloading = true;
            animator.Play("Reload");
            yield return new WaitForSeconds(0.4f);
            //reload.pitch = UnityEngine.Random.Range(0.85f, 1.0f);
            //reload.Play(); TODO
            PlaySFX("Reload", 0.85f, 1.0f);
            CurrentAmmo = maxAmmo;
            yield return new WaitForSeconds(0.66f);
            reloading = false;
        }

        protected override void DoAnimations()
        {

        }

        private void CreateBulletTrail(Vector3 startPosition, Vector3 endPosition, Vector3 normal, Transform parent)
        {
            GoBoom(endPosition, normal);
            GameObject newBulletTrail = Instantiate<GameObject>(bulletTrailPrefab, endPosition, Quaternion.identity);
            newBulletTrail.transform.up = normal;
            LineRenderer line = newBulletTrail.GetComponent<LineRenderer>();
            Vector3[] linePoints = new Vector3[2] { startPosition, endPosition };
            line.SetPositions(linePoints);
            newBulletTrail.transform.parent = parent;
        }

        private void GoBoom(RaycastHit hit)
        {
            GoBoom(hit.point, hit.normal);
        }

        private void GoBoom(Vector3 position, Vector3 normal)
        {
            GameObject newBoom = GameObject.Instantiate<GameObject>(hitExplodeFX, position, Quaternion.identity);
            newBoom.transform.up = normal;

            List<EnemyIdentifier> alreadyHit = new List<EnemyIdentifier>();

            RaycastHit[] hits = Physics.SphereCastAll(position, explosionRadius, normal, (explosionRadius / 2.0f), LayerMask.GetMask("Limb", "BigCorpse", "Outdoors", "Environment", "Default", "Projectile"));
            if (hits.Length > 0)
            {
                if (!(hits.Length == 1 && hits[0].collider.gameObject.name == "CameraCollisionChecker"))
                {
                    hits = HydraUtils.SortRaycastHitsByDistance(hits);
                    for (int i = 0; i < hits.Length; i++)
                    {


                        if (hits[i].collider.gameObject.TryGetComponent<EnemyIdentifierIdentifier>(out EnemyIdentifierIdentifier enemyIDID))
                        {
                            if (!alreadyHit.Contains(enemyIDID.eid) && !enemyIDID.eid.dead)
                            {
                                alreadyHit.Add(enemyIDID.eid);
                                enemyIDID.eid.DeliverDamage(hits[i].collider.gameObject, hits[i].normal * -1, hits[i].point, explosionDamageMultiplier, true, 0.0f, this.gameObject);
                            }

                        }
                        else if (hits[i].collider.gameObject.TryGetComponent<EnemyIdentifier>(out EnemyIdentifier enemyID))
                        {
                            if (!alreadyHit.Contains(enemyID) && !enemyID.dead)
                            {
                                alreadyHit.Add(enemyID);
                                enemyID.DeliverDamage(hits[i].collider.gameObject, hits[i].normal * -1, hits[i].point, explosionDamageMultiplier, true, 0.0f, this.gameObject);
                            }
                        }

                        if (hits[i].collider.gameObject.TryGetComponent<Breakable>(out Breakable breakable))
                        {
                            breakable.Break();

                        }

                        if (hits[i].collider.gameObject.TryGetComponent<ThrownEgg>(out ThrownEgg egg))
                        {
                            egg.Explode();

                        }

                        if (hits[i].collider.gameObject.TryGetComponent<IUFGInteractionReceiver>(out IUFGInteractionReceiver ufgInteractable))
                        {
                            ufgInteractable.Interact(new UFGInteractionEventData()
                            {
                                tags = new string[] { "explode" },
                                direction = ufgInteractable.GetPosition() - position,
                                interactorPosition = position,
                                power = 2.0f,
                                invokeType = GetType()
                            });
                        }

                        if (hits[i].collider.gameObject.TryGetComponent<Grenade>(out Grenade grenade))
                        {
                            MonoSingleton<TimeController>.Instance.ParryFlash();
                            grenade.Explode();
                        }
                    }
                }
            }
        }


        private void OnEnable()
        {

        }

        private void OnDisable()
        {
            shooting = false;
            reloading = false;
        }

        public override string GetDebuggingText()
        {
            string debug = base.GetDebuggingText();
            debug += $"AMMO: {CurrentAmmo}\n";
            return debug;
        }
    }
}