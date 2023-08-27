using System;
using System.Collections.Generic;
using System.Text;

namespace UltraFunGuns
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class TweakSlider : Attribute
    {
        public Type NumericType { get; }

        public float FloatMin { get; }
        public float FloatMax { get; }

        public int IntMin { get; }
        public int IntMax { get; }

        public int IntUnit { get; }
        public float FloatUnit { get; }

        public TweakSlider(float min, float max, float unitSize = 1f)
        {
            NumericType = typeof(float);
            FloatMin = min;
            FloatMax = max;
            FloatUnit = unitSize;
        }

        public TweakSlider(int min, int max, int unitSize = 1)
        {
            NumericType = typeof(int);
            IntMin = min;
            IntMax = max;
            IntUnit = unitSize;
        }
    }
}
