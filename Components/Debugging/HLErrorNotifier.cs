using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    public class HLErrorNotifier : MonoBehaviour
    {
        private void Start()
        {

            HydraLogger.Log($"{gameObject.name} was created, but it was not loaded properly from the assetbundle.", DebugChannel.Error);
        }
    }
}
