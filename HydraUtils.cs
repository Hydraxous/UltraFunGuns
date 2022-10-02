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
                    if(hit.collider.gameObject.name != "CameraCollisionChecker")
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
                    return new Vector3[] {hits[endingHit].point, hits[endingHit].normal };
                }
            }
            
            return new Vector3[] { Vector3.zero };
            
        }

    }
}
