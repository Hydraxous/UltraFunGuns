using UnityEngine;

namespace UltraFunGuns.Components
{
    public class CoinTarget : MonoBehaviour, ICoinTarget
    {

        private void Awake()
        {
            UltraFunGuns.Log.Log("CT: I'm in danger!");
        }

        public bool CanBeCoinTargeted(Coin coin)
        {
            return true;
        }

      
        public int GetCoinTargetPriority(Coin coin)
        {
            return 0;
        }

        public void OnCoinReflect(Coin coin, RevolverBeam beam)
        {
            UltraFunGuns.Log.Log("REFLECT COIN!");
            UltraFunGuns.Log.Log($"Coin: {coin.name} Beam: {beam.name}");
        }

        public Transform GetCoinTargetPoint(Coin coin)
        {
            return transform;
        }
    }
}
