using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.Text;

namespace UltraFunGuns
{
    public class Configurable<T>
    {
        public T DefaultValue { get; }
        protected T value;
        public T Value
        {
            get
            {
                if(value == null)
                {
                    value = default;
                }
                return value;
            }

        }

        public Action<T> OnValueChanged;

        public Configurable(T defaultValue)
        {
            this.DefaultValue = defaultValue;
        }

        public void Reset()
        {
            this.value = DefaultValue;
        }

        public void SetValue(T value)
        {
            SetValueCore(value);
        }

        protected virtual void SetValueCore(T value)
        {
            this.value = value;
            OnValueChanged?.Invoke(this.value);
        }
    }
}
