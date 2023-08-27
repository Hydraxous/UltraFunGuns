using System;
using System.Collections.Generic;
using System.Text;

namespace UltraFunGuns
{
    public class ConfigSlider<T> : Configurable<T>
    {
        public T Min { get; }
        public T Max { get; }

        public T UnitSize { get; }

        public ConfigSlider(T defaultValue, T min, T max, T unitSize) : base(defaultValue)
        {
            Min = min;
            Max = max;
            UnitSize = unitSize;
        }
    }
}
