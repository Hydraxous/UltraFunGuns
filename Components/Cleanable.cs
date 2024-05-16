using UnityEngine;

namespace UltraFunGuns.Components
{
    public class Cleanable : MonoBehaviour, ICleanable
    {
        public void Cleanup()
        {
            Destroy(gameObject);
        }
    }
}
