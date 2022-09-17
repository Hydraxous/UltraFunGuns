using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    public class ThrownEgg : MonoBehaviour
    {
        public GameObject impactFX;
        private Rigidbody rb;
        private Vector3 oldVelocity;
        private float invicibleTimer = 0.08f;
        private bool canImpact = false;

        private void Awake()
        {
            HydraLoader.prefabRegistry.TryGetValue("EggImpactFX", out impactFX);
            rb = GetComponent<Rigidbody>();
        }

        private void Start()
        {
            float randomAngle = UnityEngine.Random.Range(0, 360);
            transform.rotation = Quaternion.AngleAxis(randomAngle, transform.forward);
        }

        private void FixedUpdate()
        {
            invicibleTimer -= Time.fixedDeltaTime;
            if(invicibleTimer < 0.0f)
            {
                canImpact = true;
            }
        }

        private void LateUpdate()
        {
            oldVelocity = rb.velocity;
        }

        private void OnCollisionEnter(Collision col)
        {
            if (canImpact)
            {
                GameObject impact = GameObject.Instantiate<GameObject>(impactFX, col.GetContact(0).point, Quaternion.identity);
                impact.transform.up = col.GetContact(0).normal;
                if (col.gameObject.TryGetComponent<EnemyIdentifier>(out EnemyIdentifier enemy))
                {
                    enemy.DeliverDamage(enemy.gameObject, oldVelocity, col.GetContact(0).point, 1.0f, false);
                    MonoSingleton<StyleHUD>.Instance.AddPoints(100, "hydraxous.ultrafunguns.egged");
                }
                Destroy(gameObject);
            }
        }
    }
}
