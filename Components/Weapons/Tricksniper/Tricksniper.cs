using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    public class Tricksniper : UltraFunGunBase
    {
        public GameObject bulletTrailPrefab;
        public GameObject muzzleFX;

        public float maxTargetAngle = 90.0f;
        public float spreadTightness = 1.5f;
        public float bulletPenetrationChance = 20.0f; // 100 Percentage

        public int revolutions = 0;
        public float rotationalAngleMultiplier = 4.5f;

        public float maxRange = 2000.0f;

        public float turnCountThreshold = 6.0f;
        public int revolveCountThreshold = 12; 

        public int turnsCompleted = 0;
        public float lastRecordedRotation = 0.0f;

        public override void OnAwakeFinished()
        {
            HydraLoader.prefabRegistry.TryGetValue("BulletTrail", out bulletTrailPrefab);
            HydraLoader.prefabRegistry.TryGetValue("TricksniperMuzzleFX", out muzzleFX);
        }

        public override Dictionary<string, ActionCooldown> SetActionCooldowns()
        {
            Dictionary<string, ActionCooldown> cooldowns = new Dictionary<string, ActionCooldown>();
            cooldowns.Add("primaryFire",new ActionCooldown(0.65f));
            cooldowns.Add("turnExpiry", new ActionCooldown(0.065f));
            return cooldowns;
        }

        private void Start()
        {

        }

        public override void DoAnimations()
        {
            
        }

        public override void GetInput()
        {
            if(MonoSingleton<InputManager>.Instance.InputSource.Fire1.WasPerformedThisFrame && actionCooldowns["primaryFire"].CanFire() && !om.paused)
            {
                actionCooldowns["primaryFire"].AddCooldown();
                Shoot();
            }
            CheckRotation();
        }

        private void CheckRotation()
        {
            float currentRotation = mainCam.transform.eulerAngles.y;
            float rotationDifference = Mathf.Abs(lastRecordedRotation - currentRotation);
            if (rotationDifference >= turnCountThreshold)
            {
                ++turnsCompleted;
            }
            else if(actionCooldowns["turnExpiry"].CanFire())
            {
                turnsCompleted = 0;
                revolutions = 0;
            }

            if(turnsCompleted >= revolveCountThreshold)
            {
                turnsCompleted = 0;
                ++revolutions;
                actionCooldowns["turnExpiry"].AddCooldown();
            }
            lastRecordedRotation = currentRotation;
        }

        private bool GetTarget(out Vector3 targetDirection) //Return true if enemy target found.
        {
            List<EnemyIdentifier> possibleTargets = new List<EnemyIdentifier>();
            List<Transform> targetPoints = new List<Transform>();
            GameObject[] enemyObjectsActive = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (GameObject enemyObject in enemyObjectsActive)
            {
                if (enemyObject.TryGetComponent<EnemyIdentifier>(out EnemyIdentifier enemyFound))
                {
                    if (!enemyFound.dead && !possibleTargets.Contains(enemyFound))
                    {
                        possibleTargets.Add(enemyFound);
                        Transform enemyTargetPoint;
                        if (enemyFound.weakPoint != null && enemyFound.weakPoint.activeInHierarchy)
                        {
                            enemyTargetPoint = enemyFound.weakPoint.transform;
                        }
                        else
                        {
                            EnemyIdentifierIdentifier enemyFoundIdentifier = enemyFound.GetComponentInChildren<EnemyIdentifierIdentifier>();
                            if (enemyFoundIdentifier)
                            {
                                enemyTargetPoint = enemyFoundIdentifier.transform;
                            }
                            else
                            {
                                enemyTargetPoint = enemyFound.transform;
                            }
                        }

                        //Cone check
                        Vector3 directionToEnemy = (enemyTargetPoint.position - mainCam.position).normalized;
                        Vector3 lookDirection = mainCam.TransformDirection(Vector3.forward).normalized;

                        float currentTargetAngle = (revolutions * (rotationalAngleMultiplier * (revolutions + 1.25f)));
                        if(Vector3.Angle(directionToEnemy, lookDirection) <= Mathf.Clamp(currentTargetAngle, 0.0f, maxTargetAngle))
                        {
                            targetPoints.Add(enemyTargetPoint);
                        }
                    }
                }
            }

            //Get closest target
            int closestIndex = -1;
            float closestDistance = Mathf.Infinity;
            for (int i = 0; i < targetPoints.Count; i++)
            {
                Vector3 distance = targetPoints[i].position - mainCam.position;
                if (distance.sqrMagnitude < closestDistance)
                {
                    closestIndex = i;
                    closestDistance = distance.sqrMagnitude;
                }
            }

            if (closestIndex > -1)
            {
                targetDirection = targetPoints[closestIndex].position - mainCam.position;
                return true;
            }
            else //No targets found.
            {
                targetDirection = Vector3.zero;
                return false;
            }
        }

        private void Shoot()
        {
            Vector3 origin = mainCam.position;
            Vector3 direction;
            
            bool penetration;

            if (!(revolutions != 0 && GetTarget(out direction)))
            {
                float randomDirectionX = UnityEngine.Random.Range(-1.0f, 1.0f);
                float randomDirectionY = UnityEngine.Random.Range(-1.0f, 1.0f);

                direction = mainCam.TransformDirection(randomDirectionX, randomDirectionY, spreadTightness);
                penetration = true;
            }
            else
            {
                penetration = (bulletPenetrationChance >= UnityEngine.Random.Range(0.0f, 100.0f));
            }

            DoHit(new Ray(origin, direction), penetration);
            Instantiate<GameObject>(muzzleFX, firePoint.position, Quaternion.identity).transform.forward = firePoint.transform.forward;

        }

        private void DoHit(Ray hitRay, bool penetration)
        {
            
            RaycastHit[] hits = Physics.RaycastAll(hitRay, maxRange, LayerMask.GetMask("Limb", "BigCorpse", "Outdoors", "Environment", "Default"));
            if (hits.Length > 0)
            {
                if (!(hits.Length == 1 && hits[0].collider.gameObject.name == "CameraCollisionChecker"))
                {
                    int endingHit = 0;
                    hits = HydraUtils.SortRaycastHitsByDistance(hits);
                    for (int i = 0; i < hits.Length; i++)
                    {
                        endingHit = i;

                        if ((hits[i].collider.gameObject.layer == 24 || hits[i].collider.gameObject.layer == 25 || hits[i].collider.gameObject.layer == 8))
                        {
                            break;
                        }

                        if (hits[i].collider.gameObject.TryGetComponent<EnemyIdentifierIdentifier>(out EnemyIdentifierIdentifier enemyIDID))
                        {
                            enemyIDID.eid.DeliverDamage(hits[i].collider.gameObject, hitRay.direction, hits[i].point, 3.0f, true, 3.0f, this.gameObject);

                            if (!penetration)
                            {
                                break;
                            }
                        }
                        else if (hits[i].collider.gameObject.TryGetComponent<EnemyIdentifier>(out EnemyIdentifier enemyID))
                        {
                            enemyID.DeliverDamage(hits[i].collider.gameObject, hitRay.direction, hits[i].point, 3.0f, true, 3.0f, this.gameObject);

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

                        if (hits[i].collider.gameObject.TryGetComponent<ThrownEgg>(out ThrownEgg egg))
                        {
                            egg.Explode(10.0f);
                            if (!penetration)
                            {
                                break;
                            }
                        }

                        if (hits[i].collider.gameObject.TryGetComponent<ThrownDodgeball>(out ThrownDodgeball dodgeBall))
                        {
                            dodgeBall.ExciteBall(6);
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
                    }
                    CreateBulletTrail(firePoint.position, hits[endingHit].point, hits[endingHit].normal);
                    return;
                }
            }
        }

        private void CreateBulletTrail(Vector3 startPosition, Vector3 endPosition, Vector3 normal)
        {
            GameObject newBulletTrail = Instantiate<GameObject>(bulletTrailPrefab, endPosition, Quaternion.identity);
            newBulletTrail.transform.up = normal;
            LineRenderer line = newBulletTrail.GetComponent<LineRenderer>();
            Vector3[] linePoints = new Vector3[2] { startPosition, endPosition };
            line.SetPositions(linePoints);
        }

    }
}
