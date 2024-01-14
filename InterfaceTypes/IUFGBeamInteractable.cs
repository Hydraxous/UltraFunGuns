﻿using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    public interface IUFGBeamInteractable
    {
        public void OnRevolverBeamHit(RevolverBeam beam, ref RaycastHit hit);
        public bool CanRevolverBeamHit(RevolverBeam beam, ref RaycastHit hit);
        public bool CanRevolverBeamPierce(RevolverBeam beam);
    }
}
