using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using UnityEngine;

namespace UltraFunGuns
{
    //Egg projectile script created by EggToss and EggSplosion.
    public class ThrownEgg : MonoBehaviour, IUFGInteractionReceiver, ICoinTarget, IRevolverBeamShootable, ISharpshooterTarget
    {
        [UFGAsset("EggImpactFX")] private static GameObject impactFX;
        [UFGAsset("EggSplosion")] private static GameObject eggsplosionPrefab;

        [Configgy.Configgable("Weapons/Egg Toss/Egg")]
        private static float velocityDamageMultiplier = 0.035f;

        [Configgy.Configgable("Weapons/Egg Toss/Egg/Eggsplosion", displayName:"Damage Multiplier")]
        private static float eggsplosionEggDamageMultiplier = 0.75f;

        [Configgy.Configgable("Weapons/Egg Toss/Egg/Eggsplosion", displayName:"Velocity Damage Multiplier")]
        private static float eggsplosionEggDamageVelocityMultiplier = 0.25f;

        private Rigidbody rb;
        private CapsuleCollider eggCollider;
        public Vector3 oldVelocity;
        private float invicibleTimer = 0.015f;
        private bool canImpact = false;
        private bool impacted = false;
        public bool isEggsplosionEgg = false;
        public bool dropped = false;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            eggCollider = GetComponent<CapsuleCollider>();

            GameObject limbHitbox = transform.Find("Sphere").gameObject;
            if (limbHitbox != null)
            {
                //Awful hack to make piercing revolvers work
                limbHitbox.layer = 10;
                limbHitbox.tag = "Breakable";
            }

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

            if(isEggsplosionEgg)
            {
                //rb.velocity = oldVelocity;
            }
        }

        private void LateUpdate()
        {
            oldVelocity = rb.velocity;
        }

        public void Explode()
        {
            if(!isEggsplosionEgg && !impacted)
            {
                MonoSingleton<StyleHUD>.Instance.AddPoints(50, "hydraxous.ultrafunguns.eggsplosion");
                MonoSingleton<TimeController>.Instance.ParryFlash();
                Instantiate<GameObject>(eggsplosionPrefab, transform.position, Quaternion.identity).GetComponent<EggSplosion>();
                Destroy(gameObject);
            }  
        }

        private void Collide(Collision col)
        {

            if (!impacted)
            {
                impacted = true;

                EnemyIdentifier enemy = null;
                GameObject impact = GameObject.Instantiate<GameObject>(impactFX, col.GetContact(0).point, Quaternion.identity);
                impact.transform.up = col.GetContact(0).normal;
                impact.transform.parent = col.transform;
                float damage = rb.velocity.magnitude * velocityDamageMultiplier; //Scales damage from speed of egg

                if (col.gameObject.TryGetComponent<EnemyIdentifierIdentifier>(out EnemyIdentifierIdentifier enemyPart))
                {
                    enemy = enemyPart.eid;
                }

                if (enemy != null)
                {
                    if (!enemy.dead)
                    {
                        if(!isEggsplosionEgg)
                        {
                            enemy.DeliverDamage(enemy.gameObject, oldVelocity, col.GetContact(0).point, damage, false);
                            if (dropped)
                            {
                                MonoSingleton<StyleHUD>.Instance.AddPoints(110, "hydraxous.ultrafunguns.eggstrike");
                            }
                            else
                            {
                                MonoSingleton<StyleHUD>.Instance.AddPoints(110, "hydraxous.ultrafunguns.egged");
                            }
                        }
                        else
                        {
                            enemy.DeliverDamage(enemy.gameObject, oldVelocity*eggsplosionEggDamageVelocityMultiplier, col.GetContact(0).point, damage*eggsplosionEggDamageMultiplier, false);
                            MonoSingleton<StyleHUD>.Instance.AddPoints(10, "hydraxous.ultrafunguns.egged");
                        }
                    }
                    else
                    {
                        UnityEngine.Physics.IgnoreCollision(eggCollider, col.collider, true);
                        impacted = false;
                    }
                }

            }

            if (impacted)
            {
                Destroy(gameObject);
            }

        }

