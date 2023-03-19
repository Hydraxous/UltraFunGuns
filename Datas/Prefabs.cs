using UltraFunGuns.Datas;
using UnityEngine;

namespace UltraFunGuns
{
    public static class Prefabs
    {
        [UFGAsset("ThrownBrick")] public static GameObject ThrownBrick { get; private set; }
        [UFGAsset("BulletTrail")] public static GameObject BulletTrail { get; private set; }
        [UFGAsset("BulletImpactFX")] public static GameObject BulletImpactFX { get; private set; }
        [UFGAsset("CanLauncher_MuzzleFX")] public static GameObject CanLauncher_MuzzleFX { get; private set; }
        //[UFGAsset("Assets/Prefabs/Explosion.prefab", true)] public static GameObject UK_Explosion { get; private set; }

        public static UKAsset<GameObject> UK_Explosion { get; private set; } = new UKAsset<GameObject>("Assets/Prefabs/Explosion.prefab");
        public static UKAsset<GameObject> HakitaPlush { get; private set; } = new UKAsset<GameObject>("Assets/Prefabs/Items/DevPlushies/DevPlushie (Hakita).prefab");
        public static UKAsset<GameObject> PITRPlush { get; private set; } = new UKAsset<GameObject>("Assets/Prefabs/Items/DevPlushies/DevPlushie (PITR).prefab");
        public static UKAsset<GameObject> HeckPlush { get; private set; } = new UKAsset<GameObject>("Assets/Prefabs/Items/DevPlushies/DevPlushie (Heckteck).prefab");
        public static UKAsset<GameObject> MegaNekoPlush { get; private set; } = new UKAsset<GameObject>("Assets/Prefabs/Items/DevPlushies/DevPlushie (Meganeko).prefab");
        public static UKAsset<GameObject> JerichoPlush { get; private set; } = new UKAsset<GameObject>("Assets/Prefabs/Items/DevPlushies/DevPlushie (Jericho).prefab");
        public static UKAsset<GameObject> KGCPlush { get; private set; } = new UKAsset<GameObject>("Assets/Prefabs/Items/DevPlushies/DevPlushie (KGC).prefab");
        public static UKAsset<GameObject> JoyPlush { get; private set; } = new UKAsset<GameObject>("Assets/Prefabs/Items/DevPlushies/DevPlushie (Joy).prefab");
        public static UKAsset<GameObject> DaliaPlush { get; private set; } = new UKAsset<GameObject>("Assets/Prefabs/Items/DevPlushies/DevPlushie (Dalia).prefab");
        public static UKAsset<GameObject> SaladPlush { get; private set; } = new UKAsset<GameObject>("Assets/Prefabs/Items/DevPlushies/DevPlushie (Salad).prefab");
        public static UKAsset<GameObject> LucasPlush { get; private set; } = new UKAsset<GameObject>("Assets/Prefabs/Items/DevPlushies/DevPlushie (Lucas).prefab");
        public static UKAsset<GameObject> DawgPlush { get; private set; } = new UKAsset<GameObject>("Assets/Prefabs/Items/DevPlushies/DevPlushie (Dawg).prefab");
        public static UKAsset<GameObject> CameronPlush { get; private set; } = new UKAsset<GameObject>("Assets/Prefabs/Items/DevPlushies/DevPlushie (Cameron).prefab");
        public static UKAsset<GameObject> MakoPlush { get; private set; } = new UKAsset<GameObject>("Assets/Prefabs/Items/DevPlushies/DevPlushie (Mako).prefab");
        public static UKAsset<GameObject> MandyPlush { get; private set; } = new UKAsset<GameObject>("Assets/Prefabs/Items/DevPlushies/DevPlushie (Mandy).prefab");
        public static UKAsset<GameObject> VvizardPlush { get; private set; } = new UKAsset<GameObject>("Assets/Prefabs/Items/DevPlushies/DevPlushie (Vvizard).prefab");
        public static UKAsset<GameObject> FrancisPlush { get; private set; } = new UKAsset<GameObject>("Assets/Prefabs/Items/DevPlushies/DevPlushie (Francis).prefab");
        public static UKAsset<GameObject> GianniPlush { get; private set; } = new UKAsset<GameObject>("Assets/Prefabs/Items/DevPlushies/DevPlushie (Gianni).prefab");
        public static UKAsset<GameObject> BigRockPlush { get; private set; } = new UKAsset<GameObject>("Assets/Prefabs/Items/DevPlushies/DevPlushie (BigRock).prefab");
        public static UKAsset<GameObject> ScottPlush { get; private set; } = new UKAsset<GameObject>("Assets/Prefabs/Items/DevPlushies/DevPlushie (Scott).prefab");
        public static UKAsset<GameObject> V1Plush { get; private set; } = new UKAsset<GameObject>("Assets/Prefabs/Items/DevPlushies/DevPlushie.prefab");

        /*
        public static GameObject UK_Explosion 
        {
            get
            {
                if(uk_explosion == null)
                {
                    //uk_explosion = AssetManager.Instance.loadedBundles["bundle-0"].LoadAsset<GameObject>("Explosion.prefab");
                    uk_explosion = AssetHelper.LoadPrefab("Assets/Prefabs/Explosion.prefab");
                }
                return uk_explosion;
            }
        }

        private static GameObject uk_explosion;
        */
        [UFGAsset("SmackFX")] public static GameObject SmackFX { get; private set; }

    }
}
