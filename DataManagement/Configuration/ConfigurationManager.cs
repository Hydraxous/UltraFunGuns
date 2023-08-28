using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    public static class ConfigurationManager
    {

        private static IConfigElement[] _configElements;
        private static IConfigElement[] configElements
        {
            get
            {
                if(_configElements == null)
                {
                    RetrieveConfigElements();
                }
                return _configElements;
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

        private static void RetrieveConfigElements()
        {
            if (initialized)
                return;

            List<IConfigElement> elements = new List<IConfigElement>();
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in asm.GetTypes())
                {
                    foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
                    {
                        if (!field.IsStatic)
                            continue;

                        Configgable cfg = field.GetCustomAttribute<Configgable>();

                        if (cfg == null)
                            continue;

                        if (!typeof(IConfigElement).IsAssignableFrom(field.FieldType))
                            continue;

                        cfg.SetSerializationAddress($"{asm.GetName().Name}.{field.DeclaringType.Namespace}.{field.DeclaringType.Name}.{field.Name}");

                        if (string.IsNullOrEmpty(cfg.DisplayName))
                            cfg.SetDisplayNameFromCamelCase(field.Name);

                        IConfigElement cfgElement = (IConfigElement)field.GetValue(null);
                        cfgElement.BindDescriptor(cfg);
                        elements.Add(cfgElement);
                    }
                }
            }

            _configElements = elements.ToArray();
            initialized = true;

            OnConfigElementsChanged?.Invoke(_configElements);
        }

        public static void RegisterElement(IConfigElement configElement)
        {

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
