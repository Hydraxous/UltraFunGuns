using System;
using System.Collections.Generic;
using System.Text;

namespace UltraFunGuns
{
    public interface IUFGBeamInteractable
    {
        public bool OnBeamHit(RevolverBeam beam);
    }
}
