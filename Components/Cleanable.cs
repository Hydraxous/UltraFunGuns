using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraFunGuns.Components
{
    public class Cleanable : MonoBehaviour, ICleanable
    {
        public void Cleanup()
        {
            Destroy(gameObject);
        }
    }
}
