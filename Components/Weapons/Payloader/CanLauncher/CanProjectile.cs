using System.Collections;
using UnityEngine;

namespace UltraFunGuns
{
    public class CanProjectile : MonoBehaviour, IUFGInteractionReceiver, ICleanable, ICoinTarget, IRevolverBeamShootable, IParriable
    {
        [UFGAsset("CanLauncher_CanExplosion")] private static GameObject canExplosion;
        [UFGAsset("CanLauncher_CanProjectile_BounceFX")] private static GameObject bounceFX;

        public bool trackInsteadOfPredict = true;

        public bool impactedEnemy = false;
        public bool canBeParried = true;
        public bool parried = false;
        public bool impactedOtherCan = false;
        public bool sleeping = false;
        public bool bounced = false;
        public bool dead = false;
        public bool tracking = false;
        public bool superCharge = false;
        public bool banked = false;

        public float bankVelocityMultiplier = 0.5f;

        public float trackTargetReachDistance = 1.0f;
        public float trackSpeed = 60.0f;

        public float enemyHitDamage = 1.3f;

        public float reviveParryThrowForce = 180.0f;

        private float lastParryTime;
        private static float parryCooldown = 0.5f;

        public float killTime = 4.5f;
        private float killTimer = 0.0f;

        private UltraFunGunBase.ActionCooldown damageCD = new UltraFunGunBase.ActionCooldown(0.15f);

        private Rigidbody rb;

        public Vector3 bounceForce = new Vector3(0,16.0f,0);
        public float returnToPlayerForce = 80.0f;
        public float playerPredictTime = 0.25f;
        public float upwardsForceToPlayer = 0.05f;
        public Vector3 oldVelocity;

        private Transform lastEnemyHit;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            GameObject hitbox = transform.Find("Sphere").gameObject;
            if(hitbox != null)
            {
                hitbox.layer = 10;
                hitbox.tag = "Breakable";
            }
        }

