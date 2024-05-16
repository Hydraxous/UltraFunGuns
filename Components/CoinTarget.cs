using UnityEngine;

namespace UltraFunGuns.Components
{
    public class CoinTarget : MonoBehaviour, ICoinTarget
    {

        private void Awake()
        {
            Debug.Log("CT: I'm in danger!");
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
            Debug.Log("REFLECT COIN!");
            Debug.Log($"Coin: {coin.name} Beam: {beam.name}");
        }

        public Transform GetCoinTargetPoint(Coin coin)
        {
            return transform;
        }
    }
}
