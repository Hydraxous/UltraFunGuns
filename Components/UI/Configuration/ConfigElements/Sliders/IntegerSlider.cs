using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    public class IntegerSlider : ConfigSlider<int>
    {
        public IntegerSlider(int defaultValue, int min, int max) : base(defaultValue, min, max) {}

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
