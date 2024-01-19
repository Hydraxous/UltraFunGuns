using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    public interface IStatusEffectReceiver
    {
        public void AddStatusEffect(IStatusEffect effect);
        public void RemoveStatusEffect(IStatusEffect effect);
        public IEnumerable<IStatusEffect> GetStatusEffects();

        public GameObject GetAffectedObject();
    }
}
