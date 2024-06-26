﻿using UnityEngine;

namespace UltraFunGuns
{
    public class HydraPlushie : MonoBehaviour
    {
        [SerializeField] private MeshRenderer screen;
        [SerializeField] private Texture2D blueScreenTex, blueScreenFaceMask;
        [SerializeField] private AudioClip blueScreenClip;
        [SerializeField] private AudioSource toChange;
        [SerializeField] private GameObject breakFX;

        private bool damaged;

        private void WaterDamage()
        {

            if (screen == null || blueScreenTex == null || blueScreenFaceMask == null || damaged)
                return;

            damaged = true;

            if (screen.material.HasProperty("_MainTex"))
            {
                screen.material.SetTexture("_MainTex", blueScreenTex);

            }

            if (screen.material.HasProperty("_FaceMask"))
            {
                screen.material.SetTexture("_FaceMask", blueScreenFaceMask);
            }

            if(toChange != null)
            {
                toChange.clip = blueScreenClip;
            }

            foreach(AudioSource src in transform.GetComponentsInChildren<AudioSource>())
            {
                src.pitch = src.pitch - (src.pitch * 0.23f);
            }

            if (breakFX != null)
            {
                Instantiate(breakFX, transform);
            }

            WeaponManager.SetWeaponUnlocked("FizzyGun", true);
            WeaponManager.SetWeaponUnlocked("GrabbityGun", true);
            WeaponManager.SetWeaponUnlocked("PlushieCannon", true);
        }

        private void OnTriggerEnter(Collider col)
        {
            if (col.gameObject.layer == 4 || col.tag.ToLower().Contains("water") || col.gameObject.TryGetComponent<Water>(out Water water))
            {
                WaterDamage();
            }
        }

        private void OnDestroy()
        {
            WaterDamage();
        }
    }
}
