using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace UltraFunGuns
{
    public class IntegerSlider : ConfigSlider<int>
    {
        public IntegerSlider(int defaultValue, int min, int max) : base(defaultValue, min, max) {}

        protected override void BuildElementCore(Configgable configgable, RectTransform rect)
        {
            base.BuildElementCore(configgable, rect);
            instancedSlider.wholeNumbers = true;
            OnValueChanged += (v) => UpdateSliderValue();
        }

        protected override void ConfigureSliderRange(Slider slider)
        {
            slider.minValue = Min;
            slider.maxValue = Max;
        }

        protected override void LoadValueCore()
        {
            base.LoadValueCore();
            UpdateSliderValue();
        }

        protected override void SetValueFromSlider(float value)
        {
            SetValue((int) value);
        }

        protected override void UpdateSliderValue()
        {
            base.UpdateSliderValue();
            if (instancedSlider == null)
                return;

            instancedSlider.value = GetValue();
        }
    }
}
