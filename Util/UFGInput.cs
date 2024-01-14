using Configgy;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    public static class UFGInput
    {
        [Configgable("Binds", "Slot 7 Key", 1)]
        public static ConfigKeybind Slot7Key = new ConfigKeybind(KeyCode.Alpha7);

        [Configgable("Binds", "Slot 8 Key", 1)]
        public static ConfigKeybind Slot8Key = new ConfigKeybind(KeyCode.Alpha8);

        [Configgable("Binds", "Slot 9 Key", 1)]
        public static ConfigKeybind Slot9Key = new ConfigKeybind(KeyCode.Alpha9);

        [Configgable("Binds", "Slot 10 Key", 1)]
        public static ConfigKeybind Slot10Key = new ConfigKeybind(KeyCode.Alpha0);

        [Configgable("Binds", "Secret Use", 0)]
        public static ConfigKeybind SecretButton = new ConfigKeybind(KeyCode.K);

        [Configgable("Binds", "Open UFG Inventory", 0)]
        public static ConfigKeybind OpenInventory = new ConfigKeybind(KeyCode.I);
    }
}
