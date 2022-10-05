using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;


namespace UltraFunGuns
{
    public static class HydraUtils
    {
        public static RaycastHit[] SortRaycastHitsByDistance(RaycastHit[] hits)
        {
            List<RaycastHit> sortedHits = new List<RaycastHit>(hits.Length);

            for (int i = 0; i < hits.Length; i++)
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

        //LOS check only counts the level, environment, etc. as obstruction of view.
        public static bool LineOfSightCheck(Vector3 source, Vector3 target)
        {
            Vector3 rayCastDirection = target - source;
            RaycastHit[] hits = Physics.RaycastAll(source, rayCastDirection, rayCastDirection.magnitude);
            hits = HydraUtils.SortRaycastHitsByDistance(hits);
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.gameObject.layer == 24 || hit.collider.gameObject.layer == 25 || hit.collider.gameObject.layer == 8 || hit.collider.gameObject.layer == 0)
                {
                    if (hit.collider.gameObject.name != "CameraCollisionChecker")
                    {
                        return false;
                    }

                }
            }
            return true;
        }

        public static Vector3[] DoRayHit(Ray hitRay, float range, bool penetration = false, float enemyDamage = 1.0f, bool explodeEnemyLimb = false, float critDamageMultiplier = 0.0f, GameObject sourceObject = null, bool explodeGrenade = false, bool explodeEgg = false, bool breakGlass = false, bool breakBreakable = false, bool exciteDodgeball = false)
        {
            RaycastHit[] hits = Physics.RaycastAll(hitRay, range, LayerMask.GetMask("Limb", "BigCorpse", "Outdoors", "Environment", "Default"));
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
                            enemyIDID.eid.DeliverDamage(hits[i].collider.gameObject, hitRay.direction, hits[i].point, enemyDamage, explodeEnemyLimb, critDamageMultiplier, sourceObject);

                            if (!penetration)
                            {
                                break;
                            }
                        }
                        else if (hits[i].collider.gameObject.TryGetComponent<EnemyIdentifier>(out EnemyIdentifier enemyID))
                        {
                            enemyID.DeliverDamage(hits[i].collider.gameObject, hitRay.direction, hits[i].point, enemyDamage, explodeEnemyLimb, critDamageMultiplier, sourceObject);

                            if (!penetration)
                            {
                                break;
                            }
                        }

                        if (hits[i].collider.gameObject.TryGetComponent<Breakable>(out Breakable breakable) && breakBreakable)
                        {
                            breakable.Break();
                            if (!penetration)
                            {
                                break;
                            }
                        }

                        if (hits[i].collider.gameObject.TryGetComponent<ThrownEgg>(out ThrownEgg egg) && explodeEgg)
                        {
                            egg.Explode(10.0f);
                            if (!penetration)
                            {
                                break;
                            }
                        }

                        if (hits[i].collider.gameObject.TryGetComponent<ThrownDodgeball>(out ThrownDodgeball dodgeBall) && exciteDodgeball)
                        {
                            dodgeBall.ExciteBall(6);
                            if (!penetration)
                            {
                                break;
                            }
                        }

