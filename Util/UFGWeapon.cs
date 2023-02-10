﻿using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.Reflection;

namespace UltraFunGuns
{
    public class WeaponInfo : Attribute
    {
        public Type Type { get; private set; }
        public string WeaponKey { get; }
        public string DisplayName { get; }
        public bool Equipped { get; }
        public int Slot { get; }
        public WeaponIconColor IconColor { get; }
        public WeaponAbility[] Abilities { get; private set; }

        public WeaponInfo(string WeaponKey, string DisplayName, int Slot, bool Equipped, WeaponIconColor IconColor)
        {
            this.WeaponKey = WeaponKey;
            this.DisplayName = DisplayName;
            this.Equipped = Equipped;
            this.Slot = Slot;
            this.IconColor = IconColor;
        }

        public void SetAbilities(WeaponAbility[] abilities)
        {
            Abilities = abilities;
            Array.Sort(Abilities);
        }

        public void SetType(Type type)
        {
            Type = type;
        }

        private static string[] randomPlaceholders = { "REDACTED", "DATA EXPUNGED", "INFORMATION DELETED", "DATA DELETED", "DATA MISSING", "UNKNOWN ORIGIN"};

        public string GetCodexText()
        {
            if (UltraFunGuns.DebugMode)
            {
                //return ToString();
            }

            if (!(Abilities.Length > 0) || Abilities == null)
            {
                return $"<size=6>[<color=grey>{randomPlaceholders[UnityEngine.Random.Range(0,randomPlaceholders.Length)]}</color>]</size>";
            }     

            string codexString = "";
            for (int i = 0; i < Abilities.Length; i++)
            {
                codexString += ((i > 0) ? "\n" : "") + Abilities[i].GetLine();
            }
            return codexString;

        }

        public bool TryGetGameobject(out GameObject prefab)
        {
            prefab = null;
            return HydraLoader.prefabRegistry.TryGetValue(WeaponKey, out prefab);
        }

        public override string ToString()
        {
            string debugMessage = "";

            MemberInfo[] members = Type.GetMembers();
            for(int i=0;i<members.Length;i++)
            {
                debugMessage += $"{members[i].Name}: {members[i].ToString()}\n";
            }

            return debugMessage;
        }

    }

    public class WeaponAbility : Attribute, IComparable
    {
        public RichTextColors NameColor;
        public string Name;
        public string Description;
        public int Priority;

        public WeaponAbility(string Name, string Description, int Priority = 0, RichTextColors NameColor = RichTextColors.white)
        {
            this.Name = Name;
            this.Description = Description;
            this.NameColor = NameColor;
            this.Priority = Priority;
        }

        public string GetLine()
        {
            string fullString = String.Format("<size=6><color={2}>{0}</color></size>\n<size=3>{1}</size>", Name, Description, NameColor.ToString());
            return fullString;
        }

        public int CompareTo(object obj)
        {
            return Priority.CompareTo(obj);
        }
    }

    public enum RichTextColors { aqua, black, blue, brown, darkblue, fuchsia, green, grey, lightblue, lime, magenta, maroon, navy, olive, orange, purple, red, silver, teal, white, yellow }
    public enum WeaponIconColor { Blue, Green, Red, Yellow}
}