        private void Collide(Collider col)
        {
            if (!impacted)
            {
                impacted = true;
                
                EnemyIdentifier enemy = null;
                GameObject impact = GameObject.Instantiate<GameObject>(impactFX, transform.position, Quaternion.identity);
                impact.transform.Find("impactSpotQuad").gameObject.SetActive(false);
                Vector3 collisionNormalGuess = MonoSingleton<NewMovement>.Instance.transform.position - transform.position;
                impact.transform.up = collisionNormalGuess;
                impact.transform.parent = col.transform;
                float damage = rb.velocity.magnitude * velocityDamageMultiplier; //Scales damage from speed of egg :)

                if (col.gameObject.TryGetComponent<EnemyIdentifierIdentifier>(out EnemyIdentifierIdentifier enemyPart))
                {
                    enemy = enemyPart.eid;
                }else 
                {
                    col.gameObject.TryGetComponent<EnemyIdentifier>(out enemy);
                }

                if (enemy != null)
                {
                    if (!enemy.dead)
                    {
                        if (!isEggsplosionEgg)
                        {
                            enemy.DeliverDamage(enemy.gameObject, oldVelocity, transform.position, damage, false);
                            if (dropped)
                            {
                                MonoSingleton<StyleHUD>.Instance.AddPoints(110, "hydraxous.ultrafunguns.eggstrike");
                            }
                            else
                            {
                                MonoSingleton<StyleHUD>.Instance.AddPoints(110, "hydraxous.ultrafunguns.egged");
                            }
                        }
                        else
                        {
                            enemy.DeliverDamage(enemy.gameObject, oldVelocity * eggsplosionEggDamageVelocityMultiplier, transform.position, damage * eggsplosionEggDamageMultiplier, false);
                            MonoSingleton<StyleHUD>.Instance.AddPoints(10, "hydraxous.ultrafunguns.egged");
                        }
                    }
                    else
                    {
                        UnityEngine.Physics.IgnoreCollision(eggCollider, col, true);
                        impacted = false;
                    }
                }
            }

            if (impacted)
            {
                Destroy(gameObject);
            }
        }

        private void OnCollisionEnter(Collision col)
        {
            if (col.gameObject.TryGetComponent<ThrownEgg>(out ThrownEgg egggggggyDontUseThisVariable))
            {
                Physics.IgnoreCollision(eggCollider, col.collider, true);
                return;
            }

            if (canImpact || isEggsplosionEgg)
            {
                Collide(col);
            }
        }

        private void OnTriggerEnter(Collider col)
        {

            if ((canImpact || isEggsplosionEgg) && col.gameObject.layer != 20 && (col.gameObject.layer == 10 || col.gameObject.layer == 11) && !col.isTrigger )
            {
                Collide(col);
            }
        }


        public bool Interact(UFGInteractionEventData interaction)
        {
            if (interaction.ContainsTag("shot"))
            {
                Explode();
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
            return targetQuery.CheckTargetable(transform.position);
        }

        public Transform GetCoinTargetPoint(Coin coin)
        {
            return transform;
        }

        public bool CanBeCoinTargeted(Coin coin)
        {
            return !isEggsplosionEgg;
        }

        public void OnCoinReflect(Coin coin, RevolverBeam beam)
        {
            Explode();
        }

        public int GetCoinTargetPriority(Coin coin)
        {
            return 2;
        }

        public void OnRevolverBeamHit(RevolverBeam beam, ref RaycastHit hit)
        {
            Explode();
        }

        public bool CanRevolverBeamHit(RevolverBeam beam, ref RaycastHit hit)
        {
            return !isEggsplosionEgg;
        }

        public bool CanBeSharpshot(RevolverBeam beam, RaycastHit hit)
        {
            return !isEggsplosionEgg;
        }

        public Vector3 GetSharpshooterTargetPoint()
        {
            return transform.position;
        }

        public void OnSharpshooterTargeted(RevolverBeam beam, RaycastHit hit) {}

        public int GetSharpshooterTargetPriority()
        {
            return 1;
        }
    }
}
