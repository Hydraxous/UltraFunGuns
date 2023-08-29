using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Text;
using UltraFunGuns.UI;
using UnityEngine;
using UnityEngine.UI;
using UltraFunGuns.Configuration;

namespace UltraFunGuns
{
    public class ConfigDropdown<T> : ConfigValueElement<T>
    {
        public string[] Names { get; private set; }
        public T[] Values { get; private set; }

        private int defaultIndex;
        private int? currentIndex;

        public ConfigDropdown(T[] values, string[] names = null, int defaultIndex = 0) : base(values[defaultIndex])
        {
            this.Values = values;
            this.Names = CreateNames(values, names);
            this.defaultIndex = defaultIndex;
            OnValueChanged += (_) => RefreshElementValue();
            RefreshElementValue();
        }

        private string[] CreateNames(T[] values, string[] providedNames)
        {
            string[] names = new string[values.Length];

            if (providedNames == null)
                providedNames = Array.Empty<string>();

            for (int i = 0; i < values.Length; i++)
            {
                names[i] = (i < providedNames.Length) ? providedNames[i] : values[i].ToString();
            }

            return names;
        }

        public void SetOptions(T[] values, string[] names = null, int newIndex = 0, int defaultIndex = 0)
        {
            int valueLen = values.Length;

            if (defaultIndex >= valueLen || defaultIndex < 0)
                throw new IndexOutOfRangeException("Default index is out of bounds with provided values.");

            if (newIndex >= valueLen || newIndex < 0)
                throw new IndexOutOfRangeException("Default index is out of bounds with provided values.");

            this.Values = values;
            this.Names = CreateNames(values, names);
            this.defaultIndex = defaultIndex;
            SetIndex(newIndex);
        }

        protected Dropdown instancedDropdown;

        protected override void LoadValueCore()
        {
            //Get value from data manager.
            object obj = ConfigurationManager.GetObjectAtAddress(descriptor.SerializationAddress);

            firstLoadDone = true;

            if (obj != null)
            {
                try
                {
                    SetIndexCore((int) obj);
                    return;
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }

            ResetValue();
            SaveValue();
        }

        protected override void SaveValueCore()
        {
            object obj = currentIndex; //We dont serialize the value, we serialize the index.
            ConfigurationManager.SetObjectAtAddress(descriptor.SerializationAddress, obj);
            ConfigurationManager.Save();
            IsDirty = false;
        }

        protected override void ResetValueCore()
        {
            SetIndexCore(defaultIndex);
        }

        protected override T GetValueCore()
        {
            return Values[GetCurrentIndex()];
        }

        public string GetSelectedIndexName()
        {
            return Names[GetCurrentIndex()];
        }

        public int GetCurrentIndex()
        {
            return GetCurrentIndexCore();
        }

        protected int GetCurrentIndexCore()
        {
            if(currentIndex == null)
            {
                LoadValue();
            }

            return currentIndex.Value;
        }

        protected void SetDropdown(Dropdown dropdown)
        {
            dropdown.onValueChanged.AddListener((v) => SetValueFromDropdown(dropdown, v));
            instancedDropdown = dropdown;
            RefreshElementValue();
        }

        protected void SetValueFromDropdown(Dropdown source, int newValue)
        {
            if (source != instancedDropdown)
                return;

            SetIndex(newValue);
        }

        protected void RefreshValue()
        {
            SetValue(Values[GetCurrentIndex()]);
        }

        protected override void SetValueCore(T value)
        {
            if(!Values.Contains(value))
            {
                throw new KeyNotFoundException("Unable to set Dropdown's value directly. It is not contained within the values. Use SetValuesAndNames and SetIndex.");
            }

            base.SetValueCore(value);
        }

        public void SetIndex(int index)
        {
            if (index >= Values.Length || index < 0)
                throw new IndexOutOfRangeException("Index is out of range of Dropdown's values.");

            SetIndexCore(index);
        }

        protected void SetIndexCore(int index)
        {
            currentIndex = index;
            RefreshValue();
        }


        protected override void BuildElementCore(Configgable configgable, RectTransform rect)
        {
            DynUI.ConfigUI.CreateElementSlot(rect, this, (r) =>
            {
                DynUI.Dropdown(r, SetDropdown);
            },
            (rBS) =>
            {
                
            });
        }

        protected override void RefreshElementValueCore()
        {
            if (instancedDropdown == null)
                return;

            instancedDropdown.ClearOptions();
            instancedDropdown.options = Names.Select(x => new Dropdown.OptionData(x)).ToList();
            instancedDropdown.SetValueWithoutNotify(GetCurrentIndex());
            instancedDropdown.RefreshShownValue();
        }
    }
}
