using System;
using UnityEngine;
using UnityEngine.Events;

namespace UltraFunGuns
{
    public class Explodable : MonoBehaviour, IExplodable
    {
        public Action<Explosion> OnExplode;
        public UnityEvent<Explosion> OnExploded;

        public void Explode(Explosion explosion)
        {
            ExplodeCore(explosion);
        }

        protected virtual void ExplodeCore(Explosion explosion)
        {
            OnExplode?.Invoke(explosion);
            OnExploded?.Invoke(explosion);
        }
    }
}
