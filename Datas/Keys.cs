using System;
using System.Collections.Generic;
using System.Text;
using HydraDynamics.Keybinds;

namespace UltraFunGuns.Datas
{
    public static class Keys
    {
        public static KeybindManager KeybindManager { get; private set; } = new KeybindManager(Data.DataManager);
    }
}
