using System.Collections.Generic;
using UnityEngine;

namespace UltraFunGuns
{
    public class EntityStatusEffectHolder : MonoBehaviour, IStatusEffectReceiver
    {
        private List<IStatusEffect> currentEffects = new List<IStatusEffect>();

        public GameObject GetAffectedObject()
        {
            return gameObject;
        }

        public void AddStatusEffect(IStatusEffect effect)
        {
            currentEffects.Add(effect);
        }

        public IEnumerable<IStatusEffect> GetStatusEffects()
        {
            return currentEffects;
        }

        public void RemoveStatusEffect(IStatusEffect effect)
        {
            currentEffects.Remove(effect);
        }
    }
}
