using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace UltraFunGuns
{
    public class FloatSlider : ConfigSlider<float>
    {
        public FloatSlider(float defaultValue, float min, float max) : base(defaultValue, min, max) {}

        protected override void BuildElementCore(Configgable configgable, RectTransform rect)
        {
            base.BuildElementCore(configgable, rect);
            instancedSlider.wholeNumbers = false;
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
            SetValue(value);
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
