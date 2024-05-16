using UnityEngine;

namespace UltraFunGuns
{
    public class WeaponDef : ScriptableObject
    {
        public string DisplayName;
        public GameObject Prefab;
        public GameObject Model;
        public int VariantColorIndex;
        public Sprite HUDIcon;
        public Sprite HUDIconGlow;

        [TextArea(20, 60)] public string Lore;
        [TextArea(20, 60)] public string Usage;
    }
}
