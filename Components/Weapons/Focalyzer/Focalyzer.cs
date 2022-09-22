using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    public class Focalyzer : UltraFunGunBase
    {
        public FocalyzerLaserController laser;
        public FocalyzerTubeController tubeController;
        public GameObject pylonPrefab;

        private bool throwingPylon = false;
        public bool laserActive = false;
        public bool hittingAPylon = false;

        private LayerMask laserHitMask;

        public override void OnAwakeFinished()
        {
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
            }else if (laserActive)
            {
                laserActive = false;
                hittingAPylon = false;
                actionCooldowns["fireLaser"].AddCooldown();
            }

            if (MonoSingleton<InputManager>.Instance.InputSource.Fire2.WasPerformedThisFrame && actionCooldowns["throwPylon"].CanFire())
            {
                if (!om.paused && laser.GetPylonCount() < laser.maxPylons && !throwingPylon)
                {
                    StartCoroutine(ThrowPylon());
                }
            }
        }

        public override void DoAnimations()
        {
            laser.laserActive = laserActive;
            tubeController.crystalsUsed = laser.GetPylonCount()-1;
            animator.SetBool("LaserActive", laserActive);
        }

        //sorts raycast hits by distance cause unity is weird about it.
        public RaycastHit[] SortHitsByDistance(RaycastHit[] hits)
        {
            List<RaycastHit> sortedHits = new List<RaycastHit>(hits.Length);

            for(int i=0;i<hits.Length;i++)
            {
                RaycastHit currentHit = hits[i];
                int currentIndex = i;

                while (currentIndex > 0 && sortedHits[currentIndex - 1].distance > currentHit.distance)
                {
                    currentIndex--;
                }

                sortedHits.Insert(currentIndex, currentHit);

            }

            return sortedHits.ToArray();
        }


        public void FireLaser()
        {
            Vector3 laserVector = mainCam.TransformDirection(0, 0, 1);
            RaycastHit[] hits = Physics.RaycastAll(mainCam.transform.position, laserVector, 1000.0f, laserHitMask);
            if (hits.Length > 0)
            {
                bool hitPylon = false;
                int endingHit = 0;
                hits = SortHitsByDistance(hits);
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
                    }
                    else if (hits[i].collider.gameObject.TryGetComponent<EnemyIdentifier>(out EnemyIdentifier enemyID))
                    {
                        if (actionCooldowns["damageTick"].CanFire())
                        {
                            actionCooldowns["damageTick"].AddCooldown();
                            enemyID.DeliverDamage(hits[i].collider.gameObject, laserVector, hits[i].point, 0.75f, false);
                        }
                    }

                    if(hits[i].collider.gameObject.TryGetComponent<Breakable>(out Breakable breakable))
                    {
                        breakable.Break();
                    }

                    if (hits[i].collider.gameObject.TryGetComponent<ThrownEgg>(out ThrownEgg egg))
                    {
                        MonoSingleton<TimeController>.Instance.ParryFlash();
                        egg.Explode();
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
            }
            else //if we didnt hit anything... somehow.. 
            {
                Vector3 missEndpoint = mainCam.TransformPoint(0, 0, 1000.0f);
                Vector3 towardsPlayer = player.transform.position - missEndpoint;
                DrawLaser(firePoint.position, missEndpoint, towardsPlayer);
            }
        }

        private void DrawLaser(Vector3 origin, Vector3 endPoint, Vector3 normal)
        {
            laser.AddLinePosition(origin);
            laser.AddLinePosition(endPoint);
            laser.BuildLine(normal);
        }

        //TODO check for consequences of your hubris.
        IEnumerator ThrowPylon()
        {
            throwingPylon = true;
            actionCooldowns["throwPylon"].AddCooldown();
            animator.Play("Focalyzer_ThrowPylon");
            yield return new WaitForSeconds(0.3f);
            GameObject newPylon = GameObject.Instantiate<GameObject>(pylonPrefab, mainCam.TransformPoint(0, 0, 1), Quaternion.identity);
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
            hittingAPylon = false;
            //laser.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            
        }
    }
}
