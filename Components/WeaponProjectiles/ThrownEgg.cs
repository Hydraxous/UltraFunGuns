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
        private float invicibleTimer = 0.02f;
        private bool canImpact = false;
        private bool impacted = false;

        private void Awake()
        {
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

        //call when egg is shot
        public void Explode()
        {

        }

        //Call when player grapples the egg should heal player for 10 hp
        private void Cracked()
        {
            
        }

        private void Collide(Collision col)
        {
            //Fix this TODO also add code for shooting it and habving it explode
            EnemyIdentifier enemy;
            GameObject impact = GameObject.Instantiate<GameObject>(impactFX, col.GetContact(0).point, Quaternion.identity);
            impact.transform.up = col.GetContact(0).normal;
            impact.transform.parent = col.transform;
            if ((col.gameObject.TryGetComponent<EnemyIdentifier>(out enemy) && !enemy.dead) || (col.gameObject.TryGetComponent<EnemyIdentifierIdentifier>(out EnemyIdentifierIdentifier enemyPart) && !enemyPart.eid.dead))
            {
                enemy.DeliverDamage(enemy.gameObject, oldVelocity, col.GetContact(0).point, 1.0f, false);
                MonoSingleton<StyleHUD>.Instance.AddPoints(300, "hydraxous.ultrafunguns.egged");

            }
            Destroy(gameObject);
        }

        private void OnCollisionEnter(Collision col)
        {
            if (canImpact && !impacted && col.gameObject.layer != 14 && col.gameObject.layer != 20)
            {
                Collide(col);
            }
        }
    }
}
