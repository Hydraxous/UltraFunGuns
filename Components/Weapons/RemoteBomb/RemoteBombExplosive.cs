using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace UltraFunGuns
{
    public class RemoteBombExplosive : MonoBehaviour
    {
        private GameObject explosionPrefab;

        private RemoteBomb weapon;
        private NewMovement player;
        private EnemyIdentifier stuckTarget;
        private Rigidbody rb;
        private Renderer indicatorLight;

        public float armTime = 0.15f;
        public float blinkInterval = 0.75f;
        public float blinkTime = 0.05f;
        public float parryForce = 150.0f;

        private bool thrown = false;
        private bool armed = false;
        private bool landed = false;
        private bool alive = true;

        private bool colled = false;

        private void Awake()
        {
            HydraLoader.prefabRegistry.TryGetValue("RemoteBomb_Explosive_Explosion", out explosionPrefab);
            rb = GetComponent<Rigidbody>();
            indicatorLight = transform.Find("BombMesh/Blinker").GetComponent<Renderer>();
        }

        public void Initiate(RemoteBomb remoteBomb, NewMovement newMovement)
        {
            this.weapon = remoteBomb;
            this.player = newMovement;
            Thrown();
        }

        private void Update()
        {
            if(alive && thrown)
            {
                if(weapon == null)
                {
                    Detonate(true);
                }
            }
        }

        public void SetVelocity(Vector3 newVelocity)
        {
            if (rb)
            {
                rb.velocity = newVelocity;
            }
        }

        public bool Parriable()
        {
            return (!landed && !armed);
        }

        public void Parry(Vector3 direction)
        { 
            armed = true;
            SetVelocity(direction.normalized * parryForce);
        }

        private void Thrown()
        {
            thrown = true;
            StartCoroutine(ArmExplosive());
            StartCoroutine(DoIndicator());
        }

        private IEnumerator ArmExplosive()
        {
            float timer = armTime;
            while(timer > 0.0f && !armed)
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
                        indicatorLight.material.SetColor("_EmissiveColor", Color.green);
                        yield return new WaitForEndOfFrame();
                    }
                    else
                    {
                        float timer = blinkInterval;
                        indicatorLight.material.SetColor("_EmissiveColor", Color.black);

                        while (timer > 0.0f)
                        {
                            timer -= Time.deltaTime;
                            yield return new WaitForEndOfFrame();
                        }

                        timer = blinkTime;
                        indicatorLight.material.SetColor("_EmissiveColor", Color.red);

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
                alive = false;
                GameObject newBoom = Instantiate<GameObject>(explosionPrefab, transform.position, Quaternion.identity);
                newBoom.transform.up = transform.forward;
                weapon.BombDetonated(this);
                Destroy(gameObject);
                return true;
            }

            return false;
        }

        private void StickToEnemy(Transform newParent, EnemyIdentifier enemy, Vector3 normal)
        {
            stuckTarget = enemy;
            //TODO apply minor damage here
            StickToThing(newParent, normal);
        }

        private void StickToThing(Transform newParent, Vector3 normal)
        {
            landed = true;
            if(rb)
            {
                rb.isKinematic = true;
            }
            transform.parent = newParent;
            transform.forward = normal;
        }

        private bool CanStickToThing(GameObject thing)
        {
            if(thing.tag == "Floor" || thing.tag == "Wall")
            {
                return true;
            }

            switch(thing.layer)
            {
                case 2:
                    return true;
                case 8:
                    return true;
                case 10:
                    return true;
                case 11:
                    return true;
                case 24:
                    return true;
                case 25:
                    return true;
                case 26:
                    return true;
                default:
                    return false;
            }
        }

        private void OnCollisionEnter(Collision col)
        {
            if(landed || !armed)
            {
                return;
            }

            if(!colled)
            {
                colled = true;
                Debug.Log(HydraUtils.CollisionInfo(col));

            }

            bool stick = CanStickToThing(col.gameObject);

            if(stick)
            {
                StickToThing(col.transform, col.GetContact(0).normal);
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
