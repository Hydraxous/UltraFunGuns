using UnityEngine;

namespace UltraFunGuns
{
    public static class AudioTools
    {
        public static AudioSource PlayAudioClip(this AudioClip clip, float pitch = 1.0f, float volume = 1.0f, float spatialBlend = 0.0f)
        {
            return PlayAudioClip(clip, Vector3.zero, pitch, volume, spatialBlend);
        }

        public static AudioSource PlayAudioClip(this AudioClip clip, Vector3 position, float pitch = 1.0f, float volume = 1.0f, float spatialBlend = 0.0f)
        {
            if (clip == null)
            {
                return null;
            }

            GameObject newAudioObject = new GameObject($"AudioSource({clip.name})");
            newAudioObject.transform.position = position;
            AudioSource newAudioSource = newAudioObject.AddComponent<AudioSource>();
            DestroyAfterTime destroyOverTime = newAudioObject.AddComponent<DestroyAfterTime>();
            newAudioSource.playOnAwake = false;
            newAudioSource.spatialBlend = spatialBlend;
            newAudioSource.volume = volume;
            newAudioSource.pitch = pitch;
            newAudioSource.clip = clip;
            newAudioSource.Play();
            return newAudioSource;
        }
    }
}
