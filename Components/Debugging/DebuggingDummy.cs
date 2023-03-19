using System;
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
        }
    }
}
