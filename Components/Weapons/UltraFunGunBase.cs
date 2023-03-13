using System.Collections.Generic;
using UnityEngine;

namespace UltraFunGuns
{
    //Base class for UFG weapons.
    public abstract class UltraFunGunBase : MonoBehaviour
    {
        public Dictionary<string, ActionCooldown> actionCooldowns;
        public Dictionary<string, AudioSource> soundEffects = new Dictionary<string, AudioSource>();

        protected Transform mainCam, firePoint;
        protected OptionsManager om;
        protected NewMovement player;
        protected WeaponIcon weaponIcon;
        protected Animator animator;

        protected UFGWeapon weaponInfo;

        protected virtual void DoAnimations() { }

        private void Awake()
        {
            weaponInfo = Data.GetWeaponInfo(this.GetType());
            actionCooldowns = SetActionCooldowns();
            mainCam = MonoSingleton<CameraController>.Instance.transform;
            om = MonoSingleton<OptionsManager>.Instance;
            player = MonoSingleton<NewMovement>.Instance;
            animator = GetComponent<Animator>();
            weaponIcon = GetComponent<WeaponIcon>();
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
                HydraLogger.Log("FirePoint setup incorrectly for weapon: " + gameObject.name, DebugChannel.Error);
            }

            HydraLoader.dataRegistry.TryGetValue($"{weaponInfo.WeaponKey}_weaponIcon", out UnityEngine.Object weapon_weaponIcon);
            weaponIcon.weaponIcon = (Sprite) weapon_weaponIcon;

            HydraLoader.dataRegistry.TryGetValue($"{weaponInfo.WeaponKey}_glowIcon", out UnityEngine.Object weapon_glowIcon);
            weaponIcon.glowIcon = (Sprite) weapon_glowIcon;

            weaponIcon.variationColor = (int) weaponInfo.IconColor;
             
            if (weaponIcon.weaponIcon == null)
            {
                HydraLoader.dataRegistry.TryGetValue("debug_weaponIcon", out UnityEngine.Object debug_weaponIcon);
                weaponIcon.weaponIcon = (Sprite)debug_weaponIcon;
            }

            if(weaponIcon.glowIcon == null)
            {
                HydraLoader.dataRegistry.TryGetValue("debug_glowIcon", out UnityEngine.Object debug_glowIcon);
                weaponIcon.glowIcon = (Sprite)debug_glowIcon;
            }

            weaponIcon.UpdateIcon();

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
            if (MonoSingleton<InputManager>.Instance.InputSource.Fire1.WasPerformedThisFrame && actionCooldowns["primaryFire"].CanFire() && !om.paused)
            {
                actionCooldowns["primaryFire"].AddCooldown();
                FirePrimary();
            }

            if (MonoSingleton<InputManager>.Instance.InputSource.Fire2.WasPerformedThisFrame && actionCooldowns["secondaryFire"].CanFire() && !om.paused)
            {
                actionCooldowns["secondaryFire"].AddCooldown();
                FireSecondary();
            }

            if(WeaponManager.SecretButton.WasPerformedThisFrame)
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

        //Implement the cooldowns here.
        public virtual Dictionary<string, ActionCooldown> SetActionCooldowns()
        {
            Dictionary<string, ActionCooldown> cooldowns = new Dictionary<string, ActionCooldown>();
            cooldowns.Add("primaryFire", new ActionCooldown(1.0f, true));
            cooldowns.Add("secondaryFire", new ActionCooldown(1.0f, true));
            return cooldowns;
        }

        public virtual void FirePrimary()
        {
            HydraLogger.Log($"{gameObject.name} Fired Primary! (not implemented)");
        }

        public virtual void FireSecondary()
        {
            HydraLogger.Log($"{gameObject.name} Fired Secondary! (not implemented)");
        }
        
        public virtual void DoSecret()
        {
            HydraLogger.Log($"{gameObject.name} Used Secret! (not implemented)");
        }

        public virtual void DebugAction()
        {
            HydraLogger.Log($"{gameObject.name} Used Debug Action! (not implemented)");
        }

        //Adds sound effect to dict
        private bool AddSFX(string clipName)
        { 
            Transform audioSourceObject = transform.Find($"Audios/{clipName}");

            if(audioSourceObject == null)
            {
                HydraLogger.Log(string.Format("{0} is missing AudioSource Object: {1}", gameObject.name, clipName), DebugChannel.Error);
                return false;
            }else
            {
                if(!audioSourceObject.TryGetComponent<AudioSource>(out AudioSource newAudioSrc))
                {
                    HydraLogger.Log(string.Format("{0} is missing AudioSource Component: {1}", gameObject.name, clipName), DebugChannel.Error);
                    return false;
                }

                if(soundEffects.ContainsKey(name))
                {
                    HydraLogger.Log(string.Format("{0} attempted to add AudioSource: {1}, more than once.", gameObject.name, clipName), DebugChannel.Warning);
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

            HydraLogger.Log(string.Format("{0}: {1}/{2} SFX Added.", gameObject.name, counter, names.Length));
        }

        protected void PlaySFX(string name, float minPitch = 1.0f, float maxPitch = 1.0f)
        {
            if(!soundEffects.ContainsKey(name))
            {
                HydraLogger.Log(string.Format("{0}: sound effect: {1} not present in dictionary.", gameObject.name, name), DebugChannel.Error);
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
        }

    }
}
