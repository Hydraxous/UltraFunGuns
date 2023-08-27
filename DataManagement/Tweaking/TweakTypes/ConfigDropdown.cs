using System;
using System.Collections.Generic;
using System.Text;

namespace UltraFunGuns
{
    public class ConfigDropdown<T> : Configurable<T>
    {
        public string[] Names { get; }
        public T[] Values { get; }

        public ConfigDropdown(T[] values, string[] names, int defaultIndex = 0) : base(values[defaultIndex])
        {
            Names = names;
            Values = values;
        }
    }
}
