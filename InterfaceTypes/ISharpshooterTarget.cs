using UnityEngine;

namespace UltraFunGuns
{
    public interface ISharpshooterTarget
    {
        public bool CanBeSharpshot(RevolverBeam beam, RaycastHit hit);
        public Vector3 GetSharpshooterTargetPoint();
        public void OnSharpshooterTargeted(RevolverBeam beam, RaycastHit hit);
        
        //Currently unused.
        public int GetSharpshooterTargetPriority();
    }
}
