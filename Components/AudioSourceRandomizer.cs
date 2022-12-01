using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace UltraFunGuns
{
    public class AudioSourceRandomizer : MonoBehaviour
    {
        public float minPitch = 0.85f , maxPitch = 1.15f;
        public float minVolume = 1.0f, maxVolume = 1.0f;

        private AudioSource audioSrc;

        private void Awake()
        {
            audioSrc = GetComponent<AudioSource>();
            if(audioSrc == null)
            {
                Destroy(this);
            }else
            {
                RandomizeAudio();
            }
        }

        private void RandomizeAudio()
        {
            if(audioSrc)
            {
                float randPitch = UnityEngine.Random.Range(minPitch, maxPitch);
                float randVol = UnityEngine.Random.Range(minVolume, maxVolume);
                audioSrc.pitch = randPitch;
                audioSrc.volume = randVol;
            }
        }
    }
}
