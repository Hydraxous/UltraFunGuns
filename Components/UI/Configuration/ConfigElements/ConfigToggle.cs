using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    public class ConfigToggle : ConfigValueElement<bool>
    {
        public ConfigToggle(bool defaultValue) : base(defaultValue) {}

        protected override void BuildElementCore(Configgable descriptor, RectTransform rect)
        {

        }
    }
}
