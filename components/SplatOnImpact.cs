using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UltraFunGuns
{
    public class SplatOnImpact : MonoBehaviour
    {
        EnemyIdentifier enemy;
        private float velocityToSplatThreshold = 50.0f; //TODO IGBalancing

        void Start()
        {
            enemy = gameObject.GetComponent<EnemyIdentifier>();
        }

        private void OnCollisionEnter(Collision collision)
        {

            if (collision.relativeVelocity.magnitude >= velocityToSplatThreshold)
            {
                enemy.Splatter();
            }else
            {
                Destroy(this.GetComponent<SplatOnImpact>());
            }
        }
    }
}