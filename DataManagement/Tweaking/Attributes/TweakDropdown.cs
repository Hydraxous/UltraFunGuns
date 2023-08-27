using HarmonyLib;
using Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UltraFunGuns
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class TweakDropdown : Attribute
    {
        public object[] Values { get; }
        public string[] DisplayedValues { get; }


        public TweakDropdown(object[] values, string[] displayedValues = null) 
        {
            Values = values;
            
            if(displayedValues == null)
            {
                DisplayedValues = Values.Select(x=>x.ToString()).ToArray();
            }
            else
            {
                DisplayedValues = displayedValues;
            }
        }
    }
}
