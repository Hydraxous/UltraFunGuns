using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.Reflection;

namespace UltraFunGuns
{
    [AttributeUsage(AttributeTargets.Class)]
    public class UFGWeapon : Attribute
    {
        public Type Type { get; private set; }
        public string WeaponKey { get; }
        public string DisplayName { get; }
        public bool Equipped { get; }
        public int Slot { get; }

        public bool Finished { get; }
        public WeaponIconColor IconColor { get; }
        public WeaponAbility[] Abilities { get; private set; }
        public bool StartUnlocked { get; }
        public GameObject Prefab { get; private set; }

        private Sprite weaponIcon;
        public Sprite WeaponIcon
        {
            get
            {
                if (weaponIcon == null)
                {
                    //TODO fix this.
                    return weaponIcon;
                }
                return weaponIcon;
            }
        }

        private Sprite glowIcon;
        public Sprite GlowIcon
        {
            get
            {
                if (glowIcon == null)
                {
                    //Get icon from loader.

                    return glowIcon;
                }
                return glowIcon;
            }
        }

        /// <summary>
        /// Fun Gun attribute will register the weapon when the mod loads and is used for metadata.
        /// </summary>
        /// <param name="WeaponKey">Used for finding the weapon, or loading weapon data MUST BE UNIQUE.</param>
        /// <param name="DisplayName">Name shown in game</param>
        /// <param name="Slot">Default slot</param>
        /// <param name="Equipped">Default Equipped</param>
        /// <param name="IconColor">Color of the Weapon Icon</param>
        /// <param name="StartUnlocked">Should this be loaded in a non-debug build?</param>
        public UFGWeapon(string WeaponKey, string DisplayName, int Slot, bool Equipped, WeaponIconColor IconColor, bool StartUnlocked = true, bool finished = true)
        {
            this.WeaponKey = WeaponKey;
            this.DisplayName = DisplayName;
            this.Equipped = Equipped;
            this.Slot = Slot;
            this.IconColor = IconColor;
            this.StartUnlocked = StartUnlocked;
            this.Finished = finished;
        }

        public void SetAbilities(WeaponAbility[] abilities)
        {
            Abilities = abilities;
            Array.Sort(Abilities);
        }

        public void SetType(Type type)
        {
            if(Type == null)
            {
                Type = type;
            }
        }

        public void SetPrefab(GameObject prefab)
        {
            if(Prefab == null)
            {
                Prefab = prefab;
            }
        }

        private static string[] randomPlaceholders = { "REDACTED", "DATA EXPUNGED", "INFORMATION DELETED", "DATA DELETED", "DATA MISSING", "UNKNOWN ORIGIN", "404-ERROR", "MISSING", "CLASSIFIED", "UNKNOWN", "??????"};

        public string GetCodexText()
        {
            if (UltraFunGuns.DebugMode)
            {
                //return ToString();
            }

            if (!(Abilities.Length > 0) || Abilities == null)
            {
                return $"<size=4>[<color=grey>{randomPlaceholders[UnityEngine.Random.Range(0,randomPlaceholders.Length)]}</color>]</size>";
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
                debugMessage += $"{members[i].Name}: {members[i]}\n";
            }

            return debugMessage;
        }

    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
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

        //Formats the weapon ability line
        public string GetLine()
        {
            string fullString = String.Format("<size=4><color={2}>{0}</color></size>\n<size=3>{1}</size>", Name, Description, NameColor.ToString());
            return fullString;
        }

        public int CompareTo(object obj)
        {
            if(obj == null)
            {
                return 1;
            }

            WeaponAbility ability = obj as WeaponAbility;

            if(ability != null)
            {
                return this.Priority.CompareTo(ability.Priority);
            }else
            {
                throw new ArgumentException("Compared object is not a weaponability.");
            }
        }
    }

    public enum RichTextColors { aqua, black, blue, brown, darkblue, fuchsia, green, grey, lightblue, lime, magenta, maroon, navy, olive, orange, purple, red, silver, teal, white, yellow }
    public enum WeaponIconColor { Blue, Green, Red, Yellow}

    public static class WeaponIconColorExtensions
    {
        public static Color ToColor(this WeaponIconColor color)
        {
            if(ColorBlindSettings.Instance != null)
                return ColorBlindSettings.Instance.variationColors[(int)color];

            switch (color)
            {
                case WeaponIconColor.Blue:
                    return Color.blue;
                case WeaponIconColor.Green:
                    return Color.green;
                case WeaponIconColor.Red:
                    return Color.red;
                case WeaponIconColor.Yellow:
                    return Color.yellow;
                default:
                    return Color.white;
            }
        }
    }
}
