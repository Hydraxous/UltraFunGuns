using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using UltraFunGuns.UI;
using UnityEngine;

namespace UltraFunGuns.Configuration
{
    public static class ConfigurationManager
    {
        internal static void RegisterConfiguraitonMenu(ConfigBuilder menu)
        {
            if (menus.Select(x => x.GUID).Contains(menu.GUID))
                throw new DuplicateNameException($"{nameof(ConfigBuilder)} GUID ({menu.GUID}) already exists! Using two ConfiggableMenus with the same GUID is not allowed.");

            menus.Add(menu);
            OnMenusChanged?.Invoke(GetMenus());
        }

        private static List<ConfigBuilder> menus = new List<ConfigBuilder>();

        internal static Action<ConfigBuilder[]> OnMenusChanged;

        internal static ConfigBuilder[] GetMenus()
        {
            return menus.ToArray();
        }

        internal static object GetObjectAtAddress(string address)
        {
            if (!Data.Config.Data.Configgables.ContainsKey(address))
                return null;

            return Data.Config.Data.Configgables[address];
        }

        internal static void SetObjectAtAddress(string address, object value)
        {
            Data.Config.Data.Configgables[address] = value;
        }

        internal static void Save()
        {
            Data.Config.Save();
        }

        internal static void SubMenuElementsChanged() 
        {
            OnMenusChanged?.Invoke(GetMenus());
        }
    }
}
