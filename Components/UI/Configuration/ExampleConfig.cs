using System;
using UltraFunGuns.UI;
using UnityEngine;
using UnityEngine.UI;

namespace UltraFunGuns.Components.UI.Configuration
{
    public static class ExampleConfig
    {
        /*
        #region attribute usage

        //This is the basic usage. It will appear on your plugins root page with its typed name.
        
        [Configgable]
        public static float configurableFloat;

        //Path determines where the config UI element is located within the menus. For each piece of a path separated by '/', a menu page will
        //be created, and a button that opens that page will be created on it's parent. In this example a button called Example Floats would appear on the Examples page.
        
        [Configgable(path:"Examples/Example Floats")]
        public static float mySecondFloat;

        //Displayname changes the display name of the element when it is created instead of using the field's name.
        
        [Configgable(path:"Examples/Example Floats", displayName:"My Renamed Example Float")]
        public static float myThirdFloat;

        //Order in list controls list priority. Lower numbers have higher priority.
        
        [Configgable(path:"Examples/Example Floats", orderInList:1)]
        public static float myFourthFloat;

        //Lastly, an optional description tag that will function will all of the base elements provided and generate a
        //button on the element that toggles viewing the description. Custom ones will need to add their own implementation for this.
        
        [Configgable(path: "Examples/Example Floats", description:"This is the float's description!")]
        public static float myFifthFloat;
        
        //By default all primitive declarations are converted to ConfigInputField with the exception of boolean values which are converted to ConfigToggle
        //Additionally, floats and integers both support use of the Range attribute, which will make the element a slider instead of a field.

        [Configgable(path: "Examples/Example Floats")]
        [Range(0, 20)]
        public static float myRangedFloat = 5f;

        #endregion

        #region primitives

        //The configgable attribute supports 4 types without defining the ConfigElement type wrappers.
        //float, int, bool, and string.

        [Configgable(path: "Examples/Simple")]
        public static float primitiveFloat;

        [Configgable(path: "Examples/Simple")]
        public static int primitiveInt;

        [Configgable(path: "Examples/Simple")]
        public static bool primitiveBool;

        [Configgable(path: "Examples/Simple")]
        public static string primitiveString;

        #endregion

        #region elements
        //There are some cases in which you may want to use wrappers for your elements.
        //Wrappers have event hooks for value changed for example.
        //You can also have any type of input validation with Input Fields.

        //ConfigElements still require the Configgable attribute to be registered at runtime. Don't forget it!

        //For buttons, you can provide a Label or a DisplayName within the configgable attribute.
        //A defined Label will take priority over a DisplayName definition when the button is created in menus.
        [Configgable(path: "Examples/Config Elements")]
        public static ConfigButton MyConfigButton = new ConfigButton(()=>
        {
            Debug.Log("You pressed your config button!");
        });

        //These three are pretty self explanitory.

        [Configgable(path: "Examples/Config Elements")]
        public static IntegerSlider MyIntegerSlider = new IntegerSlider(42, 0, 42);

        [Configgable(path: "Examples/Config Elements")]
        public static FloatSlider MyFloatSlider = new FloatSlider(0, -1, 1);

        [Configgable(path: "Examples/Config Elements")]
        public static ConfigToggle MyConfigToggle = new ConfigToggle(false);

        //A config input field can be literally any type that has a TypeConverter and will convert from a string value.
        //You can also provide a clause to validate the input that goes into the field.
        //For the lambda, the value provided will be typeof(T) provided when declaring the ConfigInputField.
        //It will be converted from a string first using TypeConverters. If you wish to use custom conversion for this, you need to write a custom type converter.

        //The return value is whether the user input is valid or not. If you don't provide any validation clause, it will always return true if the conversion succeeds.
        //If the conversion of the type fails, the value won't be changed at all.

        [Configgable(path: "Examples/Config Elements")]
        public static ConfigInputField<string> MyStringField = new ConfigInputField<string>("Default String");

        [Configgable(path: "Examples/Config Elements", description: "You cannot type Hydra into this field.")]
        public static ConfigInputField<string> StrictStringField = new ConfigInputField<string>("Strict String", (s) =>
        {
            if (s.Contains("Hydra"))
                return false;

            return true;
        });

        //This is very cursed.
        //However, you can define custom type conversion like this.
        //Leaving typeConverter as null will allow it to convert primitives, but wont allow it to convert more complex types.

        [Configgable(path: "Examples/Config Elements", description: "goobr field")]
        public static ConfigInputField<GooberStruct> GooberStructField = new ConfigInputField<GooberStruct>(new GooberStruct
        {
            Number = 42,
            Text = "Hello!",
            State = true
        }, typeConverter: (s) =>
        {
            ValueTuple<bool, GooberStruct> r = (false, new GooberStruct());

            string[] parts = s.Split('.');
            if (parts.Length != 3)
                return r;

            r.Item2.Text = parts[0];

            if (!int.TryParse(parts[1], out int intValue))
                return r;

            r.Item2.Number = intValue;

            if (!bool.TryParse(parts[2], out bool boolValue))
                return r;

            r.Item2.State = boolValue;

            r.Item1 = true;
            return r;
        });
        

        [System.Serializable]
        public struct GooberStruct
        {
            public string Text;
            public int Number;
            public bool State;

            //ToString will always be used for the input field fill text
            public override string ToString()
            {
                return $"{Text}.{Number}.{State}";
            }
        }

     
        #endregion
        */
    }
}
