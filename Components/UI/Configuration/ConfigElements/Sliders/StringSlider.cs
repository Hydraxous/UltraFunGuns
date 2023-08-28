using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    public class StringSlider : ConfigValueElement<string>
    {
        public string[] Values { get; private set; }
        private int defaultIndex;
        private int currentIndex;

        public StringSlider(int defaultIndex, string[] values) : base(values[defaultIndex])
        {
            this.Values = values;
            this.defaultIndex = defaultIndex;
        }

        protected override string GetValueCore()
        {
            return Values[currentIndex];
        }

        protected override void SetValueCore(string value)
        {

        }

        protected override void ResetValueCore()
        {
            currentIndex = defaultIndex;
            base.SetValue(Values[defaultIndex]);
        }

        public void SetValues(string[] values)
        {
            this.Values = values;
            //Clamping!!! TODO
        }


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
