using BepInEx;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    public interface IUFGInteractionReceiver
    {
        void Shot(BeamType beamType);
        bool Parried(Vector3 aimVector);
        bool Interact(UFGInteractionEventData interaction);
        Vector3 GetPosition();
    }

    public struct UFGInteractionEventData
    {
        public Vector3 interactorPosition;
        public Vector3 direction;

        public float power;
        public string data;
        public Type invokeType;
        public string[] tags;

        public bool ContainsTag(string tag)
        {
            if (tags == null)
                return false;

            for(int i=0; i<tags.Length; i++)
            {
                if (tags[i].IsNullOrWhiteSpace())
                    continue;

                if (tags[i].Contains(tag))
                    return true;
            }

            return false;
        }

        public bool ContainsAnyTag(params string[] checkTags)
        {
            for (int i=0; i< checkTags.Length; i++)
            {
                if (ContainsTag(checkTags[i]))
                    return true;
            }

            return false;
        }

    }
}
