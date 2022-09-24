using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    public class ThrownDodgeball : MonoBehaviour
    {
        private UltraFunGunBase.ActionCooldown hurtCooldown = new UltraFunGunBase.ActionCooldown(0.05f);
        private UltraFunGunBase.ActionCooldown hitSoundCooldown = new UltraFunGunBase.ActionCooldown(0.015f);

        public GameObject dodgeballPopFXPrefab;
        public GameObject impactSound;

        public Transform ballMesh;

        public Dodgeball dodgeballWeapon;

        public Rigidbody rb;
        private Animator animator;
        public Vector3 sustainedVelocity = Vector3.zero;
        public float reboundVelocityMultiplier = 5f;
        public int bounces = 50;
        public int timesBounced = 12;
        public float lifeTime = 15.0f;
        public float hitDamageMultiplier = 0.05f;

        private AudioSource bigHitSound;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            animator = GetComponent<Animator>();
            ballMesh = transform.Find("DodgeballMesh");
            HydraLoader.prefabRegistry.TryGetValue("DodgeballPopFX", out dodgeballPopFXPrefab);
            HydraLoader.prefabRegistry.TryGetValue("DodgeballImpactSound", out impactSound);
            bigHitSound = transform.Find("Audios/BigHit").GetComponent<AudioSource>();

        }

        private void Start()
        {
            Invoke("Pop", lifeTime);
        }

        private void FixedUpdate()
        {
            rb.velocity = sustainedVelocity;
        }

        private void Pop()
        {
            GameObject.Instantiate<GameObject>(dodgeballPopFXPrefab, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }

        private void DoHit(Collision col)
        {
            --bounces;
            ++timesBounced;
            animator.Play("ThrownDodgeballSquish");
            EnemyIdentifier enemy = null;
            GameObject objectHit = null;

            if(col.collider.TryGetComponent<EnemyIdentifier>(out enemy))
            {
                objectHit = enemy.gameObject;
            }

            if(col.collider.TryGetComponent<EnemyIdentifierIdentifier>(out EnemyIdentifierIdentifier enemyIDID))
            {
                enemy = enemyIDID.eid;
                objectHit = enemyIDID.gameObject;
            }

            if(col.collider.TryGetComponent<Breakable>(out Breakable breakable))
            {
                breakable.Break();
            }

            if (col.collider.TryGetComponent<Glass>(out Glass glass))
            {
                glass.Shatter();
            }

            if (enemy != null && hurtCooldown.CanFire())
            {
                if(!enemy.dead)
                {
                    hurtCooldown.AddCooldown();
                    enemy.DeliverDamage(objectHit, sustainedVelocity.normalized, col.GetContact(0).point, sustainedVelocity.magnitude * hitDamageMultiplier, true, 0, dodgeballWeapon.gameObject);
                    MonoSingleton<StyleHUD>.Instance.AddPoints(30, "hydraxous.ultrafunguns.dodgeballhit", dodgeballWeapon.gameObject, enemy);
                    bigHitSound.Play();
                }else
                {
                    UnityEngine.Physics.IgnoreCollision(col.collider, GetComponent<SphereCollider>());
                }
                
            }

            if(hitSoundCooldown.CanFire())
            {
                hitSoundCooldown.AddCooldown();
                GameObject.Instantiate<GameObject>(impactSound, transform);
            }

        }

        private void OnCollisionEnter(Collision col)
        {
            if (col.collider.TryGetComponent<NewMovement>(out NewMovement player))
            {
                UnityEngine.Physics.IgnoreCollision(player.playerCollider, GetComponent<SphereCollider>());
                UnityEngine.Physics.IgnoreCollision(col.collider, GetComponent<SphereCollider>());

                return;
            }
            DoHit(col);
            transform.forward = col.GetContact(0).normal;
            sustainedVelocity = Vector3.Reflect(sustainedVelocity, col.GetContact(0).normal).normalized * (reboundVelocityMultiplier * timesBounced+1);

            if(bounces <= 0)
            {
                Pop();
            }
        }

        private void OnTriggerEnter(Collider col)
        {
            if(col.gameObject.name == "DodgeballCatcher")
            {
                dodgeballWeapon.CatchBall();
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            dodgeballWeapon.dodgeBallActive = false;
        }
    }
}
