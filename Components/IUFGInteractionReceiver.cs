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
    }
}
