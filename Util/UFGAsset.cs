using System;
using System.Collections.Generic;
using System.Text;

namespace UltraFunGuns
{
    [AttributeUsage(AttributeTargets.Field)]
    public class UFGAsset : Attribute
    {
        public string Key { get; }
        public UFGAsset(string Key)
        {
            this.Key = Key;
        }
    }
}
