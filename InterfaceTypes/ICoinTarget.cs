using UnityEngine;

namespace UltraFunGuns
{
    public interface ICoinTarget
    {
        public Transform GetCoinTargetPoint(Coin coin);
        public bool CanBeCoinTargeted(Coin coin);
        public void OnCoinReflect(Coin coin, RevolverBeam beam);
        public int GetCoinTargetPriority(Coin coin);
    }
}
