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
        [UFGAsset("HydraDevPlushie")] public static GameObject HydraPlushie { get; private set; }

        public static UKAsset<GameObject> UK_Explosion { get; private set; } = new UKAsset<GameObject>("Assets/Prefabs/Explosion.prefab");
        public static UKAsset<GameObject> UK_ExplosionMalicious { get; private set; } = new UKAsset<GameObject>("Assets/Prefabs/ExplosionMaliciousRail.prefab");

        public static UKAsset<Font> VCR_Font { get; private set; } = new UKAsset<Font>("Assets/Fonts/VCR_OSD_MONO_1.001.ttf");

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
        public static UKAsset<GameObject> FishingRod { get; private set; } = new UKAsset<GameObject>("Assets/Prefabs/Fishing/Fishing Rod Weapon.prefab");


        [UFGAsset("ShittyExplosion")] public static GameObject ShittyExplosionFX { get; private set; }

        public static UKAsset<AudioClip> ShittyExplosionSound { get; private set; } = new UKAsset<AudioClip>("Assets/Sounds/Credits/ThatExplosionSound.wav");
        public static UKAsset<AudioClip> BonusBreakSound { get; private set; } = new UKAsset<AudioClip>("Assets/Sounds/Environment/bonusbreak.wav");

        [UFGAsset("SmackFX")] public static GameObject SmackFX { get; private set; }

        [UFGAsset("BlackSmokeShockwave")] public static GameObject BlackSmokeShockwave { get; private set; }

    }
}
