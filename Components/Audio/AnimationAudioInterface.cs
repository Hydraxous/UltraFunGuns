using System.Linq;
using UnityEngine;

namespace UltraFunGuns
{
    public class AnimationAudioInterface : MonoBehaviour
    {
        [SerializeField] private AudioClip[] clips;

        public void PlayClip(int clipIndex)
        {
            if (clipIndex >= clips.Length || clipIndex < 0)
            {
                UltraFunGuns.Log.LogError($"Audio clip index ({clipIndex}) out of range on {gameObject.name}");
                return;
            }

            PlayClip(clips[clipIndex]);
        }

        public void PlayClip(string name)
        {
            if (clips == null)
                return;

            AudioClip clip = clips.Where(x=> x.name == name).First();
            PlayClip(clip);
        }

        private void PlayClip(AudioClip clip)
        {
            if (clip == null)
            {
                UltraFunGuns.Log.LogError($"Warning audio clip attempted to be played on {gameObject.name} through animation. The provided clip was null.");
                return;
            }

            Transform audio = clip.PlayAudioClip(UnityEngine.Random.Range(0.9f,1.1f), 1.0f, 0.0f).transform;
            audio.parent = transform;
        }
    }
}
