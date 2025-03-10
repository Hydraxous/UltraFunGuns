﻿using System.Collections.Generic;
using UltraFunGuns.Components;
using UnityEngine;

namespace UltraFunGuns
{
    //Base class for UFG weapons.
    public abstract class UltraFunGunBase : MonoBehaviour, IUFGWeapon
    {
        public Dictionary<string, AudioSource> soundEffects = new Dictionary<string, AudioSource>();

        protected Transform mainCam, firePoint;
        protected OptionsManager om;
        protected NewMovement player;
        protected WeaponIcon weaponIcon;
        protected WeaponIdentifier weaponIdentifier;
        protected Animator animator;
        protected WeaponTextureSwapper weaponTextureSwapper;

        protected UFGWeapon weaponInfo;

        public bool IsDuplicate
        {
            get
            {
                if (weaponIdentifier == null)
                    return false;

                return weaponIdentifier.duplicate;
            }
        }

        protected virtual void DoAnimations() { }

        private void Awake()
        {
            weaponInfo = WeaponManager.GetWeaponInfo(this.GetType());
            weaponTextureSwapper = GetComponent<WeaponTextureSwapper>();
            if(weaponTextureSwapper!= null)
                weaponTextureSwapper.WeaponName = weaponInfo.WeaponKey;
            mainCam = MonoSingleton<CameraController>.Instance.transform;
            om = MonoSingleton<OptionsManager>.Instance;
            player = MonoSingleton<NewMovement>.Instance;
            animator = GetComponent<Animator>();
            //UltraFunGuns.Log.LogWarning("Added wep icon");
            weaponIcon = gameObject.AddComponent<WeaponIcon>();
            weaponIdentifier = gameObject.AddComponent<WeaponIdentifier>();


            foreach (Transform transf in gameObject.GetComponentsInChildren<Transform>(true))
            {
                if (transf.name == "firePoint")
                {
                    firePoint = transf;
                    break;
                }
            }

            if(firePoint == null)
            {
                firePoint = mainCam;
                UltraFunGuns.Log.LogWarning("FirePoint setup incorrectly for weapon: " + gameObject.name);
            }

            if (weaponIcon.weaponDescriptor == null)
            {
                weaponIcon.weaponDescriptor = ScriptableObject.CreateInstance<WeaponDescriptor>();
                HydraLoader.dataRegistry.TryGetValue($"{weaponInfo.WeaponKey}_weaponIcon", out UnityEngine.Object weapon_weaponIcon);
                weaponIcon.weaponDescriptor.icon = (Sprite)weapon_weaponIcon;

                HydraLoader.dataRegistry.TryGetValue($"{weaponInfo.WeaponKey}_glowIcon", out UnityEngine.Object weapon_glowIcon);
                weaponIcon.weaponDescriptor.glowIcon = (Sprite)weapon_glowIcon;

                weaponIcon.weaponDescriptor.variationColor = (WeaponVariant)weaponInfo.IconColor;

                if (weaponIcon.weaponDescriptor.icon == null)
                {
                    HydraLoader.dataRegistry.TryGetValue("debug_weaponIcon", out UnityEngine.Object debug_weaponIcon);
                    weaponIcon.weaponDescriptor.icon = (Sprite)debug_weaponIcon;
                }

                if (weaponIcon.weaponDescriptor.glowIcon == null)
                {
                    HydraLoader.dataRegistry.TryGetValue("debug_glowIcon", out UnityEngine.Object debug_glowIcon);
                    weaponIcon.weaponDescriptor.glowIcon = (Sprite)debug_glowIcon;
                }
            }

            OnAwakeFinished();
        }

        public virtual void OnAwakeFinished() {}

        private void Update()
        {
            GetInput();
            DoAnimations();
        }

        //Example input function call this in update.
        public virtual void GetInput()
        {
            if (MonoSingleton<InputManager>.Instance.InputSource.Fire1.WasPerformedThisFrame && !om.paused)
            {
                FirePrimary();
            }

            if (MonoSingleton<InputManager>.Instance.InputSource.Fire2.WasPerformedThisFrame && !om.paused)
            {
                FireSecondary();
            }

            if(UFGInput.SecretButton.WasPeformed())
            {
                DoSecret();
            }

            if(Input.GetKeyDown(KeyCode.Equals))
            {
                if(UltraFunGuns.DebugMode)
                {
                    DebugAction();
                }
            }
        }

        public virtual void FirePrimary()
        {
            UltraFunGuns.Log.Log($"{gameObject.name} Fired Primary! (not implemented)");
        }

