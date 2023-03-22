using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Transactions;
using UltraFunGuns.Datas;
using UnityEngine;

namespace UltraFunGuns
{
    public class DebuggingDummy : MonoBehaviour
    {
        private ExampleClass example = new ExampleClass();

        private UFGBind testKey = new UFGBind("TestKey", KeyCode.Keypad1);

        private void Start()
        {
            HydraLogger.Log(DataManager.InventoryData.Data.text + $"\n\n\n{DataManager.InventoryData.Data.number}\n\n", DebugChannel.Warning);



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

            if(Input.GetKeyDown(KeyCode.Keypad0) && !rebinding)
            {
                HydraLogger.Log("Started rebinding.", DebugChannel.Warning);
                RebindKey();
            }
             
        }


        private void RebindKey()
        {
            StartCoroutine(RebindProcess(testKey));
        }

        private bool rebinding = false;

        private IEnumerator RebindProcess(UFGBind bind)
        {
            float timer = 10.0f;
            rebinding = true;
            int counter = 0;

            yield return new WaitForSeconds(0.25f);

            while(rebinding && timer > 0.0f)
            {
                yield return null;
                timer -= Time.deltaTime;

                Event current = Event.current;

                if(current.type != EventType.KeyDown && current.type != EventType.KeyUp)
                {
                    continue;
                }

                switch(current.keyCode)
                {
                    case KeyCode.None:
                        if(counter % 60 == 0)
                        {
                            HydraLogger.Log($"KeyCodeNone", DebugChannel.Warning);
                        }
                        break;

                    case KeyCode.Escape:
                        rebinding = false;
                        break;
                    default:
                        HydraLogger.Log($"Rebinding to {current.keyCode}", DebugChannel.Warning);
                        bind.SetBind(current.keyCode);
                        rebinding = false;
                        continue;
                }

                counter++;
            }
            rebinding = false;
            HydraLogger.Log($"Rebinding stopped", DebugChannel.Warning);
        }

    }
}
