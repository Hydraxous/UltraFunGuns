using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    public class ConfigCustomElement : IConfigElement
    {
        private Action<Configgable, RectTransform> onBuild;

        private Configgable configgable;

        public ConfigCustomElement(Action<Configgable, RectTransform> onBuild)
        {
            this.onBuild = onBuild;
        }
        
        public void BindDescriptor(Configgable configgable)
        {
            this.configgable = configgable;
        }

        public void BuildElement(RectTransform rect)
        {
            onBuild?.Invoke(configgable, rect);
        }

        public Configgable GetDescriptor()
        {
            return configgable;
        }
    }
}
