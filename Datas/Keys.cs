using System;
using System.Collections.Generic;
using System.Text;
using HydraDynamics.Keybinds;

namespace UltraFunGuns.Datas
{
    public static class Keys
    {
        private static KeybindManager keybindManager;
        public static KeybindManager KeybindManager
        {
            get
            {
                if(keybindManager == null)
                {
                    keybindManager = new KeybindManager(Data.DataManager);
                }
                return keybindManager;
            }
        }

        public static void Fetch(ref Keybinding fallback)
        {
            fallback.SetKeybindManager(KeybindManager);
            KeybindManager.Fetch(ref fallback);
        }

    }
}
