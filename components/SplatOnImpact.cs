using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UltraFunGuns
{
    public class SplatOnImpact : MonoBehaviour
    {
        EnemyIdentifier enemy;
        public float velocityToSplatThreshold = 50.0f; //TODO IGBalancing
        public float invincibilityTimer = 0.75f;
        private float timeElapsed = 0.0f;
        private int collisions = 0;

        void Start()
        {
            enemy = gameObject.GetComponent<EnemyIdentifier>();
        }

        void Update()
        {
            invincibilityTimer += Time.deltaTime;
        }

        private void OnCollisionEnter(Collision collision)
        {
            ++collisions;
            if (timeElapsed > invincibilityTimer)
            {
                if (collision.relativeVelocity.magnitude >= velocityToSplatThreshold)
                {
                    enemy.Splatter();
                }
                else
                {
                    GetComponent<Rigidbody>().isKinematic = true;       
                }
            }
            if(collisions > 20)
            {
                Destroy(this.GetComponent<SplatOnImpact>());
            }
        }
    }
}