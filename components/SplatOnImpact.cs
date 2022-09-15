using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UltraFunGuns
{
    public class SplatOnImpact : MonoBehaviour
    {
        EnemyIdentifier enemy;
        private float velocityToSplatThreshold = 50.0f; //TODO IGBalancing
        public float invincibilityTimer = 0.75f;
        private float timeElapsed = 0.0f;

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
            if (timeElapsed > invincibilityTimer)
            {
                if (collision.relativeVelocity.magnitude >= velocityToSplatThreshold)
                {
                    enemy.Splatter();
                }
                else
                {
                    GetComponent<Rigidbody>().isKinematic = true;
                    Destroy(this.GetComponent<SplatOnImpact>());
                }
            }
        }
    }
}