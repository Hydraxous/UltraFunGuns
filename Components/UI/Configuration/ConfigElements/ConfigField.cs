using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    public class ConfigField<T> : ConfigValueElement<T>, IConfigElement
    {
        private Func<T, bool> inputValidator;

        public ConfigField(T defaultValue, Func<T, bool> inputValidator = null)  : base (defaultValue)
        {
            this.inputValidator = inputValidator ?? ((v) => { return true; });
        }

        private bool ValidateInputSyntax(string inputValue, out T converted)
        {
            converted = default(T);

            try
            {
                TypeConverter typeConverter = TypeDescriptor.GetConverter(typeof(T));
                object convertedValue = typeConverter.ConvertFromString(inputValue);
                converted = (T) convertedValue;
                return converted != null;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            return false;
        }

        public bool ValidateValue(T value)
        {
            return inputValidator(value);
        }

        private void SetValueFromString(string input)
        {
            if (!ValidateInputSyntax(input, out T converted))
            {
                Debug.LogError("Syntax for field invalid! Conversion failed!");
                return;
            }

            if(!ValidateValue(converted))
            {
                Debug.LogError("Value validation failure. Rejected.");
                return;
            }

            base.SetValue(converted);
        }

        protected override void BuildElementCore(Configgable configgable, RectTransform rect)
        {

        }
    }
}
