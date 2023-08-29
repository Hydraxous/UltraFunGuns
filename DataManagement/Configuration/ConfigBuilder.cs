using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace UltraFunGuns.Configuration
{
    public class ConfigBuilder
    {
        public string GUID { get; }
        public string OwnerDisplayName { get; }
        public bool Initialized { get; private set; }

        private Assembly owner;

        private List<IConfigElement> _configElements;
        private IConfigElement[] configElements
        {
            get
            {
                if (_configElements == null)
                {
                    Build();
                }
                return _configElements.ToArray();
            }
        }

        public ConfigBuilder(string guid = null, string menuDisplayName = null) 
        {
            this.owner = Assembly.GetCallingAssembly();
            this.GUID = (string.IsNullOrEmpty(guid) ? owner.GetName().Name : guid);
            this.OwnerDisplayName = (string.IsNullOrEmpty(menuDisplayName) ? GUID : menuDisplayName);
        }

        public void Build()
        {
            if (Initialized)
                return;

            _configElements = new List<IConfigElement>();
            currentAssembly = owner;
            
            foreach (Type type in owner.GetTypes())
            {
                currentType = type;
                foreach (MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
                {
                    ProcessMethod(method);
                }

                foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
                {
                    ProcessField(field);
                }

                foreach (PropertyInfo property in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
                {
                    //ProcessProperty(property);
                }
            }

            Initialized = true;
            OnConfigElementsChanged += (v) => ConfigurationManager.SubMenuElementsChanged();
            OnConfigElementsChanged?.Invoke(_configElements.ToArray());
            ConfigurationManager.RegisterConfiguraitonMenu(this);
        }

        public Action<IConfigElement[]> OnConfigElementsChanged;

        public IConfigElement[] GetConfigElements()
        {
            return configElements;
        }


        private Assembly currentAssembly;
        private Type currentType;


        private void ProcessMethod(MethodInfo method)
        {
            if (!method.IsStatic) //no instance!!
                return;

            if (method.ReturnType != typeof(void))
                return;

            if (method.GetParameters().Length > 0)
                return;

            Configgable cfg = method.GetCustomAttribute<Configgable>();

            if (cfg == null)
                return;

            cfg.SetOwner(this);
            cfg.SetSerializationAddress($"{GUID}.{method.DeclaringType.Namespace}.{method.DeclaringType.Name}.{method.Name}"); //THis isnt needed, but who cares.

            if (string.IsNullOrEmpty(cfg.DisplayName))
                cfg.SetDisplayNameFromCamelCase(method.Name);

            RegisterMethodAsButton(cfg, method);
        }

        private void ProcessField(FieldInfo field)
        {
            if (!field.IsStatic) //no instance!!
                return;

            Configgable cfg = field.GetCustomAttribute<Configgable>();

            if (cfg == null)
                return;

            cfg.SetOwner(this);
            cfg.SetSerializationAddress($"{GUID}.{field.DeclaringType.Namespace}.{field.DeclaringType.Name}.{field.Name}");

            if (string.IsNullOrEmpty(cfg.DisplayName))
                cfg.SetDisplayNameFromCamelCase(field.Name);

            if (typeof(IConfigElement).IsAssignableFrom(field.FieldType))
            {
                IConfigElement cfgElement = (IConfigElement)field.GetValue(null);
                RegisterElementCore(cfg, cfgElement);
            }
            else
            {
                RegisterPrimitive(cfg, field);
            }
        }

        private void ProcessProperty(PropertyInfo property)
        {
            if (!property.CanWrite) //no instance!!
                return;

            if (!property.CanRead)
                return;

            Configgable cfg = property.GetCustomAttribute<Configgable>();

            if (cfg == null)
                return;

            cfg.SetOwner(this);
            cfg.SetSerializationAddress($"{GUID}.{property.DeclaringType.Namespace}.{property.DeclaringType.Name}.{property.Name}");

            if (string.IsNullOrEmpty(cfg.DisplayName))
                cfg.SetDisplayNameFromCamelCase(property.Name);

            if (typeof(IConfigElement).IsAssignableFrom(property.PropertyType))
            {
                IConfigElement cfgElement = (IConfigElement)property.GetValue(null);
                RegisterElementCore(cfg, cfgElement);
            }
            else
            {
                //RegisterPrimitive(cfg, field);
            }
        }

        private void RegisterMethodAsButton(Configgable descriptor, MethodInfo method)
        {
            ConfigButton button = new ConfigButton(() =>
            {
                method.Invoke(null, null);
            });

            button.BindDescriptor(descriptor);
            RegisterElementCore(descriptor, button);
        }

        private void RegisterElementCore(Configgable descriptor, IConfigElement configElement)
        {
            configElement.BindDescriptor(descriptor);
            _configElements.Add(configElement);
        }

        public void RegisterElement(Configgable descriptor, IConfigElement configElement)
        {
            if (!Initialized)
                Build();

            RegisterElementCore(descriptor, configElement);
        }


        private void RegisterPrimitive(Configgable descriptor, FieldInfo field)
        {
            if (field.FieldType == typeof(float))
            {
                RangeAttribute range = field.GetCustomAttribute<RangeAttribute>();

                ConfigValueElement<float> floatElement = null;

                if (range == null)
                {
                    float baseValue = (float)field.GetValue(null);
                    floatElement = new ConfigInputField<float>(baseValue);

                }
                else
                {
                    float baseValue = (float)field.GetValue(null);
                    float clampedValue = Mathf.Clamp(baseValue, range.min, range.max);
                    floatElement = new FloatSlider(clampedValue, range.min, range.max);
                }

                floatElement.OnValueChanged += (v) => field.SetValue(null, v); //this is cursed as hell lol, dont care
                floatElement.BindDescriptor(descriptor);
                RegisterElementCore(descriptor, floatElement);
            }

            if (field.FieldType == typeof(int))
            {
                RangeAttribute range = field.GetCustomAttribute<RangeAttribute>();

                ConfigValueElement<int> intElement = null;

                int baseValue = (int)field.GetValue(null);

                if (range == null)
                {
                    intElement = new ConfigInputField<int>(baseValue);
                }
                else
                {
                    int clampedValue = Mathf.Clamp(baseValue, (int)range.min, (int)range.max);
                    intElement = new IntegerSlider(clampedValue, (int)range.min, (int)range.max);
                }

                intElement.OnValueChanged += (v) => field.SetValue(null, v);
                intElement.BindDescriptor(descriptor);
                RegisterElementCore(descriptor, intElement);
            }

            if (field.FieldType == typeof(bool))
            {
                bool baseValue = (bool)field.GetValue(null);
                ConfigToggle toggleElement = new ConfigToggle(baseValue);
                toggleElement.OnValueChanged += (v) => field.SetValue(null, v);
                toggleElement.BindDescriptor(descriptor);
                RegisterElementCore(descriptor, toggleElement);
            }

            if (field.FieldType == typeof(string))
            {
                string baseValue = (string)field.GetValue(null);
                ConfigInputField<string> stringElement = new ConfigInputField<string>(baseValue);
                stringElement.OnValueChanged += (v) => field.SetValue(null, v);
                stringElement.BindDescriptor(descriptor);
                RegisterElementCore(descriptor, stringElement);
            }
        }

        public object GetValueAtAddress(string address)
        {
            if (!Data.Config.Data.Configgables.ContainsKey(address))
                return null;

            return Data.Config.Data.Configgables[address];
        }

        public void SetValueAtAddress(string address, object value)
        {
            Data.Config.Data.Configgables[address] = value;
        }

        public void Save()
        {
            Data.Config.Save();
        }
    }
}
