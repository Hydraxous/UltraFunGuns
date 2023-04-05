using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraFunGuns.Components.Weapons.Melee
{
    public class AggromorphicWeapon : MonoBehaviour
    {
        [SerializeField] private AudioClip[] hitSounds;
        [SerializeField] private AudioClip[] hitEnemySounds;
        [SerializeField] private int hitAnimationVariations = 1;
        public float hitCooldown = 0.3f;
        public float damage = 1f;
        private Animator animator;
        private int currentHitAnimation = 0;

        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        public void Hit()
        {
            animator.Play("Hit", 0, 0);
            
            currentHitAnimation = (UnityEngine.Random.value >= 0.75f) ? UnityEngine.Random.Range(0,hitAnimationVariations-1) : (currentHitAnimation + 1 > (hitAnimationVariations - 1)) ? 0 : currentHitAnimation + 1;
            animator.SetInteger("HitAnim", currentHitAnimation);
        }

        public void PlayHitAudio(Vector3 position, float spatial, bool flesh = false)
        {

            if(flesh)
            {
                hitEnemySounds[UnityEngine.Random.Range(0, hitEnemySounds.Length)].PlayAudioClip(position, UnityEngine.Random.Range(0.85f, 1.1f), 1.0f, spatial);
            }
            else
            {
                hitSounds[UnityEngine.Random.Range(0,hitSounds.Length)].PlayAudioClip(position, UnityEngine.Random.Range(0.85f, 1.1f),1.0f,spatial);
            }
        }

        public void SecretAnimation()
        {
            animator.Play("Secret", 0, 0);
        }

        private void OnEnable()
        {
            animator.Play("Equip", 0, 0);
        }
    }
}