        public virtual void FireSecondary()
        {
            UltraFunGuns.Log.Log($"{gameObject.name} Fired Secondary! (not implemented)");
        }
        
        public virtual void DoSecret()
        {
            UltraFunGuns.Log.Log($"{gameObject.name} Used Secret! (not implemented)");
        }

        public virtual void DebugAction()
        {
            UltraFunGuns.Log.Log($"{gameObject.name} Used Debug Action! (not implemented)");
        }

        public UFGWeapon GetWeaponInfo()
        {
            if(weaponInfo == null)
            {
                weaponInfo = WeaponManager.GetWeaponInfo(this.GetType());
            }
            return weaponInfo;
        }

        public virtual string GetDebuggingText()
        {
            return
                $"WEAPONKEY: {weaponInfo.WeaponKey}\n" +
                $"DISPLAYNAME: {weaponInfo.DisplayName}\n";
        }

        void OnGUI()
        {
            if (!UltraFunGuns.DebugMode)
                return;
            //GUI.skin.label.fontSize = 20;
            //GUI.skin.label.font = Prefabs.VCR_Font.Asset;
            GUI.skin.box.fontSize = 35;
            GUI.skin.box.font = Prefabs.VCR_Font.Asset;
            GUI.skin.box.normal.textColor = Color.white;
            GUI.skin.box.alignment = TextAnchor.UpperLeft;
            GUILayout.Box(GetDebuggingText().TrimEnd('\n','\r'));
            //GUILayout.Label(GetDebuggingText());
        }

        //Adds sound effect to dict
        private bool AddSFX(string clipName)
        { 
            Transform audioSourceObject = transform.Find($"Audios/{clipName}");

            if(audioSourceObject == null)
            {
                UltraFunGuns.Log.LogError(string.Format("{0} is missing AudioSource Object: {1}", gameObject.name, clipName));
                return false;
            }else
            {
                if(!audioSourceObject.TryGetComponent<AudioSource>(out AudioSource newAudioSrc))
                {
                    UltraFunGuns.Log.LogError(string.Format("{0} is missing AudioSource Component: {1}", gameObject.name, clipName));
                    return false;
                }

                if(soundEffects.ContainsKey(name))
                {
                    UltraFunGuns.Log.LogWarning(string.Format("{0} attempted to add AudioSource: {1}, more than once.", gameObject.name, clipName));
                    return false;
                }

                soundEffects.Add(clipName, newAudioSrc);
                return true;
            }
        }

        protected void AddSFX(params string[] names)
        {
            int counter = 0;

            for (int i = 0; i < names.Length; i++)
            {
                if(AddSFX(names[i]))
                {
                    ++counter;
                }
            }

            UltraFunGuns.Log.Log(string.Format("{0}: {1}/{2} SFX Added.", gameObject.name, counter, names.Length));
        }

        protected void PlaySFX(string name, float minPitch = 1.0f, float maxPitch = 1.0f)
        {
            if(!soundEffects.ContainsKey(name))
            {
                UltraFunGuns.Log.LogError(string.Format("{0}: sound effect: {1} not present in dictionary.", gameObject.name, name));
                return;
            }

            if(minPitch != 1.0f || maxPitch != 1.0f)
            {
                soundEffects[name].pitch = UnityEngine.Random.Range(minPitch, maxPitch);
            }

            soundEffects[name].Play();
        }

        public class ActionCooldown
        {
            public float TimeToFire;
            public float FireDelay;
            public bool NoCooldown;
            public bool AffectedByNoCooldownCheat;

            public ActionCooldown(float delay = 1f, bool affectedByNoCooldownCheat = false)
            {
                TimeToFire = 0.0f;
                this.NoCooldown = (delay <= 0.0f);
                this.FireDelay = delay;
                this.AffectedByNoCooldownCheat = affectedByNoCooldownCheat;
            }            

            public void AddCooldown()
            {
                TimeToFire = FireDelay + Time.time;
            }

            public void AddCooldown(float delayInSeconds)
            {
                TimeToFire = delayInSeconds + Time.time;
            }

            public bool CanFire()
            {
                if(TimeToFire < Time.time || NoCooldown || (ULTRAKILL.Cheats.NoWeaponCooldown.NoCooldown && AffectedByNoCooldownCheat))
                {
                    return true;
                }
                return false;
            }

            public override string ToString()
            {
                return Mathf.Max(0,TimeToFire - Time.time).ToString("0.000");
            }
        }

    }
}
