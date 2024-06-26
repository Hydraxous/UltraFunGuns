﻿using System.Collections.Generic;
using System.Linq;

namespace UltraFunGuns
{
    public static class RandomExtensions
    {
        public static T RandomEntry<T>(this IEnumerable<T> elements)
        {
            return elements.ElementAt(UnityEngine.Random.Range(0, elements.Count()));
        }
    }
}
