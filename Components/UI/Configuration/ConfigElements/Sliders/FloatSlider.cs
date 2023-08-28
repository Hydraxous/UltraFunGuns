using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    public class FloatSlider : ConfigSlider<float>
    {
        public FloatSlider(float defaultValue, float min, float max) : base(defaultValue, min, max) {}

        protected override void BuildElementCore(Configgable configgable, RectTransform rect)
        {

        }

        protected override void LoadValueCore()
        {

        }

        protected override void SaveValueCore()
        {

        }
    }
}
