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
        private bool orbited = false;

        void Start()
        {
            enemy = gameObject.GetComponent<EnemyIdentifier>();
        }

        void FixedUpdate()
        {
            timeElapsed += Time.fixedDeltaTime;
            if (transform.position.y >= 1500.0f && !orbited)
            {
                orbited = true;
                MonoSingleton<StyleHUD>.Instance.AddPoints(150, "hydraxous.ultrafunguns.orbited", null, enemy, -1, "", "");
                enemy.health = -1.0f;
                enemy.Explode();
            }
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