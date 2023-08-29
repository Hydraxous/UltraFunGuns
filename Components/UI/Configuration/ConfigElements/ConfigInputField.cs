using Mono.Cecil;
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
    public class ConfigInputField<T> : ConfigValueElement<T>
    {
        private Func<T, bool> inputValidator;
        private Func<string, ValueTuple<bool, T>> valueConverter;

        public ConfigInputField(T defaultValue, Func<T, bool> inputValidator = null, Func<string, ValueTuple<bool, T>> typeConverter = null)  : base (defaultValue)
        {
            this.valueConverter = typeConverter ?? ValidateInputSyntax;
            this.inputValidator = inputValidator ?? ((v) => { return true; });

            OnValueChanged += (_) => RefreshElementValue();
            RefreshElementValue();
        }

        protected InputField instancedField;

        private ValueTuple<bool, T> ValidateInputSyntax(string inputValue)
        {
            ValueTuple<bool, T> result = new ValueTuple<bool, T>();
            
            result.Item1 = false;
            result.Item2 = default(T);

            try
            {
                TypeConverter typeConverter = TypeDescriptor.GetConverter(typeof(T));
                object convertedValue = typeConverter.ConvertFromString(inputValue);
                result.Item2 = (T) convertedValue;
                result.Item1 = result.Item2 != null;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            return result;
        }

        private void SetValueFromString(InputField source, string input)
        {
            if (source != instancedField) //prevent old non-null instance from calling this method.
                return;

            ValueTuple<bool, T> conversionResult;
            
            try
            {
                conversionResult = valueConverter.Invoke(input);
                if (!conversionResult.Item1)
                {
                    Debug.LogError("Syntax for field invalid! Conversion failed!");
                    RefreshElementValue();
                    return;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
                RefreshElementValue();
                return;
            }

            if(!inputValidator.Invoke(conversionResult.Item2))
            {
                Debug.LogError("Value validation failure. Rejected.");
                RefreshElementValue();
                return;
            }

            base.SetValue(conversionResult.Item2);
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
