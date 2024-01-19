using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    public interface IUFGShootable
    {
        public bool CanBeShot(GameObject sourceWeapon);
        public void OnShot(GameObject sourceWeapon);
    }
}
