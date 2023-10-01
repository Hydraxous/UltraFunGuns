using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraFunGuns.InterfaceTypes
{
    public interface IParriable
    {
        public bool Parry(Vector3 position, Vector3 direction);
    }
}
