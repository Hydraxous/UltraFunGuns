using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace UltraFunGuns.Util
{
    public static class Cleaner
    {
        public static void Clean()
        {
            ICleanable[] cleanables = GameObject.FindObjectsOfType<MonoBehaviour>().OfType<ICleanable>().ToArray();

            foreach (var cleanable in cleanables)
            {
                cleanable?.Cleanup();
            }
        }
    }
}
