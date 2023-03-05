using System;
using System.Collections.Generic;
using System.Text;

namespace UltraFunGuns
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class UFGAsset : Attribute
    {
        public string Key { get; }
        /// <summary>
        /// Asset metadata
        /// </summary>
        /// <param name="Key">Name of the asset, this willbe the member name if left blank.</param>
        public UFGAsset(string Key = "")
        {
            this.Key = Key;
        }
    }
}
