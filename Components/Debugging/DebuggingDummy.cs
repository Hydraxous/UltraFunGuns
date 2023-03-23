using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Transactions;
using UltraFunGuns.Datas;
using UnityEngine;
using HydraDynamics.Keybinds;

namespace UltraFunGuns
{
    public class DebuggingDummy : MonoBehaviour
    {
        private ExampleClass example = new ExampleClass();

        private Keybinding testKey = Keys.KeybindManager.Fetch(new Keybinding("TestKey", KeyCode.Keypad1));

        private void Start()
        {
            



            return;
            foreach(KeyValuePair<string, string> dependancy in AssetManager.Instance.assetDependencies)
            {
                HydraLogger.Log($"{dependancy.Key}:{dependancy.Value}");
            }
            
        }

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.G))
            {
                example.DoThing();
            }

            if (Input.GetKeyDown(KeyCode.H))
            {
                example.SetStuff(true, 0);
            }

            if (Input.GetKeyDown(KeyCode.J))
            {
                example.SetStuff(false, 2);
            }

            if (Input.GetKeyDown(KeyCode.Keypad8))
            {
                PlayerDeathPatch.god = !PlayerDeathPatch.god;
                if(PlayerDeathPatch.god)
                {
                    SonicReverberator.vineBoom_Loud.PlayAudioClip();
                }
            }

            if (testKey.WasPerformedThisFrame)
            {
                HydraLogger.Log($"Test Key Pressed: {testKey.KeyCode}", DebugChannel.Warning);
            }

            if(Input.GetKeyDown(KeyCode.Keypad0))
            {
                HydraLogger.Log("Started rebinding.", DebugChannel.Warning);
                testKey.Rebind();
            }
             
        }

    }
}
