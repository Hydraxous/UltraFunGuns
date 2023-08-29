using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UltraFunGuns.Configuration;


namespace UltraFunGuns
{
    public abstract class ConfigValueElement<T> : IConfigElement
    {
        public T DefaultValue { get; }

        private T? value;

        public Action<T> OnValueChanged;

        protected Configgable descriptor;

        private bool initialized => descriptor != null;
        protected bool firstLoadDone = false;

        public bool IsDirty { get; protected set; }

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
                return;

            LoadValueCore();
            firstLoadDone = true; //just to be safe.
        }

        protected virtual void LoadValueCore()
        {
            //Get value from data manager.
            //This should probably be changed to something more reliable and not static.
            object obj = ConfigurationManager.GetObjectAtAddress(descriptor.SerializationAddress);

            firstLoadDone = true; //nullable values apparently can just randomly have values so this annoying bool is needed

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

            ResetValue();
            SaveValue();
        }


        public void SaveValue()
        {
            SaveValueCore();
        }

        protected virtual void SaveValueCore()
        {
            object obj = GetValue();
            ConfigurationManager.SetObjectAtAddress(descriptor.SerializationAddress, obj);
            ConfigurationManager.Save();
            IsDirty = false;
        }


        public T GetValue()
        {
            return GetValueCore();
        }

        protected virtual T GetValueCore()
        {
            if (value == null || !firstLoadDone)
            {
                LoadValue();
            }
            
            return value;
        }


        public void SetValue(T value)
        {
            SetValueCore(value);
            IsDirty = true;
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
                return;

            BuildElementCore(descriptor, rect);
        }

        protected abstract void BuildElementCore(Configgable descriptor, RectTransform rect);

        public override string ToString()
        {
            return GetValue().ToString();
        }

        public void RefreshElementValue()
        {
            RefreshElementValueCore();
        }

        protected abstract void RefreshElementValueCore();

        public void OnMenuOpen()
        {
            RefreshElementValue();
        }

        public void OnMenuClose()
        {
            if(IsDirty)
                SaveValue();
        }
    }
}
