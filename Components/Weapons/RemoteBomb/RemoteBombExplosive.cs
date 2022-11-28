using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace UltraFunGuns
{
    public class RemoteBombExplosive : MonoBehaviour
    {
        private RemoteBomb weapon;
        private NewMovement player;
        private EnemyIdentifier stuckTarget;
        private Rigidbody rb;
        private Renderer indicatorLight;


        public float armTime = 0.65f;
        public float blinkInterval = 1.0f;
        public float blinkTime = 0.05f;
        
        private bool armed = false;
        private bool landed = false;
        private bool alive = true;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            indicatorLight = transform.Find("BombMesh/Blinker").GetComponent<Renderer>();
        }

        public void Initiate(RemoteBomb remoteBomb, NewMovement newMovement)
        {
            this.weapon = remoteBomb;
            this.player = newMovement;
            Thrown();
        }

        public void SetVelocity(Vector3 newVelocity)
        {
            if(rb)
            {
                rb.velocity = newVelocity;
            }
        }

        private void Thrown()
        {
            StartCoroutine(ArmExplosive());
            StartCoroutine(DoIndicator());
        }

        private IEnumerator ArmExplosive()
        {
            float timer = armTime;
            while(timer > 0.0f)
            {
                timer -= Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
            armed = true;
        }

        private IEnumerator DoIndicator()
        {
            if(indicatorLight != null)
            {
                while (alive)
                {
                    if (!armed)
                    {
                        indicatorLight.material.color = Color.green;
                        yield return new WaitForEndOfFrame();
                    }
                    else
                    {
                        float timer = blinkInterval;
                        indicatorLight.material.color = Color.black;
                        while (timer > 0.0f)
                        {
                            timer -= Time.deltaTime;
                            yield return new WaitForEndOfFrame();
                        }

                        timer = blinkTime;
                        indicatorLight.material.color = Color.red;
                        while (timer > 0.0f)
                        {
                            timer -= Time.deltaTime;
                            yield return new WaitForEndOfFrame();
                        }
                    }
                }
            }   
        }

        public bool CanDetonate()
        {
            return armed;
        }

        public bool Detonate(bool force = false)
        {
            if(force || armed)
            {
                Debug.Log("Remote Boom!");
                Destroy(gameObject);
                return true;
            }

            return false;
        }

        private void StickToEnemy(Transform newParent, EnemyIdentifier enemy, Vector3 normal)
        {
            stuckTarget = enemy;

            StickToThing(newParent, normal);
        }

        private void StickToThing(Transform newParent, Vector3 normal)
        {
            transform.parent = newParent;
            transform.forward = normal;
        }

        private void OnCollisionEnter(Collision col)
        {
            if (col.gameObject.tag == "Floor" || col.gameObject.layer == LayerMask.GetMask("Environment"))
            {
                
                return;
            }

            if (col.collider.TryGetComponent<EnemyIdentifierIdentifier>(out EnemyIdentifierIdentifier enmy))
            {
                if (!enmy.eid.dead)
                {
                    StickToEnemy(col.collider.transform, enmy.eid, col.GetContact(0).normal);
                    return;
                }
            }

            if (col.collider.TryGetComponent<EnemyIdentifier>(out EnemyIdentifier enemy))
            {
                if (!enemy.dead)
                {
                    StickToEnemy(col.collider.transform, enemy, col.GetContact(0).normal);
                    return;
                }
            }
        }
    }
}
