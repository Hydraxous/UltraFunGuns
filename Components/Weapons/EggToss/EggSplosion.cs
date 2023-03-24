using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    /* Eggplosion object is created when a thrown egg is shot, when an eggsplosions occurs it will attempt to find nearby enemies and throw less powerful eggs at them.
     the explosion size scales the radius of the enemy check.
     */
    public class EggSplosion : MonoBehaviour
    {
        public float explosionSize = 50.0f;
        public float eggSpeedModifier = 50.0f;
        public float sourceOffset = 1.45f;

        private List<EnemyIdentifier> targetedEnemies = new List<EnemyIdentifier>();

        //Eggsplosion casts a sphere to try and find enemies nearby. It then spawns an egg and shoots it at the found enemy.
        private void Start()
        {
            int eggsSpawned = 0;
            
            RaycastHit[] hits = Physics.SphereCastAll(transform.position, explosionSize, transform.position, Mathf.Infinity);
            if (hits.Length > 0)
            {
                foreach(RaycastHit hit in hits)
                {
                    EnemyIdentifier enemy = null;
                    if (hit.collider.gameObject.TryGetComponent<EnemyIdentifierIdentifier>(out EnemyIdentifierIdentifier enemyPart))
                    {
                        enemy = enemyPart.eid;
                    }
                    if(enemy == null)
                    {
                        hit.collider.gameObject.TryGetComponent<EnemyIdentifier>(out enemy);
                    }

                    if (enemy != null)
                    {
                        if (!enemy.dead && !targetedEnemies.Contains(enemy))
                        {
                            targetedEnemies.Add(enemy);
                        }
                    }
                }

                List<Ray> eggTrajectories = new List<Ray>();
                foreach(EnemyIdentifier enemy in targetedEnemies)
                {
                    Ray eggTrajectory = new Ray();

                    Vector3 enemyOffset = enemy.transform.position;
                    enemyOffset.y += 2.0f;
                    Vector3 eggDirection = enemyOffset - transform.position;
                    Vector3 offsetPosition = transform.TransformPoint(eggDirection.normalized * sourceOffset);

                    eggDirection = enemy.transform.position - offsetPosition;

                    eggTrajectory.origin = offsetPosition;
                    eggTrajectory.direction = eggDirection;

                    eggTrajectories.Add(eggTrajectory);
                }

                foreach (Ray eggTrajectory in eggTrajectories)
                {
                    ThrownEgg newEgg = GameObject.Instantiate<GameObject>(EggToss.ThrownEggPrefab, eggTrajectory.origin, Quaternion.identity).GetComponent<ThrownEgg>();
                    newEgg.transform.forward = eggTrajectory.direction;
                    newEgg.gameObject.GetComponent<Rigidbody>().velocity = eggTrajectory.direction * eggSpeedModifier;
                    newEgg.oldVelocity = eggTrajectory.direction * eggSpeedModifier;
                    newEgg.isEggsplosionEgg = true;
                    ++eggsSpawned;
                }
            }
            
            Deboog.Log($"Eggsplosion spawned eggs: {eggsSpawned}");
        }
    }
}
