using System;
using System.Collections.Generic;
using System.Text;

namespace UltraFunGuns
{
    public interface IUFGInteractionReceiver
    {
        void Shot(BeamType beamType);
        void Parried();

    }
}
