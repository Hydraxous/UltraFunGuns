using System;
using UnityEngine;
using UnityEngine.Events;

namespace UltraFunGuns
{
    public class Parriable : MonoBehaviour, IParriable
    {
        public Func<Vector3, Vector3, bool> OnParryCheck;
        public Action<Vector3, Vector3> OnParry;
        public UnityEvent OnParried;

        public bool Parry(Vector3 position, Vector3 direction)
        {
            return ParryCore(position, direction);
        }

        protected virtual bool ParryCore(Vector3 position, Vector3 direction)
        {
            OnParry(position, direction);
            OnParried?.Invoke();

            if (OnParryCheck == null)
                return true;

            return OnParryCheck.Invoke(position, direction);
        }
    }
}
