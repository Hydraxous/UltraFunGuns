using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.Collections;

namespace UltraFunGuns
{
    public class CanProjectile : MonoBehaviour
    {
        public GameObject canExplosion;

        public bool trackInsteadOfPredict = true;

        public bool impactedEnemy = false;
        public bool canBeParried = true;
        public bool parried = false;
        public bool bouncedOffOtherCan = false;
        public bool sleeping = false;
        public bool revived = false;
        public bool dead = false;
        public bool tracking = false;
        public bool superCharge = false;

        public float trackTargetReachDistance = 1.0f;
        public float trackSpeed = 60.0f;

        public float enemyHitDamage = 1.3f;

        public float reviveParryThrowForce = 180.0f;

        public float killTime = 4.5f;
        private float killTimer = 0.0f;

        private UltraFunGunBase.ActionCooldown damageCD = new UltraFunGunBase.ActionCooldown(0.15f);

        private Rigidbody rb;

        public Vector3 bounceForce = new Vector3(0,16.0f,0);
        public float returnToPlayerForce = 80.0f;
        public float playerPredictTime = 0.25f;
        public float upwardsForceToPlayer = 0.05f;
        public Vector3 oldVelocity;

        private void Awake()
        {
            HydraLoader.prefabRegistry.TryGetValue("CanLauncher_CanExplosion", out canExplosion);
            rb = GetComponent<Rigidbody>();
        }

        private void Start()
        {
            killTimer = killTime;
            canBeParried = true;

        }

        private void Update()
        {
            killTimer -= Time.deltaTime;
            if(killTimer <= 0.0f)
            {
                Explode(transform.up, 0);
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
            if(tracking)
            {
                tracking = false;
            }

            if(sleeping || dead)
            {
                return;
            }

            if(col.gameObject.tag == "Floor" && !sleeping)
            {
                Debug.Log("Can Sleeping!");
                sleeping = true;
                canBeParried = false;
                return;
            }

            if(col.collider.TryGetComponent<CanProjectile>(out CanProjectile anotherCan))
            {
                anotherCan.AlterVelocity(col.GetContact(0).normal * (col.relativeVelocity.magnitude), true);
                HitOtherCan(anotherCan);
                return;
            }

            if(col.collider.TryGetComponent<EnemyIdentifierIdentifier>(out EnemyIdentifierIdentifier enmy))
            {
                if (!enmy.eid.dead)
                {
                    HitEnemy(enmy.eid,col.GetContact(0).point);//TODO do damage lol
                    return;
                }
            }

            if(col.collider.TryGetComponent<EnemyIdentifier>(out EnemyIdentifier enemy))
            {
                if(!enemy.dead)
                {
                    HitEnemy(enemy, col.GetContact(0).point);//TODO do damage lol
                    return;
                } 
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

        public void HitEnemy(EnemyIdentifier enemy, Vector3 impactSpot)
        {
            Debug.Log("We hit de enemei");
            if(dead || enemy.dead || enemy == null)
            {
                return;
            }

            impactedEnemy = true;
            revived = false;

            enemy.DeliverDamage(enemy.gameObject, oldVelocity, impactSpot, enemyHitDamage, false);
            SendToPlayer();
        }

        private Vector3 PathToPlayer()
        {
            Vector3 currentProjectedPosition = CameraController.Instance.transform.TransformPoint(0, 0, 0.25f) + (NewMovement.Instance.rb.velocity*playerPredictTime);

            Vector3 newVelocity = (currentProjectedPosition - transform.position).normalized * returnToPlayerForce;
            newVelocity.y += (upwardsForceToPlayer * newVelocity.magnitude);
            return newVelocity;
        }

        public void HitOtherCan(CanProjectile otherCan)
        {
            if(dead || sleeping)
            {
                return;
            }

            canBeParried = true;
            if(parried)
            {
                Explode(-oldVelocity, 1);
                otherCan.Explode(oldVelocity, 2);
                Debug.Log("Parried and hit other can");
            }else
            {
                transform.forward = -oldVelocity;
                killTimer += killTime;
                SendToPlayer();
                
            }

        }

        private void SendToPlayer()
        {
            if (!trackInsteadOfPredict)
            {
                Vector3 newPath = PathToPlayer();
                transform.forward = newPath;
                canBeParried = true;
                AlterVelocity(newPath, false);
            }
            else
            {
                TrackTarget(CameraController.Instance.transform);
            }
        }

        public void Bounce()
        {
            if(dead)
            {
                return;
            }

            if (!sleeping && !revived)
            {
                revived = true;
            }
            revived = true;
            sleeping = false;

            killTimer += killTime;
            canBeParried = true;
            AlterVelocity(bounceForce, false);
        }

        public void Explode(Vector3 direction, int strength = 0)
        {
            //SpawnCanShrapnelExplosion strength 0 is normal explosion when parried, 1 is big explosion when can banked and parried, 2 is 360 shrapnel explosion and huge shot by railgun
            if(strength == 3)
            {
                TimeController.Instance.ParryFlash();
            }
            GameObject newExplosion = Instantiate<GameObject>(canExplosion, transform.position, Quaternion.identity);
            newExplosion.transform.forward = direction;
            Die();
            newExplosion.gameObject.GetComponent<CanExplosion>().Explode(strength);
        }

        public bool Parry()
        {
            if (dead || sleeping || !canBeParried)
            {
                return false;
            }

            tracking = false;

            canBeParried = false;

            killTimer += killTime;

            if (!revived && !parried && !bouncedOffOtherCan && !impactedEnemy)
            {
                parried = true;
                AlterVelocity(CameraController.Instance.transform.TransformDirection(0, 0, 1).normalized*reviveParryThrowForce,false);
                rb.AddTorque(200.0f, 0.0f, 0.0f);//iDk funny
                return true;
            }

            if(bouncedOffOtherCan || superCharge)
            {
                Explode(CameraController.Instance.transform.TransformDirection(0, 0, 1).normalized, 2);
            }
            else
            {
                Explode(CameraController.Instance.transform.TransformDirection(0, 0, 1).normalized, 1);
            }

            parried = true;
            return true;
        }

        //pops soda does tiny aoe damage later
        private void Die()
        {
            //instantiate tiny explosion pop fx that does tiny aoe damage.
            dead = true;
            gameObject.SetActive(false);
            Destroy(gameObject, 4.0f);
        }

    }
}
