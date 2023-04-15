using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace UltraFunGuns
{

    public class DestroyAfterTime : MonoBehaviour, ICleanable
    {
        public float TimeLeft = 2.0f;

        void Start()
        {
            CheckComponents();
        }

        private void CheckComponents()
        {
            if (TryGetComponent<AudioSource>(out AudioSource src))
            {
                float audioTimeLeft = src.clip.length - src.time;
                SetMaxTime(audioTimeLeft);
            }

            if (TryGetComponent<TrailRenderer>(out TrailRenderer trail))
            {
                //trail.emitting = false;
                SetMaxTime(trail.time);
                StartCoroutine(FadeOutTrail(trail));
            }

            if (TryGetComponent<ParticleSystem>(out ParticleSystem particleSystem))
            {
                ParticleSystem.EmissionModule emitter = particleSystem.emission;
                emitter.enabled = false;
                SetMaxTime(emitter.rateOverTime.constant);
                SetMaxTime(particleSystem.main.duration);
            }
        }

        private IEnumerator FadeOutTrail(TrailRenderer trail)
        {
            float startTime = trail.time;
            float startWidth = trail.widthMultiplier;
            while (trail.time > 0.0f)
            {
                trail.time -= Time.deltaTime;
                trail.widthMultiplier = Mathf.Lerp(startWidth, 0.0f, Mathf.InverseLerp(startTime, 0.0f, trail.time));
                yield return new WaitForEndOfFrame();
            }
        }

        public void SetMaxTime(float newMaxTime)
        {
            TimeLeft = Mathf.Max(TimeLeft, newMaxTime);
        }

        void Update()
        {
            if (TimeLeft <= 0.0f)
            {
                Destroy(gameObject);
            }

            TimeLeft -= Time.deltaTime;
        }

        public static void PreserveComponents(Transform transf)
        {
            AudioSource[] audios = transf.GetComponentsInChildren<AudioSource>();

            TrailRenderer[] trails = transf.GetComponentsInChildren<TrailRenderer>();

            ParticleSystem[] particles = transf.GetComponentsInChildren<ParticleSystem>();

            for (int i = 0; i < audios.Length; i++)
            {
                PreserveComponent(audios[i].transform);
            }

            for (int i = 0; i < trails.Length; i++)
            {
                PreserveComponent(trails[i].transform);
            }

            for (int i = 0; i < particles.Length; i++)
            {
                PreserveComponent(particles[i].transform);
            }
        }

        private static void PreserveComponent(Transform tf, bool changeParent = true)
        {
            if (tf != null)
            {
                tf.gameObject.AddComponent<DestroyAfterTime>();
                if (changeParent)
                {
                    tf.parent = null;
                }
            }
        }

        public void Cleanup()
        {
            Destroy(gameObject);
        }
    }

}
