using System;
using System.Collections.Generic;
using System.Text;


namespace UltraFunGuns
{
    public class WeaponInfo : Attribute
    {
        public string WeaponKey { get; }
        public string DisplayName { get; }
        public bool Equipped { get; }
        public int Slot { get; }
        public WeaponIconColor IconColor { get; }

        public WeaponInfo(string WeaponKey, string DisplayName, int Slot, bool Equipped, WeaponIconColor IconColor)
        {
            this.WeaponKey = WeaponKey;
            this.DisplayName = DisplayName;
            this.Equipped = Equipped;
            this.Slot = Slot;
            this.IconColor = IconColor;
        }

    }

    public enum WeaponIconColor { Blue, Green, Red, Yellow}
}
