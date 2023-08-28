using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    public abstract class ConfigValueElement<T> : IConfigElement
    {
        public T DefaultValue { get; }

        private T? value;

        public Action<T> OnValueChanged;

        protected Configgable descriptor;

        private bool initialized => descriptor != null;

        public T Value
        {
            get
            {
                return GetValue();
            }
        }

        public ConfigValueElement(T defaultValue)
        {
            DefaultValue = defaultValue;
        }


        public void LoadValue()
        {
            if (!initialized)
                ConfigurationManager.Initialize();

            LoadValueCore();
        }

        protected virtual void LoadValueCore()
        {
            //Get value from data manager.
            object obj = ConfigurationManager.GetValueAtAddress(descriptor.SerializationAddress);

            if (obj != null)
            {
                try
                {
                    SetValue((T)obj);
                    return;
                } catch(Exception ex)
                {
                    Debug.LogException(ex);
                }
            }

            SetValue(DefaultValue);
            SaveValue();
        }


        public void SaveValue()
        {
            SaveValueCore();
        }

        protected virtual void SaveValueCore()
        {
            object obj = GetValue();
            ConfigurationManager.SetValueAtAddress(descriptor.SerializationAddress, obj);
            ConfigurationManager.Save();
        }


        public T GetValue()
        {
            return GetValueCore();
        }

        protected virtual T GetValueCore()
        {
            if (value == null)
            {
                LoadValue();
            }
            
            return value;
        }



        public void SetValue(T value)
        {
            SetValueCore(value);
        }

        protected virtual void SetValueCore(T value)
        {
            this.value = value;
            OnValueChanged?.Invoke(value);
        }


        public void ResetValue()
        {
            ResetValueCore();
        }

        protected virtual void ResetValueCore()
        {
            SetValue(DefaultValue);
        }


        public void BindDescriptor(Configgable configgable)
        {
            this.descriptor = configgable;
        }

        public Configgable GetDescriptor()
        {
            return descriptor;
        }

        public void BuildElement(RectTransform rect)
        {
            if (!initialized)
                ConfigurationManager.Initialize();

            BuildElementCore(descriptor, rect);
        }

        protected abstract void BuildElementCore(Configgable descriptor, RectTransform rect);
    }
}