                        if (hits[i].collider.gameObject.TryGetComponent<Grenade>(out Grenade grenade) && explodeGrenade)
                        {
                            MonoSingleton<TimeController>.Instance.ParryFlash();
                            grenade.Explode();
                            if (!penetration)
                            {
                                break;
                            }
                        }
                    }
                    return new Vector3[] { hits[endingHit].point, hits[endingHit].normal };
                }
            }

            return new Vector3[] { Vector3.zero };

        }

        public static TargetObject GetTargetFromGameObject(GameObject targetGameObject)
        {
            TargetObject newTargetObject = new TargetObject(targetGameObject);
            return newTargetObject;
        }

        public static List<TargetObject> GetTargetsFromGameObjects(GameObject[] targetGameObjects)
        {
            List<TargetObject> newTargets = new List<TargetObject>();
            for(int i = 0; i < targetGameObjects.Length; i++)
            {
                TargetObject newTarget = new TargetObject(targetGameObjects[i]);
                if(newTarget.validTarget)
                {
                    newTargets.Add(new TargetObject(targetGameObjects[i]));
                }
            }
            return newTargets;
        }

        public static bool ConeCheck(Vector3 direction1, Vector3 direction2, float maximumAngle = 90.0f)
        {
            return Vector3.Angle(direction1, direction2) <= maximumAngle;
        }
    }

    //TODO Placeholder
    public class AltTarget
    {
        public enum TargetType { Zombie, Spider, Machine, Statue, Wicked, Drone, Idol, Dodgeball, GenericRigidbody, Egg, Pylon }
        public TargetType targetType;
        public GameObject gameObject;
        public Transform weakPoint;
        public Transform targetPoint;
        public Rigidbody rigidbody;
        public bool validTarget = true;
        private void NULL()
        {
            if (this.gameObject.TryGetComponent<ThrownEgg>(out ThrownEgg egg))
            {
                this.targetPoint = egg.transform;
                this.targetType = TargetType.Egg;
                return;
            }
            else if (this.gameObject.GetComponentInParent<ThrownEgg>() != null)
            {
                this.targetPoint = this.gameObject.GetComponentInParent<ThrownEgg>().transform;
                this.targetType = TargetType.Egg;
                return;
            }


            if (this.gameObject.TryGetComponent<ThrownDodgeball>(out ThrownDodgeball thrownDodgeball))
            {
                this.targetPoint = egg.transform;
                this.targetType = TargetType.Dodgeball;
                return;
            }
            else if (this.gameObject.GetComponentInParent<ThrownDodgeball>() != null)
            {
                this.targetPoint = this.gameObject.GetComponentInParent<ThrownDodgeball>().transform;
                this.targetType = TargetType.Dodgeball;
                return;
            }


            if (this.gameObject.TryGetComponent<FocalyzerPylon>(out FocalyzerPylon pylon))
            {
                this.targetPoint = pylon.transform;
                this.targetType = TargetType.Pylon;
                return;
            }
            else if (this.gameObject.GetComponentInParent<FocalyzerPylon>() != null)
            {
                this.targetPoint = this.gameObject.GetComponentInParent<FocalyzerPylon>().transform;
                this.targetType = TargetType.Pylon;
                return;
            }


            if (this.gameObject.TryGetComponent<FocalyzerPylonAlternate>(out FocalyzerPylonAlternate pylonAlt))
            {
                this.targetPoint = pylonAlt.transform;
                this.targetType = TargetType.Pylon;
                return;
            }
            else if (this.gameObject.GetComponentInParent<FocalyzerPylonAlternate>() != null)
            {
                this.targetPoint = this.gameObject.GetComponentInParent<FocalyzerPylonAlternate>().transform;
                this.targetType = TargetType.Pylon;
                return;
            }
        }
    }

    public class TargetObject
    {
        public enum TargetType { Zombie, Spider, Machine, Statue, Wicked, Drone, Idol, Dodgeball, GenericRigidbody, Egg, Pylon }
        public TargetType targetType;
        public GameObject gameObject;
        public Transform weakPoint;
        public Transform targetPoint;
        public Rigidbody rigidbody;
        public bool validTarget = true;

        public TargetObject(GameObject gameObject)
        {
            this.gameObject = gameObject;

            if(!this.gameObject.activeInHierarchy)
            {
                this.validTarget = false;
                return;
            }

            //Generic rigibody check
            if (this.gameObject.TryGetComponent<Rigidbody>(out Rigidbody genericRigidbody))
            {
                this.targetPoint = genericRigidbody.transform;
                this.targetType = TargetType.GenericRigidbody;
            }


            if (this.gameObject.TryGetComponent<EnemyIdentifier>(out EnemyIdentifier enemyFound))
            {
                if (!enemyFound.dead)
                {
                    if (enemyFound.zombie != null)
                    {
                        targetType = TargetType.Zombie;
                    }
                    else if (enemyFound.spider != null)
                    {
                        targetType = TargetType.Spider;
                    }
                    else if (enemyFound.machine != null)
                    {
                        targetType = TargetType.Machine;
                    }
                    else if (enemyFound.statue != null)
                    {
                        targetType = TargetType.Statue;
                    }
                    else if (enemyFound.wicked != null)
                    {
                        targetType = TargetType.Wicked;
                    }
                    else if (enemyFound.drone != null)
                    {
                        targetType = TargetType.Drone;
                    }
                    else if (enemyFound.idol != null)
                    {
                        targetType = TargetType.Idol;
                    }

                    if (enemyFound.weakPoint != null && enemyFound.weakPoint.activeInHierarchy)
                    {
                        this.weakPoint = enemyFound.weakPoint.transform;
                    }

                    EnemyIdentifierIdentifier enemyFoundIdentifier = enemyFound.GetComponentInChildren<EnemyIdentifierIdentifier>();
                    if (enemyFoundIdentifier)
                    {
                        this.targetPoint = enemyFoundIdentifier.transform;
                    }
                    else
                    {
                        this.targetPoint = enemyFound.transform;
                    }
                    return;
                }
            }


            

            //Ending validity check
            if(this.targetType != TargetType.GenericRigidbody)
            {
                this.validTarget = false;
                return;
            }
        }
    }
}
