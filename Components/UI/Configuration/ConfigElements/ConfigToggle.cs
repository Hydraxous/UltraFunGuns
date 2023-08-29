using Mono.CompilerServices.SymbolWriter;
using System;
using System.Collections.Generic;
using System.Text;
using UltraFunGuns.UI;
using UnityEngine;
using UnityEngine.UI;

namespace UltraFunGuns
{
    public class ConfigToggle : ConfigValueElement<bool>
    {
        public ConfigToggle(bool defaultValue) : base(defaultValue) 
        {
            OnValueChanged += (_) => RefreshElementValue();
        }

        protected Toggle instancedToggle;

        protected override void RefreshElementValueCore()
        {
            if (instancedToggle == null)
                return;

            instancedToggle.SetIsOnWithoutNotify(GetValue());
        }

        protected void SetToggle(Toggle toggle)
        {
            toggle.onValueChanged.AddListener((v) => SetValueFromToggle(toggle, v));
            instancedToggle = toggle;
            RefreshElementValue();
        }

        protected override void LoadValueCore()
        {
            base.LoadValueCore();
            RefreshElementValue();
        }

        protected void SetValueFromToggle(Toggle source, bool newValue)
        {
            if (source != instancedToggle)
                return;

            SetValue(newValue);
        }

        protected override void BuildElementCore(Configgable descriptor, RectTransform rect)
        {
            DynUI.ConfigUI.CreateElementSlot(rect, this, (r) =>
            {
                DynUI.Toggle(r, SetToggle);
            },
            (rBS) =>
            {

            });
        }
    }
}
