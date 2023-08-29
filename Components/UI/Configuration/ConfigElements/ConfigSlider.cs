using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UltraFunGuns.UI;
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

        protected Slider instancedSlider;
        protected Text outputText;

        protected virtual void SetValueFromSlider(Slider origin, float sliderValue)
        {
            if (origin != instancedSlider)
                return;

            SetValueFromSlider(sliderValue);
        }

        protected abstract void ConfigureSliderRange(Slider slider);
        protected abstract void SetValueFromSlider(float value);

        protected void SetSlider(Slider slider)
        {
            slider.onValueChanged.AddListener((v) => SetValueFromSlider(slider, v));
            ConfigureSliderRange(slider);
            this.instancedSlider = slider;
            RefreshElementValue();
        }

        protected override void RefreshElementValueCore()
        {
            if (instancedSlider == null)
                return;

            if (outputText != null)
                outputText.text = ToString();
        }

        protected override void BuildElementCore(Configgable descriptor, RectTransform rect)
        {
            DynUI.ConfigUI.CreateElementSlot(rect, this, (r) =>
            {
                DynUI.Label(r, (t) =>
                {
                    outputText = t;
                });
                DynUI.Slider(r, SetSlider);
            },
            (rBS) =>
            {

            });
        }
    }
}
