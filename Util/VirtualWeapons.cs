using System.Collections.Generic;
using UnityEngine;

namespace UltraFunGuns.Util
{
    public static class VirtualWeapons
    {
        
    }


    public class VirtualExplosion
    {
        public Vector3 Position { get; private set; }
        public float Radius { get; private set; }
        public float Power { get; private set; }

        public VirtualExplosion(Vector3 position, float radius)
        {
            Position = position;
            Radius = radius;
        }

        public EnemyIdentifier[] GetAffectedEnemies()
        {
            Collider[] hitObjects = Physics.OverlapSphere(Position, Radius, LayerMaskDefaults.Get(LMD.Enemies));
            UltraFunGuns.Log.LogWarning($"Hit Obejcts {hitObjects.Length}");
            List<EnemyIdentifier> enemies = new List<EnemyIdentifier>();
            foreach(Collider hitCol in hitObjects)
            {
                if (!hitCol.IsColliderEnemy(out EnemyIdentifier eid))
                {
                    continue;
                }

                if (enemies.Contains(eid))
                {
                    continue;
                }

                enemies.Add(eid);
            }
            return enemies.ToArray();
        }
    }
}
