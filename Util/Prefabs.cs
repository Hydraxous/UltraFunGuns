using UnityEngine;

namespace UltraFunGuns
{
    public static class Prefabs
    {
        [UFGAsset("ThrownBrick")] public static GameObject ThrownBrick { get; private set; }
        [UFGAsset("BulletTrail")] public static GameObject BulletTrail { get; private set; }
        [UFGAsset("BulletImpactFX")] public static GameObject BulletImpactFX { get; private set; }
        [UFGAsset("CanLauncher_MuzzleFX")] public static GameObject CanLauncher_MuzzleFX { get; private set; }
        public static GameObject UK_Explosion 
        {
            get
            {
                if(uk_explosion == null)
                {
                    uk_explosion = AssetHelper.LoadPrefab("Explosion.prefab");
                }
                return uk_explosion;
            }
        }

        private static GameObject uk_explosion;

        [UFGAsset("SmackFX")] public static GameObject SmackFX { get; private set; }

    }
}
