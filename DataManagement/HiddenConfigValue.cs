using Configgy;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraFunGuns.DataManagement
{
    public class HiddenConfigValue<T> : ConfigValueElement<T>
    {
        public HiddenConfigValue(T defaultValue) : base(defaultValue) { }
        protected override void BuildElementCore(ConfiggableAttribute _, RectTransform __) { }
        protected override void RefreshElementValueCore() { }
    }
}
