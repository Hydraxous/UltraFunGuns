using System;
using System.Linq;
using System.Text.RegularExpressions;
using UltraFunGuns.Configuration;

namespace UltraFunGuns
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method)]
    public class Configgable : Attribute
    {
        public string Path { get; }
        public string DisplayName { get; private set; }
        public string Description { get; private set; }
        public int OrderInList { get; }
        public string SerializationAddress { get; private set; }

        public ConfiggableMenu Owner { get; private set; }


        public Configgable(string path = "", string displayName = null, int orderInList = 0, string description = null) 
        {
            this.Path = path;
            this.DisplayName = displayName;
            this.Description = description;
            this.OrderInList = orderInList;
        }

        //Todo figure out a better solution to this please.

        public void SetSerializationAddress(string address)
        {
            this.SerializationAddress = address;
        }

        public void SetDisplayName(string name)
        {
            this.DisplayName = name;
        }

        public void SetDescription(string description)
        {
            this.Description = description;
        }

        public void SetOwner(ConfiggableMenu owner)
        {
            if (Owner != null)
                return;

            Owner = owner;
        }

        public void SetDisplayNameFromCamelCase(string camelCaseName)
        {
            string newName = camelCaseName;
            newName = Regex.Replace(newName, "^_", "").Trim();
            newName = Regex.Replace(newName, "([a-z])([A-Z])", "$1 $2").Trim();
            newName = Regex.Replace(newName, "([A-Z])([A-Z][a-z])", "$1 $2").Trim();
            newName = string.Concat(newName.Select(x => char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' ');

            if (newName.Length > 0)
                if (char.IsLower(newName[0]))
                {
                    char startChar = newName[0];
                    newName = newName.Remove(0, 1);
                    newName = char.ToUpper(startChar) + newName;
                }

            this.DisplayName = newName;
        }
    }
}
