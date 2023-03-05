using UnityEngine;

namespace UltraFunGuns
{
    public static class Prefabs
    {
        [UFGAsset("ThrownBrick")]
        public static GameObject ThrownBrick { get; private set; }

        [UFGAsset]
        public static GameObject BulletTrail { get; private set; }
    }
}
