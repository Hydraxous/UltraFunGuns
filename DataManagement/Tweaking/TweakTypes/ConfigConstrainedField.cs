using System;
using System.Collections.Generic;
using System.Text;

namespace UltraFunGuns
{
    public class ConfigConstrainedField<T> : Configurable<T>
    {
        private Func<T, bool> validator;

        public ConfigConstrainedField(T defaultValue, Func<T,bool> constraints) : base (defaultValue)
        {
            validator = constraints;
        }

        protected override void SetValueCore(T value)
        {
            if (!validator.Invoke(value))
                return;

            this.value = value;
            OnValueChanged?.Invoke(this.value);
        }
    }
}
