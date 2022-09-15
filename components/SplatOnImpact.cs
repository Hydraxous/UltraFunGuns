using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UltraFunGuns
{
    public class SplatOnImpact : MonoBehaviour
    {
        EnemyIdentifier enemy;
        public float velocityToSplatThreshold = 17.0f; //TODO IGBalancing
        public float gravityTimer = 1.0f;
        public float invincibilityTimer = 0.01f;
        private float timeElapsed = 0.0f;
        private int collisions = 0;

        void Start()
        {
            enemy = gameObject.GetComponent<EnemyIdentifier>();
        }

        void FixedUpdate()
        {
            timeElapsed += Time.fixedDeltaTime;
        }

        private void OnCollisionEnter(Collision collision)
        {
            ++collisions;
            if (collisions > 5)
            {
                Destroy(this.GetComponent<SplatOnImpact>());
            }
            if (timeElapsed > invincibilityTimer)
            {
                
                if (collision.relativeVelocity.magnitude >= velocityToSplatThreshold)
                {
                    enemy.Splatter();
                }
                else
                {
                    GetComponent<Rigidbody>().isKinematic = true;
                    collisions = 10;
                }
            }
            if (timeElapsed > gravityTimer)
            {
                GetComponent<Rigidbody>().useGravity = true;
            }
        }
    }
}