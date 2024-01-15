using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using UltraFunGuns.Util;
using UnityEngine;

namespace UltraFunGuns
{
    public class DeadlyPlushie : MonoBehaviour, IUFGInteractionReceiver, ICleanable, IRevolverBeamShootable, ICoinTarget
    {
        private UltraFunGunBase.ActionCooldown impactCooldown = new UltraFunGunBase.ActionCooldown(0.1f);
        private Rigidbody rb;

        public GameObject sourceWeapon = null;
        public float damage = 1.0f;
        private bool dying = false;

        private void Awake()
        {
            Fix();
        }

        private AudioSource plushAudioSrc;

        //This will repurpose the plushie object for use as a projectile.
        private void Fix()
        {
            gameObject.AddComponent<DestroyAfterTime>().TimeLeft = 20.0f;
            rb = gameObject.GetComponent<Rigidbody>();
            impactCooldown.AddCooldown();
            if(transform.TryGetComponent<ItemIdentifier>(out ItemIdentifier itemId))
            {
                Destroy(itemId);
            }

            if(transform.Find("SpecialPutDownSound").TryGetComponent<AudioSource>(out AudioSource audio))
            {
                plushAudioSrc = audio;
                plushAudioSrc.spatialBlend = 0.87f;
                PlayPlushieAudio();
            }

            Collider col = gameObject.GetComponentInChildren<Collider>();
            if(col != null)
            {
                col.gameObject.layer = 14;
            }


            GameObject sphereHitbox = new GameObject(gameObject.name + " Hitbox");
            sphereHitbox.transform.parent = transform;
            sphereHitbox.transform.localPosition = Vector3.zero;
            sphereHitbox.transform.localRotation = Quaternion.identity;
            sphereHitbox.layer = 10;

            SphereCollider shootHitbox = sphereHitbox.AddComponent<SphereCollider>();
            shootHitbox.isTrigger = true;
            shootHitbox.radius = 1f;
        }

        
        //Plays the special audio attatched to the plushie
        private void PlayPlushieAudio()
        {
            if(plushAudioSrc != null)
            {
                plushAudioSrc.Play();
            }
        }

        private void LateUpdate()
        {
            if(rb != null)
            {
                lastVelo = rb.velocity;
            }
        }

        private Vector3 lastVelo;

        private void OnCollisionEnter(Collision collision)
        {

            if (collision.IsCollisionEnemy(out EnemyIdentifier eid))
            {
                eid.DeliverDamage(eid.gameObject, collision.relativeVelocity, collision.GetContact(0).normal, damage, true, 0.5f, sourceWeapon);
            }

            if (impactCooldown.CanFire())
            {
                Impact(collision);
            }else
            {
                rb.velocity = Vector3.Reflect(lastVelo, collision.GetContact(0).normal) + (Vector3.up*0.25f*lastVelo.magnitude);
            }
        }

        //When we collide with something
        private void Impact(Collision col)
        {
            if (dying)
                return;

            dying = true;

            Explode(transform.position+col.GetContact(0).normal * 0.24f);
        }


        private void Explode(params Vector3[] positions)
        {
            if (positions == null)
                positions = new Vector3[] { transform.position };

            for (int i = 0; i < positions.Length; i++)
            {
                GameObject newExplosionFX = Instantiate(Prefabs.ShittyExplosionFX, positions[i], Quaternion.identity);
                Prefabs.ShittyExplosionSound?.PlayAudioClip(positions[i], 1, 1, 0.6f);
            }

            Destroy(gameObject);
        }


        private void Shot(BeamType beamType)
        {
            dying = true;
            VirtualExplosion explosion = new VirtualExplosion(transform.position, 30.0f);

            EnemyIdentifier[] enemies = explosion.GetAffectedEnemies();

            foreach(EnemyIdentifier enemy in enemies)
            {
                enemy.DeliverDamage(enemy.gameObject, enemy.transform.position - transform.position, enemy.transform.position, damage, true, 1.0f, sourceWeapon);
            }

            List<Vector3> positions = enemies.Select(x => x.transform.position).ToList();

            positions.Add(transform.position);

            Explode(positions.ToArray());
        }


        public bool Interact(UFGInteractionEventData interaction)
        {
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

        public void Cleanup()
        {
            Explode();
        }

        public void OnRevolverBeamHit(RevolverBeam beam, ref RaycastHit hit)
        {
            Shot(beam.beamType);
        }

        public bool CanRevolverBeamHit(RevolverBeam beam, ref RaycastHit hit)
        {
            return true;
        }

        public Transform GetCoinTargetPoint(Coin coin)
        {
            return transform;
        }

        public bool CanBeCoinTargeted(Coin coin)
        {
            return true;
        }

        public void OnCoinReflect(Coin coin, RevolverBeam beam) {}

        public int GetCoinTargetPriority(Coin coin)
        {
            return 2;
        }
    }
}
