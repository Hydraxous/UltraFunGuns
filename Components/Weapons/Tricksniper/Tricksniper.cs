using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace UltraFunGuns
{
    [WeaponInfo("Tricksniper", "Tricksniper", 3, true, WeaponIconColor.Green)]
    public class Tricksniper : UltraFunGunBase
    {
        public GameObject bulletTrailPrefab;
        public GameObject muzzleFX;
        public GameObject scopeUI;
        public GameObject viewModelWrapper;

        public Text debugText;

        public float maxTargetAngle = 150.0f;
        public float spreadTightness = 3.0f;
        public float bulletPenetrationChance = 20.0f; // 100 Percentage

        public int revolutions = 0;
        public float rotationalAngleMultiplier = 4.5f;

        public float maxRange = 2000.0f;

        public float turnCountThreshold = 6.0f;
        public int revolveCountThreshold = 18;

        public int turnsCompleted = 0;
        public float lastRecordedRotation = 0.0f;

        public bool scopedIn = false;
        public float scopeInTime = 0.15f;
        public float scopeTime = 0.0f;

        public override void OnAwakeFinished()
        {
            HydraLoader.prefabRegistry.TryGetValue("BulletTrail", out bulletTrailPrefab);
            HydraLoader.prefabRegistry.TryGetValue("TricksniperMuzzleFX", out muzzleFX);
            scopeUI = transform.Find("ScopeUI").gameObject;
            viewModelWrapper = transform.Find("viewModelWrapper").gameObject;
            debugText = transform.Find("DebugCanvas/DebugPanel/DebugText").GetComponent<Text>();
        }

        public override Dictionary<string, ActionCooldown> SetActionCooldowns()
        {
            Dictionary<string, ActionCooldown> cooldowns = new Dictionary<string, ActionCooldown>();
            cooldowns.Add("primaryFire",new ActionCooldown(0.65f));
            cooldowns.Add("turnExpiry", new ActionCooldown(0.2f));
            return cooldowns;
        }

        private void Start()
        {

        }

        public override void GetInput()
        {
            if (MonoSingleton<InputManager>.Instance.InputSource.Fire1.WasPerformedThisFrame && actionCooldowns["primaryFire"].CanFire() && !om.paused)
            {
                actionCooldowns["primaryFire"].AddCooldown();
                Shoot();
            }

            bool rightClickPressed = (MonoSingleton<InputManager>.Instance.InputSource.Fire2.IsPressed);

            if (rightClickPressed && !om.paused)
            {
                scopeTime = Mathf.Clamp(scopeTime + Time.deltaTime, 0.0f, scopeInTime);
            }
            else if (!om.paused)
            {
                scopeTime = Mathf.Clamp(scopeTime - Time.deltaTime, 0.0f, scopeInTime);
            }

            scopedIn = (scopeTime >= scopeInTime);

            if(scopedIn)
            {
                viewModelWrapper.SetActive(false);
                scopeUI.SetActive(true);
            }else
            {
                viewModelWrapper.SetActive(true);
                scopeUI.SetActive(false);
            }

            CheckRotation();
            debugText.text = String.Format("{0} ROT\n{1} TURN", revolutions, turnsCompleted);
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
            bool penetration = scopedIn;
            bool enemyFound = false;
            Ray shootRay = new Ray(mainCam.position, mainCam.forward);

            if (revolutions != 0)
            {
                if (GetTarget(out Vector3 targetDir))
                {
                    enemyFound = true;
                    shootRay.direction = targetDir;
                }
                penetration = (bulletPenetrationChance >= UnityEngine.Random.Range(0.0f, 100.0f) || penetration);
            }    

            if(!scopedIn && !enemyFound)
            {
                float randomDirectionX = UnityEngine.Random.Range(-1.0f, 1.0f);
                float randomDirectionY = UnityEngine.Random.Range(-1.0f, 1.0f);

                shootRay.direction = mainCam.TransformDirection(randomDirectionX, randomDirectionY, spreadTightness);
                penetration = true;
            }

            DoHit(shootRay, penetration);

            Instantiate<GameObject>(muzzleFX, (scopedIn) ? mainCam.position : firePoint.position, Quaternion.identity).transform.forward = (scopedIn) ? mainCam.forward : firePoint.forward;
            
        }

        private void DoHit(Ray hitRay, bool penetration)
        {
            float damageAmount = (scopedIn) ? 1.5f : 3.0f;

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
                            enemyIDID.eid.DeliverDamage(hits[i].collider.gameObject, hitRay.direction, hits[i].point, damageAmount, true, damageAmount, this.gameObject);

                            if (!penetration)
                            {
                                break;
                            }
                        }
                        else if (hits[i].collider.gameObject.TryGetComponent<EnemyIdentifier>(out EnemyIdentifier enemyID))
                        {
                            enemyID.DeliverDamage(hits[i].collider.gameObject, hitRay.direction, hits[i].point, damageAmount, true, damageAmount, this.gameObject);

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
                    CreateBulletTrail((scopedIn) ? mainCam.position : firePoint.position, hits[endingHit].point, hits[endingHit].normal);
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
