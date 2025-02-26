using UnityEngine;

namespace UltraFunGuns
{
    public class HLErrorNotifier : MonoBehaviour
    {
        private void Start()
        {

            UltraFunGuns.Log.LogError($"{gameObject.name} was created, but it was not loaded properly from the assetbundle.");
        }
    }
}
