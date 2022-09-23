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
        public GameObject thrownEggPrefab;
        public float explosionSize = 3.5f;
        public float eggSpeedModifier = 3.0f;
        public float sourceOffset = 0.75f;

        private List<EnemyIdentifier> targetedEnemies = new List<EnemyIdentifier>();

        private void Awake()
        {
            HydraLoader.prefabRegistry.TryGetValue("ThrownEgg", out thrownEggPrefab);
        }


        //Eggsplosion casts a sphere to try and find enemies nearby. It then spawns an egg and shoots it at the found enemy.
        private void Start()
        {
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

                    Vector3 eggDirection = enemy.transform.position - transform.position;
                    Vector3 offsetPosition = transform.TransformPoint(eggDirection.normalized * sourceOffset);

                    eggDirection = enemy.transform.position - offsetPosition;
                    eggDirection *= eggSpeedModifier;

                    eggTrajectory.origin = offsetPosition;
                    eggTrajectory.direction = eggDirection;

                    eggTrajectories.Add(eggTrajectory);
                }

                foreach(Ray eggTrajectory in eggTrajectories)
                {
                    ThrownEgg newEgg = GameObject.Instantiate<GameObject>(thrownEggPrefab, eggTrajectory.origin, Quaternion.identity).GetComponent<ThrownEgg>();
                    newEgg.isEggsplosionEgg = true;
                    newEgg.transform.forward = eggTrajectory.direction;
                    Rigidbody rb = newEgg.GetComponent<Rigidbody>();
                    rb.velocity = eggTrajectory.direction;
                }
            }

        }
    }
}
