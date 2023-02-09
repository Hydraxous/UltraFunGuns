using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{

    //Laser rifle that does damage to enemies over time while it's hitting them, also can place pylons which will refract the laser at random or to another pylon.
    public class Focalyzer : UltraFunGunBase
    {
        public FocalyzerLaserController laser;
        public FocalyzerTubeController tubeController;
        public GameObject pylonPrefab;

        private bool throwingPylon = false;
        public bool laserActive = false;
        public bool hittingAPylon = false;

        public float laserWidth = 0.3f;
        public float laserMaxRange = 2000.0f;

        private LayerMask laserHitMask;

        public override void OnAwakeFinished()
        {
            weaponIcon.variationColor = 2;
            tubeController = transform.Find("viewModelWrapper/FocalyzerGunModel/Tubes").gameObject.AddComponent<FocalyzerTubeController>();
        }

        private void Start()
        {
            laserHitMask = LayerMask.GetMask("Projectile", "Limb", "BigCorpse", "Environment", "Outdoors", "Armor", "Default");
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
                MonoSingleton<CameraController>.Instance.CameraShake(0.05f);
            }
            else if (laserActive)
            {
                laserActive = false;
                hittingAPylon = false;
                actionCooldowns["fireLaser"].AddCooldown();
            }else
            {
                //should make the laser follow the weapon even when it's turning off.
                Ray missingRay = new Ray();
                missingRay.origin = firePoint.position;
                missingRay.direction = mainCam.TransformDirection(0, 0, 1);
                Vector3 missEndpoint = missingRay.GetPoint(laserMaxRange);
                Vector3 towardsPlayer = mainCam.transform.position - missEndpoint;
                DrawLaser(firePoint.position, missEndpoint, towardsPlayer);
            }

            if (MonoSingleton<InputManager>.Instance.InputSource.Fire2.WasPerformedThisFrame && actionCooldowns["throwPylon"].CanFire())
            {
                if (!om.paused && (laser.GetPylonCount() < laser.maxPylons || ULTRAKILL.Cheats.NoWeaponCooldown.NoCooldown) && !throwingPylon)
                {
                    StartCoroutine(ThrowPylon());
                }
            }
        }

        protected override void DoAnimations()
        {
            laser.laserActive = laserActive;
            tubeController.crystalsUsed = Mathf.Clamp(laser.GetPylonCount()-1,0,laser.maxPylons);
            animator.SetBool("LaserActive", laserActive);
        }

        /* Laser function sweeps a sphere in the direction the player is looking, returns all targets in order of distance, checks
         *  each target and acts accordingly based on the information gathered from the target. Laser should stop when it hits an object.
         *  if the laser hits a FocalyzerPylon it will attempt to tell the pylon to start firing it's own laser depending on the result of a pylon
         *  targeting check. For laser penetration, remove the break statements.
         */
        public void FireLaser()
        {
            Vector3 laserVector = mainCam.TransformDirection(0, 0, 1);
            RaycastHit[] hits = Physics.SphereCastAll(mainCam.transform.position, laserWidth, laserVector, laserMaxRange, laserHitMask);
            if (hits.Length > 0)
            {
                if (!(hits.Length == 1 && hits[0].collider.gameObject.name == "CameraCollisionChecker"))
                {
                    bool hitPylon = false;
                    int endingHit = 0;
                    hits = HydraUtils.SortRaycastHitsByDistance(hits);
                    for (int i = 0; i < hits.Length; i++)
                    {
                        endingHit = i;

                        if (hits[i].collider.gameObject.layer == 24 || hits[i].collider.gameObject.layer == 25 || hits[i].collider.gameObject.layer == 8)
                        {
                            break;
                        }

                        if (hits[i].collider.gameObject.TryGetComponent<EnemyIdentifierIdentifier>(out EnemyIdentifierIdentifier enemyIDID))
                        {
                            if (actionCooldowns["damageTick"].CanFire())
                            {
                                actionCooldowns["damageTick"].AddCooldown();
                                enemyIDID.eid.DeliverDamage(hits[i].collider.gameObject, laserVector, hits[i].point, 0.75f, false);
                            }
                            break;
                        }
                        else if (hits[i].collider.gameObject.TryGetComponent<EnemyIdentifier>(out EnemyIdentifier enemyID))
                        {
                            if (actionCooldowns["damageTick"].CanFire())
                            {
                                actionCooldowns["damageTick"].AddCooldown();
                                enemyID.DeliverDamage(hits[i].collider.gameObject, laserVector, hits[i].point, 0.75f, false);
                            }
                            break;
                        }

                        if (hits[i].collider.gameObject.TryGetComponent<Breakable>(out Breakable breakable))
                        {
                            breakable.Break();
                            break;
                        }

                        if (hits[i].collider.gameObject.TryGetComponent<ThrownEgg>(out ThrownEgg egg))
                        {

                            egg.Explode(10.0f);
                            break;
                        }

                        if (hits[i].collider.gameObject.TryGetComponent<Grenade>(out Grenade grenade))
                        {
                            MonoSingleton<TimeController>.Instance.ParryFlash();
                            grenade.Explode();
                            break;
                        }

                        if (hits[i].collider.gameObject.TryGetComponent<FocalyzerPylon>(out FocalyzerPylon pylon))
                        {
                            hittingAPylon = true; //need this here because of the coroutine in FocalyzerPylon
                            pylon.DoRefraction(pylon, true);
                            hitPylon = true;
                            break;
                        }
                    }

                    hittingAPylon = hitPylon;
                    DrawLaser(firePoint.position, hits[endingHit].point, hits[endingHit].normal);
                    return;
                }
            }
            hittingAPylon = false;
            Ray missingRay = new Ray();
            missingRay.origin = firePoint.position;
            missingRay.direction = mainCam.TransformDirection(0, 0, 1);
            Vector3 missEndpoint = missingRay.GetPoint(laserMaxRange);
            Vector3 towardsPlayer = mainCam.transform.position - missEndpoint;
            DrawLaser(firePoint.position, missEndpoint, towardsPlayer);
        }

        private void DrawLaser(Vector3 origin, Vector3 endPoint, Vector3 normal)
        {
            laser.AddLinePosition(origin);
            laser.AddLinePosition(endPoint);
            laser.BuildLine(normal);
        }

        //Check for consequences of your hubris. Update: No hubris found... yet.
        //Throws pylon out TODO maybe update position so it comes from the gun?
        IEnumerator ThrowPylon()
        {
            throwingPylon = true;
            actionCooldowns["throwPylon"].AddCooldown();
            animator.Play("Focalyzer_ThrowPylon");
            yield return new WaitForSeconds(0.3f);
            GameObject newPylon = GameObject.Instantiate<GameObject>(pylonPrefab, mainCam.TransformPoint(0, 0, 1), Quaternion.identity);
            MonoSingleton<CameraController>.Instance.CameraShake(0.2f);
            FocalyzerPylon pylon = newPylon.GetComponent<FocalyzerPylon>();
            pylon.laserHitMask = laserHitMask;
            pylon.focalyzer = this;
            pylon.pylonManager = laser;

            newPylon.GetComponent<Rigidbody>().velocity = (mainCam.TransformDirection(0, 0, 1)*40.0f);
            throwingPylon = false;
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
            DoAnimations();
            hittingAPylon = false;
            throwingPylon = false;
            //laser.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            animator.Play("Focalyzer_Equip");
        }
    }
}
