using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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
        internal static void RegisterConfiguraitonMenu(ConfiggableMenu menu)
        {
            menus.Add(menu);
            OnMenusChanged?.Invoke(GetMenus());
        }

        internal static List<ConfiggableMenu> menus = new List<ConfiggableMenu>();

        internal static Action<ConfiggableMenu[]> OnMenusChanged;

        internal static ConfiggableMenu[] GetMenus()
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
