using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.Collections;

namespace UltraFunGuns
{
    public class CanProjectile : MonoBehaviour
    {
        public GameObject canPopFX;
        public GameObject canShrapnelExplosion;

        public bool impactedEnemy = false;
        public bool canBeParried = false;
        public bool parried = false;
        public bool bouncedOffOtherCan = false;
        public bool sleeping = false;
        public bool revived = false;
        public bool dead = false;
        public bool tracking = false;

        public float trackTargetReachDistance = 1.0f;
        public float trackSpeed = 1.0f;


        public float reviveParryThrowForce;

        public float killTime = 4.5f;
        private float killTimer = 0.0f;

        private Rigidbody rb;

        public Vector3 bounceForce;
        public Vector3 oldVelocity;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        private void Start()
        {
            
        }

        private void Update()
        {
            killTimer -= Time.deltaTime;
            if(!(killTimer > 0.0f))
            {
                Die();
            }
        }

        private void LateUpdate()
        {
            oldVelocity = rb.velocity;
        }

        public void FlyTowards(Vector3 point, float multiplier = 1.0f, float speed = 1.0f)
        {
            Vector3 direction = point - transform.position;

            if(speed != 1.0f)
            {
                direction.Normalize();
                direction *= speed;
            }

            AlterVelocity(direction*multiplier, false);
        }

        public void AlterVelocity(Vector3 direction, bool add = true)
        {
            if(add)
            {
                rb.velocity += direction;
            }else
            {
                rb.velocity = direction;
            }
        }

        private void OnCollisionEnter(Collision col)
        {
            if(sleeping || dead)
            {
                return;
            }

            if(col.gameObject.tag == "Floor")
            {
                sleeping = true;
                canBeParried = false;
            }

            if(col.gameObject.TryGetComponent<CanProjectile>(out CanProjectile anotherCan))
            {
                anotherCan.AlterVelocity(col.GetContact(0).normal * (col.relativeVelocity.magnitude), true);
            }
        }

        public void TrackTarget(Transform target)
        {
            if(!tracking)
            {
                StartCoroutine(DoTargetTrack(target));
            }
        }

        private IEnumerator DoTargetTrack(Transform target)
        {
            tracking = true;
            Vector3 targetDistance = target.position - transform.position;
            while(targetDistance.magnitude > trackTargetReachDistance && tracking)
            {
                AlterVelocity(targetDistance.normalized * trackSpeed, false);
                yield return new WaitForEndOfFrame();
                targetDistance = target.position - transform.position;
            }
            tracking = false;
        }

        public void HitEnemy()
        {
            if(dead)
            {
                return;
            }

            canBeParried = true;
            if(parried && revived)
            {
                revived = false;
            }

            //shoot can at where player will be


        }

        public void HitOtherCan()
        {
            canBeParried = true;
            if(parried)
            AlterVelocity(-oldVelocity, false);

        }

        public void Bounce()
        {
            if(dead)
            {
                return;
            }

            if(sleeping)
            {
                revived = true;
                sleeping = false;
            }
            canBeParried = true;
            AlterVelocity(bounceForce, false);
        }

        public void Explode(Vector3 direction, int strength = 0)
        {
            //SpawnCanShrapnelExplosion strength 0 is normal explosion when parried, 1 is big explosion when can banked and parried, 2 is 360 shrapnel explosion and huge shot by railgun
            TimeController.Instance.ParryFlash();
            GameObject newExplosion = Instantiate<GameObject>(canShrapnelExplosion, transform.position, Quaternion.identity);
            newExplosion.transform.forward = direction;
            Die();
            newExplosion.gameObject.GetComponent<CanExplosion>().Explode(strength);
        }

        public void Parry()
        {
            if (dead || sleeping || !canBeParried)
            {
                return;
            }

            canBeParried = false;

            if(revived && !parried && !bouncedOffOtherCan)
            {
                revived = false;
                parried = true;
                AlterVelocity(CameraController.Instance.transform.TransformDirection(0, 0, 1).normalized*reviveParryThrowForce,false);
                return;
            }

            if(bouncedOffOtherCan)
            {
                Explode(CameraController.Instance.transform.TransformDirection(0, 0, 1).normalized);
            }
            else
            {
                Explode(CameraController.Instance.transform.TransformDirection(0, 0, 1).normalized, 1);

            }

            parried = true;
            dead = true;
        }

        

        //pops soda does tiny aoe damage
        private void Die()
        {
            //instantiate tiny explosion pop fx that does tiny aoe damage.
            dead = true;
            gameObject.SetActive(false);
            Destroy(gameObject, 4.0f);
        }

    }
}
