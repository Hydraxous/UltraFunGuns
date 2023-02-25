using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    //Used for physics objects to play sounds on impacte
    [RequireComponent(typeof(Rigidbody),typeof(AudioSource))]
    public class PhysicsAudio : MonoBehaviour
    {
        public string bruhMoment;
        public PhysicAudioClip[] clips;
        public SerialTest cluck;
        private AudioSource audioSource;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();

            if(clips == null)
            {
                HydraLogger.Log($"{gameObject.name} Physic Audio: No clips found.", DebugChannel.Error);
                return;
            }

            Array.Sort(clips);
        }

        private void OnCollisionEnter(Collision col)
        {
            if(audioSource == null)
            {
                HydraLogger.Log($"{gameObject.name} Physic Audio: No audio source.", DebugChannel.Error);
                return;
            }

            if (clips == null)
            {
                HydraLogger.Log($"{gameObject.name} Physic Audio: No clips found.", DebugChannel.Error);
                return;
            } else if (clips.Length <= 0)
            {
                HydraLogger.Log($"{gameObject.name} Physic Audio: No clips found.", DebugChannel.Error);
                return;
            }

            ResolveCollisionAudio(col);
        }

        private void ResolveCollisionAudio(Collision col)
        {
            Vector3 impactVelocity = col.relativeVelocity;
            float impactForce = impactVelocity.magnitude;

            HydraLogger.Log($"impact force: {impactForce}");

            int layer = col.GetContact(0).otherCollider.gameObject.layer;

            for(int i=0; i< clips.Length; i++)
            {
                if(CheckClip(impactForce, layer, clips[i]))
                {
                    float volume = Mathf.InverseLerp(clips[i].forceVolumeScale.x, clips[i].forceVolumeScale.y, impactForce) + 0.35f;
                    audioSource.clip = clips[i].clips[UnityEngine.Random.Range(0, clips[i].clips.Length)];
                    audioSource.pitch = UnityEngine.Random.Range(clips[i].pitchMinMax.x, clips[i].pitchMinMax.y);
                    audioSource.Play();
                    break;
                }
            }
        }

        private bool CheckClip(float force, int layer, PhysicAudioClip clip)
        {
            if (clip == null)
            {
                return false;
            }
            else if (clip.clips == null || clip.clips.Length <= 0)
            {
                return false;
            }

            if (!(clip.layer == (clip.layer | (1 << layer))))
            {
                return false;
            }

            if(force < clip.forceVolumeScale.x)
            {
                return false;
            }

            return true;
        }      
    }

    [System.Serializable]
    public class PhysicAudioClip : IComparable
    {
        public LayerMask layer;
        public Vector2 forceVolumeScale = new Vector2(0.25f, 2.0f);
        public Vector2 pitchMinMax = new Vector2(1,1);
        public AudioClip[] clips;

        public int CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }

            PhysicAudioClip physicAudioClip = obj as PhysicAudioClip;

            if (physicAudioClip != null)
            {
                return this.forceVolumeScale.x.CompareTo(physicAudioClip.forceVolumeScale.x);
            }
            else
            {
                throw new ArgumentException("Compared object is not a physic audio clip.");
            }
        }
    }

    [System.Serializable]
    public class SerialTest
    {
        public string cuck;
    }
}