        private void Start()
        {
            killTimer = killTime;
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

        //idk lol
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

        //Directly modifies velocity
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

        private Vector3 PathToPlayer()
        {
            Vector3 currentProjectedPosition = CameraController.Instance.transform.TransformPoint(0, 0, 0.25f) + (NewMovement.Instance.rb.velocity * playerPredictTime);

            Vector3 newVelocity = (currentProjectedPosition - transform.position).normalized * returnToPlayerForce;
            newVelocity.y += (upwardsForceToPlayer * newVelocity.magnitude);
            return newVelocity;
        }

        public void TrackLastEnemy()
        {
            if(lastEnemyHit != null)
            {
                TrackTarget(lastEnemyHit);
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
            HydraLogger.Log($"({gameObject.name}) can hit enemy ({enemy.name})");
            if(dead || enemy.dead || enemy == null)
            {
                return;
            }

            impactedEnemy = true;
            banked = true;

            if(damageCD.CanFire())
            {
                damageCD.AddCooldown();
                enemy.DeliverDamage(enemy.gameObject, oldVelocity, impactSpot, enemyHitDamage, false);
            }
            lastEnemyHit = enemy.transform;
            SendToPlayer();
        } 

        public void HitOtherCan(CanProjectile otherCan)
        {
            if(dead || sleeping || impactedEnemy || otherCan.sleeping || otherCan.dead)
            {
                return;
            }

            tracking = false;
            otherCan.tracking = false;
            otherCan.banked = true;
            
            impactedOtherCan = true;
            otherCan.impactedOtherCan = true;
            otherCan.TrackLastEnemy();

            if (parried && banked)
            {
                Explode(-oldVelocity, 2);
                if(otherCan.impactedEnemy)
                {
                    otherCan.Explode(oldVelocity, 2);
                }
                HydraLogger.Log("Parried and hit other can");
            }
            else
            {
                banked = true;
                transform.forward = -oldVelocity;
                killTimer += killTime;
                otherCan.transform.forward = oldVelocity;
                otherCan.AlterVelocity(oldVelocity, false);
                SendToPlayer();    
            }
        }

        public void Bounce()
        {
            if(dead)
            {
                return;
            }

            tracking = false;
            bounced = true;
            sleeping = false;

            killTimer += killTime;
            Instantiate<GameObject>(bounceFX, transform.position, Quaternion.identity);
            AlterVelocity(bounceForce, false);
        }

        public bool Parry(Vector3 origin, Vector3 aimVector)
        {
            if (dead || !canBeParried || sleeping)
            {
                return false;
            }

            if(Time.time-lastParryTime < parryCooldown)
            {
                return false;
            }

            lastParryTime = Time.time;
            tracking = false;
            //canBeParried = false;
            killTimer += killTime;

            if (!banked && !bounced)
            {   
                AlterVelocity(aimVector.normalized * reviveParryThrowForce, false);
                rb.AddTorque(200.0f, 0.0f, 0.0f);
                if (!impactedOtherCan)
                {
                    return true;
                }
            }

            parried = true;

            if (impactedOtherCan || (!banked && bounced))
            {
                Explode(CameraController.Instance.transform.TransformDirection(0, 0, 1).normalized, 2);
            }
            else
            {
                Explode(CameraController.Instance.transform.TransformDirection(0, 0, 1).normalized, 1);
            }

            return true;
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

        

        private void OnCollisionEnter(Collision col)
        {
            if (tracking)
            {
                tracking = false;
            }

            if (sleeping || dead)
            {
                return;
            }

            if (col.gameObject.tag == "Floor" || col.gameObject.layer == LayerMask.GetMask("Environment"))
            {
                if (!banked)
                {
                    banked = true;
                    Vector3 newDirection = Vector3.Reflect(oldVelocity*bankVelocityMultiplier, col.GetContact(0).normal);
                    AlterVelocity(newDirection, false);
                    return;
                }
                HydraLogger.Log($"({gameObject.name}) can is sleeping!");
                sleeping = true;
                return;
            }

            if (col.collider.TryGetComponent<CanProjectile>(out CanProjectile anotherCan))
            {
                //anotherCan.AlterVelocity(col.GetContact(0).normal * (col.relativeVelocity.magnitude), true);
                HitOtherCan(anotherCan);
                return;
            }

            if (col.collider.TryGetComponent<EnemyIdentifierIdentifier>(out EnemyIdentifierIdentifier enmy))
            {
                if (!enmy.eid.dead)
                {
                    HitEnemy(enmy.eid, col.GetContact(0).point);//TODO do damage lol
                    return;
                }
            }

            if (col.collider.TryGetComponent<EnemyIdentifier>(out EnemyIdentifier enemy))
            {
                if (!enemy.dead)
                {
                    HitEnemy(enemy, col.GetContact(0).point);//TODO do damage lol
                    return;
                }
            }
        }

        //pops soda does tiny aoe damage later
        private void Die()
        {
            if (dead)
                return;

            //instantiate tiny explosion pop fx that does tiny aoe damage.
            dead = true;
            gameObject.SetActive(false);
            Destroy(gameObject, 4.0f);
        }


        public bool Interact(UFGInteractionEventData interaction)
        {

            if(interaction.ContainsTag("god"))
            {
                Explode(Vector3.up, 3);
                return true;
            }

            if(interaction.ContainsAnyTag("explode", "heavy", "pierce"))
            {
                Explode(interaction.direction, 1);
                return true;
            }

            if (interaction.ContainsTag("shot"))
            {
                Bounce();
                return true;
            }

            return false;
        }

        public Vector3 GetPosition()
        {
            return transform.position;
        }

        public bool Targetable(TargetQuery targetQuery)
        {
            if (sleeping || dead)
                return false;

            return targetQuery.CheckTargetable(transform.position);
        }

        public void Cleanup()
        {
            Die();
        }

        public Transform GetCoinTargetPoint(Coin coin)
        {
            return transform;
        }

        public bool CanBeCoinTargeted(Coin coin)
        {
            //Todo
            return !sleeping && !dead;
        }

        public void OnCoinReflect(Coin coin, RevolverBeam beam) 
        {

        }

        public int GetCoinTargetPriority(Coin coin)
        {
            return 1;
        }

        public void OnRevolverBeamHit(RevolverBeam beam, ref RaycastHit hit)
        {
            switch (beam.beamType)
            {
                case BeamType.Railgun:
                    Explode(Vector3.up, 3);
                    break;
                case BeamType.Revolver:
                    
                    if(!beam.strongAlt)
                    {
                        Bounce();
                        break;
                    }
                    if (sleeping)
                    {
                        Explode(Vector3.up, 2);
                    }
                    else if (bounced)
                    {
                        Vector3 direction = transform.position - beam.transform.position;
                        Explode(direction, 2);
                    }
                    break;
                case BeamType.MaliciousFace:
                    Explode(Vector3.up, 3);
                    break;
                case BeamType.Enemy:
                    Bounce();
                    break;
            }
        }

        public bool CanRevolverBeamHit(RevolverBeam beam, ref RaycastHit hit)
        {
            return !dead;
        }
    }
}
