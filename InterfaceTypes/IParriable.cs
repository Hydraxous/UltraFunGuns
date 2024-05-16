using UnityEngine;

namespace UltraFunGuns
{
    public interface IParriable
    {
        public bool Parry(Vector3 position, Vector3 direction);
    }
}
