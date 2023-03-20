﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UltraFunGuns.Util;
using UnityEngine;

namespace UltraFunGuns
{
    public class DeadlyPlushie : MonoBehaviour, IUFGInteractionReceiver
    {
        private UltraFunGunBase.ActionCooldown impactCooldown = new UltraFunGunBase.ActionCooldown(0.1f);
        private Rigidbody rb;

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
                PlayPlushieAudio();
            }
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
                
            }

            if(impactCooldown.CanFire())
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
            Explode(transform.position+col.GetContact(0).normal * 0.24f);
        }


        private void Explode(params Vector3[] positions)
        {
            for (int i = 0; i < positions.Length; i++)
            {
                GameObject newExplosionFX = Instantiate(Prefabs.ShittyExplosionFX, positions[i], Quaternion.identity);
                Prefabs.ShittyExplosionSound.Asset.PlayAudioClip(positions[i], 1, 1, 0.6f);
            }

            Destroy(gameObject);
        }

        public void Shot(BeamType beamType)
        {
            VirtualExplosion explosion = new VirtualExplosion(transform.position, 30.0f);

            EnemyIdentifier[] enemies = explosion.GetAffectedEnemies();

            List<Vector3> positions = enemies.Select(x => x.transform.position).ToList();

            positions.Add(transform.position);

            Explode(positions.ToArray());
        }

        public bool Parried(Vector3 aimVector)
        {
            return false;
        }

        public void Interact(UFGInteractionEventData interaction)
        {
            return;
        }

        public Vector3 GetPosition()
        {
            return transform.position;
        }
    }
}