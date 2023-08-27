using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;

namespace UltraFunGuns
{
    public static class ConfigurableManager
    {

        public static void LoadConfigurables()
        {

            Assembly asm = Assembly.GetExecutingAssembly();

            foreach(Type type in asm.GetTypes())
            {
                foreach(FieldInfo fieldInfo in type.GetFields()) //ADD BINDING FLAGS
                {
                    ProcessField(fieldInfo);
                }

                foreach(PropertyInfo propertyInfo in type.GetProperties()) //ADD BINDING FLAGS
                {

                }
            }









        }

        private static void ProcessField(FieldInfo field)
        {
            ConfigMenuOption menuDef = field.GetCustomAttribute<ConfigMenuOption>();

            if (menuDef == null)
                return;

            Type fieldType = field.FieldType;

            if (!typeof(Configurable<>).IsAssignableFrom(fieldType)) //Not configurable
                return;

            if (!fieldType.ContainsGenericParameters) //No generic args smh
                return;


            ProcessConfigOption<ConfigSlider<bool>, bool>(menuDef, field);
        }

        public static void SetValue(string key, object obj)
        {
            throw new NotImplementedException();
        }


        public static T GetValue<T>(string key)
        {
            throw new NotImplementedException();
            return default(T);
        }

        static Dictionary<string, MethodInfo> setters = new Dictionary<string, MethodInfo>();
        static Dictionary<string, MethodInfo> getters = new Dictionary<string, MethodInfo>();

        private static void ProcessConfigOption<T1, T2>(ConfigMenuOption configTag, FieldInfo brug) where T1 : Configurable<T2>
        {
            MethodInfo setter = typeof(T1).GetMethod("SetValue");
            PropertyInfo bruhg = null;

            MethodInfo gettr = bruhg.GetAccessors()[0];
            //getters.Add(gettr.DeclaringType.,);
            setter.Invoke(brug.GetValue(null), new object[] { });
        }

        private static T GetValue<T>(Configurable<T> funct)
        {
            return funct.Value;
        }

        private static void SetValue<T>(T configable, T value) where T : Configurable<T>
        {
            configable.SetValue(value);
        }

        private static void ProcessProperty(PropertyInfo property)
        {

        }

    }
}
