using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace UltraFunGuns
{
    public abstract class ConfigSlider<T> : ConfigValueElement<T>
    {
        public T Min { get; }
        public T Max { get; }
        
        public ConfigSlider(T defaultValue, T min, T max) : base(defaultValue)
        {
            this.Min = min;
            this.Max = max;
        }
    }
}
