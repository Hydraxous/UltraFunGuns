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
    }
}
