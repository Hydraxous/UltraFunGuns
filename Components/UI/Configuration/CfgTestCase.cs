using UltraFunGuns.UI;
using UnityEngine;
using UnityEngine.UI;

namespace UltraFunGuns.Components.UI.Configuration
{
    public static class CfgTestCase
    {
        [Configgable("Value Testing")]
        private static float configurableFloat = 12f;

        [Configgable("Value Testing")]
        [Range(0,10)]
        private static float rangedFloat = 0f;

        [Configgable("Value Testing")]
        private static int changableInteger = 45;

        [Configgable("Value Testing")]
        [Range(-5,5)]
        private static int rangedInteger = 1;

        [Configgable(path:"Value Testing")]
        private static void PrintOutFloats()
        {
            Debug.Log($"Your first float is {configurableFloat}");
            Debug.Log($"Your second float is {rangedFloat}");
            Debug.Log($"Your changable int is {changableInteger}");
            Debug.Log($"Your ranged int is {rangedInteger}");
        }

        [Configgable(path: "Value Testing")]
        private static void PrintOutString()
        {
            Debug.Log($"Your string is {noSpacesString.Value}");
        }

        [Configgable(path: "Value Testing", displayName:"Print Strict String")]
        private static void PrintOutStringStrict()
        {
            Debug.Log($"Your strict string is {strictString2.Value}");
        }


        [Configgable("Value Testing")]
        private static ConfigField<string> noSpacesString = new ConfigField<string>("Hello!", (s) =>
        {
            return !s.Contains(" ");
        });

        [Configgable("Value Testing", displayName:"Cant contain the number 4 or the letter a, and must be less than 6 characters long and greater than 2 characters.")]
        private static ConfigField<string> strictString2 = new ConfigField<string>("Hello!", (s) =>
        {

            if (s.Contains("4"))
                return false;

            if(s.Contains("a") || s.Contains("A"))
                return false;

            if(s.Length > 6)
                return false;

            if(s.Length < 2)
                return false;

            return true;

        });

        [Configgable("Value Testing", displayName: "Serialized String")]
        private static string simpleString = "Default string!";

        [Configgable(path: "Value Testing", displayName: "Print Simple String")]
        private static void PrintSimpleString()
        {
            Debug.Log($"Your string is {simpleString}");
        }

    }
}
