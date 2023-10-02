using System;
using System.Collections.Generic;
using System.Text;
using UltraFunGuns.Components;
using UnityEngine;
using UnityEngine.Events;

namespace UltraFunGuns
{
    public class SimpleBreakable : MonoBehaviour, IBreakable
    {
        public Action OnBreak;
        public UnityEvent OnBreakEvent;

        public void Break()
        {
            BreakCore();
        }

        protected virtual void BreakCore()
        {
            OnBreak?.Invoke();
            OnBreakEvent?.Invoke();
        }
    }
}
