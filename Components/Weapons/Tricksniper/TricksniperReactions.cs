using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    public class TricksniperReactions : MonoBehaviour
    {
        [SerializeField] private AudioClip[] reactions;

        public void PlayReaction()
        {
            if (reactions == null || !Data.Config.Data.TricksniperReactionsEnabled)
                return;

            AudioClip clipToPlay = reactions[UnityEngine.Random.Range(0, reactions.Length)];
            AudioSource src = clipToPlay?.PlayAudioClip();
        }
    }
}
