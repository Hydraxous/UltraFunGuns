using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using UnityEngine;

namespace UltraFunGuns
{
    public static class ConfigurationManager
    {

        private static List<IConfigElement> _configElements;
        private static IConfigElement[] configElements
        {
            get
            {
                if(_configElements == null)
                {
                    RetrieveConfigElements();
                }
                return _configElements.ToArray();
            }
        }

        public static Action<IConfigElement[]> OnConfigElementsChanged;

        public static IConfigElement[] GetConfigElements()
        {
            return configElements;
        }

        private static bool initialized;

        public static void Initialize()
        {
            RetrieveConfigElements();
        }

        private static Assembly currentAssembly;
        private static Type currentType;

        private static void RetrieveConfigElements()
        {
            if (initialized)
                return;

            _configElements = new List<IConfigElement>();

            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                currentAssembly = asm;
                foreach (Type type in asm.GetTypes())
                {
                    currentType = type;
                    foreach(MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
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
            }

            initialized = true;

            OnConfigElementsChanged?.Invoke(_configElements.ToArray());
        }


        private static void ProcessMethod(MethodInfo method)
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

            cfg.SetSerializationAddress($"{currentAssembly.GetName().Name}.{method.DeclaringType.Namespace}.{method.DeclaringType.Name}.{method.Name}"); //THis isnt needed, but who cares.

            if (string.IsNullOrEmpty(cfg.DisplayName))
                cfg.SetDisplayNameFromCamelCase(method.Name);

            RegisterMethodAsButton(cfg, method);
        }

        private static void ProcessField(FieldInfo field)
        {
            if (!field.IsStatic) //no instance!!
                return;

            Configgable cfg = field.GetCustomAttribute<Configgable>();

            if (cfg == null)
                return;

            cfg.SetSerializationAddress($"{currentAssembly.GetName().Name}.{field.DeclaringType.Namespace}.{field.DeclaringType.Name}.{field.Name}");

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

        private static void ProcessProperty(PropertyInfo property)
        {
            if (!property.CanWrite) //no instance!!
                return;

            if (!property.CanRead)
                return;

            Configgable cfg = property.GetCustomAttribute<Configgable>();

            if (cfg == null)
                return;

            cfg.SetSerializationAddress($"{currentAssembly.GetName().Name}.{property.DeclaringType.Namespace}.{property.DeclaringType.Name}.{property.Name}");

            if (string.IsNullOrEmpty(cfg.DisplayName))
                cfg.SetDisplayNameFromCamelCase(property.Name);

            if (typeof(IConfigElement).IsAssignableFrom(property.PropertyType))
            {
                IConfigElement cfgElement = (IConfigElement) property.GetValue(null);
                RegisterElementCore(cfg, cfgElement);
            }
            else
            {
                //RegisterPrimitive(cfg, field);
            }
        }

        private static void RegisterMethodAsButton(Configgable descriptor, MethodInfo method)
        {
            ConfigButton button = new ConfigButton(()=>
            {
                method.Invoke(null, null);
            });

            button.BindDescriptor(descriptor);
            RegisterElementCore(descriptor, button);
        }

        public static void RegisterElementCore(Configgable descriptor, IConfigElement configElement)
        {
            configElement.BindDescriptor(descriptor);
            _configElements.Add(configElement);
        }

        public static void RegisterElement(Configgable descriptor, IConfigElement configElement)
        {
            if (!initialized)
                Initialize();

            RegisterElementCore(descriptor, configElement);
        }


        private static void RegisterPrimitive(Configgable descriptor, FieldInfo field)
        {

            if(field.FieldType == typeof(float))
            {
                RangeAttribute range = field.GetCustomAttribute<RangeAttribute>();

                ConfigValueElement<float> floatElement = null;

                if(range == null)
                {
                    float baseValue = (float)field.GetValue(null);
                    floatElement = new ConfigField<float>(baseValue);
                    
                }else
                {
                    float baseValue = (float)field.GetValue(null);
                    float clampedValue = Mathf.Clamp(baseValue, range.min, range.max);
                    floatElement = new FloatSlider(clampedValue, range.min, range.max);
                }

                floatElement.OnValueChanged += (v) => field.SetValue(null, v); //this is cursed as hell lol, dont care
                floatElement.BindDescriptor(descriptor);
                RegisterElementCore(descriptor, floatElement);
            }

            if(field.FieldType == typeof(int))
            {
                RangeAttribute range = field.GetCustomAttribute<RangeAttribute>();

                ConfigValueElement<int> intElement = null;

                if (range == null)
                {
                    int baseValue = (int)field.GetValue(null);
                    intElement = new ConfigField<int>(baseValue);

                }
                else
                {
                    int baseValue = (int)field.GetValue(null);
                    int clampedValue = Mathf.Clamp(baseValue, (int)range.min, (int)range.max);
                    intElement = new IntegerSlider(clampedValue, (int)range.min, (int)range.max);
                }

                intElement.OnValueChanged += (v) => field.SetValue(null, v);
                intElement.BindDescriptor(descriptor);
                RegisterElementCore(descriptor, intElement);
            }

            if(field.FieldType == typeof(bool))
            {
                //Todo make toggle
            }

            if (field.FieldType == typeof(string))
            {
                string baseValue = (string) field.GetValue(null);
                ConfigField<string> stringElement = new ConfigField<string>(baseValue);
                stringElement.OnValueChanged += (v) => field.SetValue(null, v);
                stringElement.BindDescriptor(descriptor);
                RegisterElementCore(descriptor, stringElement);
            }
        }

        public static object GetValueAtAddress(string address)
        {
            if (!Data.Config.Data.Configgables.ContainsKey(address))
                return null;

            return Data.Config.Data.Configgables[address];
        }

        public static void SetValueAtAddress(string address, object value)
        {
            Data.Config.Data.Configgables[address] = value;
        }

        public static void Save()
        {
            Data.Config.Save();
        }
    }
}
