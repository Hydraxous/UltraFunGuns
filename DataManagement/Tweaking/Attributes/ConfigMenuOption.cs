using System;
using System.Collections.Generic;
using System.Text;

namespace UltraFunGuns
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class ConfigMenuOption : Attribute
    {
        public string Path { get; }

        public ConfigMenuOption(string path)
        {
            Path = path;
        }
    }
}
