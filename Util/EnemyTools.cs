using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    public static class EnemyTools
    {

        public static bool CanBeDamaged(this EnemyIdentifier eid)
        {
            if (eid == null)
            {
                return false;
            }

            if (eid.dead)
            {
                return false;
            }

            if (eid.health <= 0.0f)
            {
                return false;
            }

            if (!eid.enabled)
            {
                return false;
            }

            return true;
        }

        /*
        public static TargetObject GetTargetFromGameObject(GameObject targetGameObject)
        {
            TargetObject newTargetObject = new TargetObject(targetGameObject);
            return newTargetObject;
        }
        */

        public static Vector3 GetTargetPoint(this EnemyIdentifier eid)
        {
            if (eid == null)
            {
                return Vector3.zero;
            }

            if (eid.weakPoint != null)
            {
                return eid.weakPoint.transform.position;
            }

            return eid.transform.position;
        }

    /*
        public static List<TargetObject> GetTargetsFromGameObjects(GameObject[] targetGameObjects)
        {
            List<TargetObject> newTargets = new List<TargetObject>();
            for (int i = 0; i < targetGameObjects.Length; i++)
            {
                TargetObject newTarget = new TargetObject(targetGameObjects[i]);
                if (newTarget.validTarget)
                {
                    newTargets.Add(new TargetObject(targetGameObjects[i]));
                }
            }
            return newTargets;
        }
    */
        public static bool IsColliderEnemy(this Collider collider, out EnemyIdentifier enemy, bool filterDead = true)
        {
            enemy = null;
            if (collider.gameObject.TryGetComponent<EnemyIdentifierIdentifier>(out EnemyIdentifierIdentifier enemyIDID))
            {
                if (enemyIDID.eid != null)
                {
                    enemy = enemyIDID.eid;
                    if (!enemy.dead || !filterDead)
                    {
                        return true;
                    }
                }
            }
            else if (collider.gameObject.TryGetComponent<EnemyIdentifier>(out EnemyIdentifier enemyID))
            {
                enemy = enemyID;
                if (!enemy.dead || !filterDead)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsCollisionEnemy(this Collision collider, out EnemyIdentifier enemy, bool filterDead = true)
        {
            enemy = null;
            if (collider.gameObject.TryGetComponent<EnemyIdentifierIdentifier>(out EnemyIdentifierIdentifier enemyIDID))
            {
                if (enemyIDID.eid != null)
                {
                    enemy = enemyIDID.eid;
                    if (!enemy.dead || !filterDead)
                    {
                        return true;
                    }
                }
            }
            else if (collider.gameObject.TryGetComponent<EnemyIdentifier>(out EnemyIdentifier enemyID))
            {
                enemy = enemyID;
                if (!enemy.dead || !filterDead)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool TryGetHomingTarget(Vector3 sampleLocation, out Transform homingTarget, out EnemyIdentifier enemyComponent)
        {
            homingTarget = null;
            enemyComponent = null;

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

                        Vector3 directionToEnemy = enemyTargetPoint.position - sampleLocation;
                        if (Physics.Raycast(sampleLocation, directionToEnemy, out RaycastHit rayHit, directionToEnemy.magnitude, LayerMask.GetMask("Limb", "BigCorpse", "Outdoors", "Environment", "Default")))
                        {
                            if (rayHit.collider.gameObject.TryGetComponent<EnemyIdentifier>(out EnemyIdentifier losEnemyID) || rayHit.collider.gameObject.TryGetComponent<EnemyIdentifierIdentifier>(out EnemyIdentifierIdentifier losEnemyIDID))
                            {
                                targetPoints.Add(enemyTargetPoint);
                            }
                        }

                    }
                }
            }

            int closestIndex = -1;
            float closestDistance = Mathf.Infinity;
            for (int i = 0; i < targetPoints.Count; i++)
            {
                Vector3 distance = targetPoints[i].position - sampleLocation;
                if (distance.sqrMagnitude < closestDistance)
                {
                    closestIndex = i;
                    closestDistance = distance.sqrMagnitude;
                }
            }

            if (closestIndex > -1)
            {
                homingTarget = targetPoints[closestIndex];
                enemyComponent = homingTarget.GetComponent<EnemyIdentifier>();
                return true;
            }
            else
            {
                return false;
            }

        }


        //TODO optimize this. this camera only
        public static bool TryGetTarget(out Vector3 targetDirection) //Return true if enemy target found.
        {
            List<EnemyIdentifier> possibleTargets = new List<EnemyIdentifier>();
            List<Transform> targetPoints = new List<Transform>();
            GameObject[] enemyObjectsActive = EnemyTracker.Instance.GetCurrentEnemies().Where(x => !x.blessed && !x.dead).Select(e => e.gameObject).ToArray();
            Transform camera = CameraController.Instance.transform;

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
                        Vector3 directionToEnemy = (enemyTargetPoint.position - camera.position).normalized;
                        Vector3 lookDirection = camera.forward;
                        if (HydraUtils.SphereCastMacro(camera.position, 0.05f, directionToEnemy, Mathf.Infinity, out RaycastHit hit))
                        {
                            switch (hit.collider.gameObject.layer)
                            {
                                case 10:
                                case 11:
                                    targetPoints.Add(enemyTargetPoint);
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
            }

            //Get closest target
            int closestIndex = -1;
            float closestDistance = Mathf.Infinity;
            for (int i = 0; i < targetPoints.Count; i++)
            {
                Vector3 distance = targetPoints[i].position - camera.position;
                if (distance.sqrMagnitude < closestDistance)
                {
                    closestIndex = i;
                    closestDistance = distance.sqrMagnitude;
                }
            }

            if (closestIndex > -1)
            {
                targetDirection = targetPoints[closestIndex].position - camera.position;
                return true;
            }
            else //No targets found.
            {
                targetDirection = Vector3.zero;
                return false;
            }
        }

    }
}
