using Mono.CompilerServices.SymbolWriter;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using UltraFunGuns.UI;
using UnityEngine;
using UnityEngine.UI;

namespace UltraFunGuns
{
    public class ConfigField<T> : ConfigValueElement<T>, IConfigElement
    {
        private Func<T, bool> inputValidator;

        public ConfigField(T defaultValue, Func<T, bool> inputValidator = null)  : base (defaultValue)
        {
            this.inputValidator = inputValidator ?? ((v) => { return true; });
            OnValueChanged += (_) => RefreshElementValue();
            RefreshElementValue();
        }

        protected InputField instancedField;

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

        private void SetValueFromString(InputField source, string input)
        {
            if (source != instancedField) //prevent old non-null instance from calling this method.
                return;

            if (!ValidateInputSyntax(input, out T converted))
            {
                Debug.LogError("Syntax for field invalid! Conversion failed!");
                RefreshElementValue();
                return;
            }

            if(!ValidateValue(converted))
            {
                Debug.LogError("Value validation failure. Rejected.");
                RefreshElementValue();
                return;
            }

            base.SetValue(converted);
        }

        protected void SetInputField(InputField inputField)
        {
            inputField.onEndEdit.AddListener((s) => SetValueFromString(inputField, s));
            instancedField = inputField;
            RefreshElementValue();
        }

        protected override void RefreshElementValueCore()
        {
            if (instancedField == null)
                return;

            instancedField.SetTextWithoutNotify(GetValue().ToString());
        }

        protected override void BuildElementCore(Configgable configgable, RectTransform rect)
        {
            DynUI.ConfigUI.CreateElementSlot(rect, this, (r) =>
            {
                DynUI.InputField(r, SetInputField);
            },
            (rBS) =>
            {
                DynUI.ImageButton(rBS, (button, icon) =>
                {
                    button.colors.SetFirstColor(Color.red);
                    RectTransform rt = button.GetComponent<RectTransform>();
                    rt.sizeDelta = new Vector2(55f, 55f);
                    button.onClick.AddListener(() => { Debug.Log(GetValue()); });
                });

                DynUI.ImageButton(rBS, (button, icon) =>
                {
                    button.colors.SetFirstColor(Color.red);
                    RectTransform rt = button.GetComponent<RectTransform>();
                    rt.sizeDelta = new Vector2(55f, 55f);
                    button.onClick.AddListener(() => { Debug.Log(DefaultValue); });
                });
            });
        }
    }
}
