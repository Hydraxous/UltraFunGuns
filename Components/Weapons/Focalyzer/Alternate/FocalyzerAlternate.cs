using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{

    //Laser rifle that does damage to enemies over time while it's hitting them, also can place pylons which will refract the laser at random or to another pylon.
    [FunGun("FocalyzerAlternate", "Focalyzer V2", 2, true, WeaponIconColor.Blue)]
    public class FocalyzerAlternate : UltraFunGunBase
    {
        public FocalyzerLaserControllerAlternate laser;
        public FocalyzerTubeControllerAlternate tubeController;
        public GameObject pylonPrefab;
        public Transform aimSpot;

        private StyleHUD style;

        public int maxStoredPylons = 3;
        public int pylonsRemaining = 3;

        public float pylonRechargeTime = 6.0f;
        public float pylonRechargeTimeRemaining = 0.0f;

        private bool throwingPylon = false;
        public bool laserActive = false;

        public float laserWidth = 0.3f;
        public float laserMaxRange = 2000.0f;

        private LayerMask laserHitMask;

        public override void OnAwakeFinished()
        {
            style = MonoSingleton<StyleHUD>.Instance;
            weaponIcon.variationColor = 0;
            tubeController = transform.Find("viewModelWrapper/FocalyzerGunModel/Tubes").gameObject.AddComponent<FocalyzerTubeControllerAlternate>();
            aimSpot = GameObject.Instantiate<GameObject>(new GameObject(), Vector3.zero, Quaternion.identity).transform;
            aimSpot.name = "PylonTarget";
        }

        private void Start()
        {
            laserHitMask = LayerMask.GetMask("Projectile", "Limb", "BigCorpse", "Environment", "Outdoors", "Armor", "Default");
            HydraLoader.prefabRegistry.TryGetValue("FocalyzerPylonAlternate", out pylonPrefab);
            HydraLoader.prefabRegistry.TryGetValue("FocalyzerLaserAlternate", out GameObject laserPrefab);
            laser = GameObject.Instantiate<GameObject>(laserPrefab, Vector3.zero, Quaternion.identity).GetComponent<FocalyzerLaserControllerAlternate>();
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
                if (!om.paused && !throwingPylon && (pylonsRemaining > 0 || ULTRAKILL.Cheats.NoWeaponCooldown.NoCooldown))
                {
                    StartCoroutine(ThrowPylon());
                }
            }

            CheckPylonRecharge();
        }

        protected override void DoAnimations()
        {
            laser.laserActive = laserActive;
            tubeController.crystalsRemaining = pylonsRemaining;
            animator.SetBool("LaserActive", laserActive);
        }

        /* Laser function sweeps a sphere in the direction the player is looking, returns all targets in order of distance, checks
         *  each target and acts accordingly based on the information gathered from the target. Laser should stop when it hits an object.
         */
        public void FireLaser()
        {
            Vector3 laserVector = mainCam.TransformDirection(0, 0, 1);
            RaycastHit[] hits = Physics.SphereCastAll(mainCam.transform.position, laserWidth, laserVector, laserMaxRange, laserHitMask);
            if (hits.Length > 0)
            {
                if (!(hits.Length == 1 && hits[0].collider.gameObject.name == "CameraCollisionChecker"))
                {
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
                    }

                    DrawLaser(firePoint.position, hits[endingHit].point, hits[endingHit].normal);
                    return;
                }
            }

            Ray missingRay = new Ray();
            missingRay.origin = firePoint.position;
            missingRay.direction = mainCam.TransformDirection(0, 0, 1);
            Vector3 missEndpoint = missingRay.GetPoint(laserMaxRange);
            Vector3 towardsPlayer = mainCam.transform.position - missEndpoint;
            DrawLaser(firePoint.position, missEndpoint, towardsPlayer);
        }

        private void DrawLaser(Vector3 origin, Vector3 endPoint, Vector3 normal)
        {
            if(laserActive)
            {
                aimSpot.position = endPoint;
            }

            laser.AddLinePosition(origin);
            laser.AddLinePosition(endPoint);          
            laser.BuildLine(normal);
        }

        private void CheckPylonRecharge()
        {
            if(pylonRechargeTimeRemaining - style.rankIndex < Time.time) //As style rank increases, it lessens the cooldown time.
            {
                pylonRechargeTimeRemaining = Time.time + pylonRechargeTime;
                pylonsRemaining = Mathf.Clamp(pylonsRemaining+1, 0, maxStoredPylons);
            }
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

            pylonRechargeTimeRemaining = Time.time + pylonRechargeTime;
            pylonsRemaining = Mathf.Clamp(pylonsRemaining - 1, 0, maxStoredPylons);

            MonoSingleton<CameraController>.Instance.CameraShake(0.2f);
            FocalyzerPylonAlternate pylon = newPylon.GetComponent<FocalyzerPylonAlternate>();
            pylon.laserHitMask = laserHitMask;
            pylon.focalyzer = this;
            pylon.targetPoint = aimSpot;

            newPylon.GetComponent<Rigidbody>().velocity = (mainCam.TransformDirection(0, 0, 1)*40.0f);
            throwingPylon = false;
        }

        public override Dictionary<string, ActionCooldown> SetActionCooldowns()
        {
            Dictionary<string, ActionCooldown> cooldowns = new Dictionary<string, ActionCooldown>();
            cooldowns.Add("fireLaser", new ActionCooldown(0.16f));
            cooldowns.Add("damageTick", new ActionCooldown(0.2f));
            cooldowns.Add("throwPylon", new ActionCooldown(0.25f));
            return cooldowns;
        }

        private void OnDisable()
        {
            laserActive = false;
            DoAnimations();
            throwingPylon = false;
        }

        private void OnEnable()
        {
            animator.Play("Focalyzer_Equip");
            //TODO fix this algorithm it does not work.
            return;
            while (pylonRechargeTimeRemaining + (pylonRechargeTime - style.rankIndex) < Time.time && pylonsRemaining < maxStoredPylons)
            {
                pylonRechargeTimeRemaining += pylonRechargeTime;
                pylonsRemaining = Mathf.Clamp(pylonsRemaining + 1, 0, maxStoredPylons);
            }
        }

        public void OnPylonDeath()
        {
            pylonsRemaining = Mathf.Clamp(pylonsRemaining + 1, 0, maxStoredPylons);
        }
    }
}
