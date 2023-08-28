using System;
using System.Collections.Generic;
using System.Net.Security;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    public class ConfigDropdown<T> : ConfigValueElement<T>, IConfigElement
    {
        public string[] Names { get; private set; }
        public T[] Values { get; private set; }

        private int defaultIndex;
        private int currentIndex;

        public ConfigDropdown(T[] values, string[] names, int defaultIndex = 0) : base(values[defaultIndex])
        {
            this.Names = names;
            this.Values = values;
            this.defaultIndex = defaultIndex;
        }

        public void SetValuesAndNames(T[] values, string[] names)
        {
            this.Values = values;
            this.Names = names;

            //TODO CLAMPING!
        }

        protected override void LoadValueCore()
        {
            //set selectedIndex to serialized value
        }

        protected override void SaveValueCore()
        {
            //save selectedIndex as serialized value
        }

        protected override void ResetValueCore()
        {
            currentIndex = defaultIndex;
            base.SetValue(Values[currentIndex]);
        }

        protected override T GetValueCore()
        {
            return Values[currentIndex];
        }

        protected override void BuildElementCore(Configgable configgable, RectTransform rect)
        {

        }
    }
}
