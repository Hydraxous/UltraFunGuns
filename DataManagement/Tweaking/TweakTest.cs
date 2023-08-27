using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    public class TweakTest : MonoBehaviour
    {

        private UltraFunGunBase.ActionCooldown cooldown = new UltraFunGunBase.ActionCooldown(2f);

        [ConfigMenuOption("Debug/Test String")]
        private static Configurable<string> tweakedString = new Configurable<string>("Default Text");

        [ConfigMenuOption("Debug/My Tweaked Float")]
        private static ConfigSlider<float> tweakedFloat = new ConfigSlider<float>(2f, 2f, 25f, 1f);
        
        [ConfigMenuOption("Debug/My Integer Dropdown")]
        private static ConfigDropdown<int> intValues = new ConfigDropdown<int>(new int[]{ 0, 2, 4}, new string[] { "none", "few", "some" }, 1);

        [ConfigMenuOption("Debug/My Boolean")]
        private static Configurable<bool> booleanValue = new Configurable<bool>(true);

        [ConfigMenuOption("Debug/Spawn Count")]
        private static ConfigSlider<int> spawnCount = new ConfigSlider<int>(2, 2, 120, 1);

        private static ConfigConstrainedField<int> validatedInteger = new ConfigConstrainedField<int>(0, (x) => 
        {
            if(x == 0)
                return false;

            if (x == 2)
                return false;

            if (x == 3)
                return true;

            return false;
        });

        private static readonly string[] tweakerDisplays =
        {
            "First",
            "Second",
            "Third",
        };

        private static string[] tweakerObjects = new string[]
        {
            "This Is the First String",
            "This Is the Second String",
            "This Is the Third String",
        };

    private static Dictionary<string, object> tweakStringOptions = new Dictionary<string, object>()
        {
            { "Forward", "fwd" },
            { "Backwards", "bckwd" },
            { "Left", "left" },
            { "Right", "right" },
            { "Down", "down" }
        };

        private void Update()
        {
            bool bruh = (spawnCount.Value > 2);

            if(cooldown.CanFire())
            {
                Debug.Log(tweakedString.Value);
            }
        }
    }
}
