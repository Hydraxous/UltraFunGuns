﻿using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

namespace UltraFunGuns
{

    //at this point, literally just copy the code from grabby gun

    /*Gun that fires a "Sonic Wave" in a cone in front of it. Can be charged indefinitely.
     * Charging scales with size of sonic wave, knockback velocity, and player knockback velocity.
     * 
     * It's a vine boom gun :)
     * 
     * TODO:
     * - Fix charging animations jittering
     * - Fix equip animation
     * - Affect non-kinematic rigidbodies
     * 
     * BUGS: 
     * Enemy knockback is completely broken and needs to be reworked. : See enemy navigation disabling and rocket launcher code for in-game knockback mechanics.
     * The nature of being able to charge indefinitely causes many issues.
     * Fix algorithm for checking if enemy is behind player or in front of player, currently the hitbox goes WAY too far behind the player.
     */
    [UFGWeapon("SonicReverberator", "Sonic Reverberator", 0, true, WeaponIconColor.Blue)]
    public class SonicReverberator : UltraFunGunBase
    {
        [UFGAsset("SonicReverberationExplosion")] public static GameObject ReverbProjectile { get; private set; }

        [UFGAsset("vB_standard")] public static AudioClip vineBoom_Standard { get; private set; }
        [UFGAsset("vB_loud")] public static AudioClip vineBoom_Loud { get; private set; }
        [UFGAsset("vB_loud")] public static AudioClip vineBoom_Loudest { get; private set; }

        private AudioSource chargeIncrease, chargeDecrease, chargeFinal;

        private List<float> chargeMilestones = new List<float> { 2.0f, 5.0f, 10.0f, 20.0f, 60.0f };

        public bool skipConeCheck = true, enablePlayerKnockback = true;

        public float baseGyroRotationSpeed = 0.01f;
        public float gyroRotationSpeedModifier = 0.1f;

        public float chargeLevel = 0.0f;
        public float chargeSpeedMultiplier = 2.8f;
        public float chargeDecayMultiplier = 0.25f;

        private float maxTargetAngle = 80.0f;
        private float minTargetAngle = 18.0f;

        private float maxRange = 2000.0f;

        private int lastChargeState = 0;

        public Vector3 playerKnockbackVector = new Vector3(0,2.0f,8.0f);
        private float playerKnockbackMultiplier = 0.8f;
        private float playerKnockbackVerticalMultiplier = 1.2f;
        private float playerKnockbackJumpMultiplier = 1.5f;
        private float playerKnockbackMaxRange = 5000.0f;

        public bool charging = false;

        private VisualCounterAnimator pistons;

        private Animator moyaiAnimator;

        private float cooldownRate = 0.17f;
        private float minimumCooldown = 0.75f;
        private float maximumCooldown = 600.0f;
        private float lastKnownCooldownTime = 0.0f;

        private GyroRotator[] rotators;

        public override void OnAwakeFinished()
        {
            pistons = transform.Find("viewModelWrapper/MoyaiGun/Animated/PistonBase").gameObject.AddComponent<VisualCounterAnimator>();
            pistons.Initialize(4,"PistonHolder{0}/Pusher", true);
            chargeIncrease = transform.Find("viewModelWrapper/Audios/ChargeIncrease").GetComponent<AudioSource>();
            chargeDecrease = transform.Find("viewModelWrapper/Audios/ChargeDecrease").GetComponent<AudioSource>();
            chargeFinal = transform.Find("viewModelWrapper/Audios/ChargeFinal").GetComponent<AudioSource>();
            rotators = GetComponentsInChildren<GyroRotator>();
            moyaiAnimator = transform.Find("viewModelWrapper/MoyaiGun/Animated/OuterGyroBearing/InnerGyroBearing/OuterGyro/MiddleGyro/InnerGyro/Moyai").GetComponent<Animator>();
        }

        private void SetGyroSpeed(float newSpeed)
        {
            for (int i = 0; i < rotators.Length; i++)
            {
                if (rotators[i] == null)
                    continue;
                rotators[i].Speed = newSpeed;
            }
        }

        private void SetGyroEnable(bool newState)
        {
            for (int i = 0; i < rotators.Length; i++)
            {
                if (rotators[i] == null)
                    continue;
                rotators[i].Spin = newState;
            }
        }

        private void AddGyroSpin(float velocity)
        {
            for (int i = 0; i < rotators.Length; i++)
            {
                rotators[i]?.AddAngularVelocity(velocity);
            }
        }

        public override Dictionary<string, ActionCooldown> SetActionCooldowns()
        {
            Dictionary<string, ActionCooldown> cooldowns = new Dictionary<string, ActionCooldown>();
            cooldowns.Add("fire", new ActionCooldown(0.75f, true));
            return cooldowns;
        }

        public override void GetInput()
        {
            if (MonoSingleton<InputManager>.Instance.InputSource.Fire1.IsPressed && actionCooldowns["fire"].CanFire() && !om.paused)
            {
                charging = true;
                chargeLevel += Time.deltaTime * chargeSpeedMultiplier;
                CameraController.Instance.CameraShake((chargeLevel > chargeMilestones[0])? (chargeLevel > chargeMilestones[chargeMilestones.Count-1])? 0: chargeLevel*0.01f:0);
            }
            else
            {
                charging = false;
                chargeLevel = Mathf.Clamp((chargeLevel - (Time.deltaTime * chargeDecayMultiplier)), 0.0f, Mathf.Infinity);
            }

            if (MonoSingleton<InputManager>.Instance.InputSource.Fire2.WasPerformedThisFrame && actionCooldowns["fire"].CanFire() && !om.paused && chargeLevel >= 2.0f)
            {
                Fire();
            }

            if(WeaponManager.SecretButton.WasPerformedThisFrame)
            {
                SecretAnimation();
            }
        }

        private void SecretAnimation()
        {
            //animator.Play("Inspect", 0, 0);
        }

        protected override void DoAnimations()
        {
            SetGyroSpeed(baseGyroRotationSpeed * (chargeLevel * gyroRotationSpeedModifier));
            SetGyroEnable(charging);
            animator.SetBool("CanShoot", actionCooldowns["fire"].CanFire());
            animator.SetBool("Charging", charging);
            
            
            int chargeState = GetChargeState(chargeLevel);
            if (chargeState > lastChargeState)
            {
                if(chargeState == chargeMilestones.Count+1)
                {
                    chargeFinal.Play();
                }
                else
                {
                    chargeIncrease.Play();
                }
                
            }
            else if (chargeState < lastChargeState)
            {
                chargeDecrease.Play();
            }

            lastChargeState = chargeState;

            animator.SetFloat(("ChargeLevel"), Mathf.Clamp((chargeLevel / 5.0f), 1f, 6f)); //clamps multiplier speed to prevent over animation
            animator.SetInteger("ChargeState", chargeState);
            pistons.DisplayCount = chargeState;
            moyaiAnimator.SetFloat("ChargeLevel", chargeLevel + 0.5f);
        }

        private float CalcDistance(float scalar)
        {
            //y = (x+((x*(x*0.45))/5))
            float distance = scalar * 0.45f;
            distance *= scalar;
            distance /= scalar;
            distance += scalar;
            distance *= 2.0f;
            return Mathf.Min(distance, maxRange);
        }
        
        private float CalcRadius(float scalar)
        {
            //y = (x+((x*(x*0.45))/5))*0.15
            float radius = scalar * 0.45f;
            radius *= scalar;
            radius /= scalar;
            radius += scalar;
            radius *= 0.8f;
            return Mathf.Min(radius, maxRange);
        }

        private void Fire()
        {
            float thickness = CalcRadius(chargeLevel);
            float maxRange = CalcDistance(chargeLevel);

            VineBoom(); // does the thing

            //float currentAngle = Mathf.Clamp(chargeLevel, minTargetAngle, maxTargetAngle);
            int chargeState = GetChargeState();

            MonoSingleton<TimeController>.Instance.HitStop(0.25f * (chargeState - 1));
            MonoSingleton<CameraController>.Instance.CameraShake(1.5f * (chargeState - 1));

            Visualizer.DrawSphereCast(mainCam.position, thickness, mainCam.forward, maxRange, 3.0f);

            if (HydraUtils.SphereCastAllMacro(mainCam.position, thickness, mainCam.forward, maxRange, out RaycastHit[] hits))
            {
                foreach (RaycastHit hit in hits)
                {
                    ProcessHit(hit);
                }
            }

            KnockbackPlayer();

            animator.Play("SonicReverberator_Shoot");
            pistons.DisplayCount = chargeState;
            AddGyroSpin(chargeLevel);

            actionCooldowns["fire"].AddCooldown(Mathf.Clamp((chargeLevel * (cooldownRate + chargeLevel * (cooldownRate - (cooldownRate / 1.03f)))), minimumCooldown, maximumCooldown)); //Somewhat hyperbolic cooldown time based on charge level capped at 10 mins maximum.
            chargeLevel = 0;
            lastChargeState = 0;
            charging = false;
        }

        private void ProcessHit(RaycastHit hit)
        {
            if (!HydraUtils.LineOfSightCheck(mainCam.position, hit.point) )
            {
                //return;
            }

            Vector3 direction = hit.point - mainCam.position;

            if ((Vector3.Dot((hit.collider.transform.position - mainCam.position), mainCam.forward) < 0) && !skipConeCheck)
            {
                return;
            }

            if (hit.collider.IsColliderEnemy(out EnemyIdentifier eid))
            {
                EnemyOverride enemyOverride = eid.Override();
                eid.DeliverDamage(eid.gameObject, direction.normalized * 2000.0f, hit.point, chargeLevel*0.1f, true, 0.0f, gameObject);
                enemyOverride.Knockback(direction.normalized*chargeLevel * 20.0f);
            }else if(hit.collider.attachedRigidbody != null)
            {
                hit.collider.attachedRigidbody.velocity = direction.normalized * chargeLevel;
            }

            Vector3 reflect = Vector3.Reflect(mainCam.position - hit.collider.bounds.center, mainCam.forward);

            ParryTool.TryParryProjectile(hit.collider, reflect, out Projectile _);
            ParryTool.TryParryCannonball(hit.collider, reflect, out Cannonball __);

            if (hit.collider.gameObject.TryGetComponent<IUFGInteractionReceiver>(out IUFGInteractionReceiver uFGInteractionReceiver))
            {
                UFGInteractionEventData eventData = new UFGInteractionEventData()
                {
                    invokeType = typeof(SonicReverberator),
                    interactorPosition = mainCam.position,
                    direction = direction,
                    tags = new string[] { "shot", "sonic", "shatter" },
                    power = chargeLevel,
                };

                uFGInteractionReceiver.Interact(eventData);
            }

        }

        public int GetChargeState(float charge) //gets charge state from provided charge level.
        {
            for (int i = 0; i < chargeMilestones.Count; i++)
            {
                if (charge < chargeMilestones[i])
                {
                    return i;
                }
            }
            return chargeMilestones.Count + 1;
        }

        public int GetChargeState() //Gets charge state from internal chargelevel
        {
            return GetChargeState(chargeLevel);
        }

        private void OnDisable()
        {
            lastKnownCooldownTime = Time.time;
        }

        //TODO FIX THIS IT FORCES COOLDOWN VERY DUMB!!
        private void OnEnable()
        {
            if ((actionCooldowns["fire"].TimeToFire - Time.time) > 0.0f)
            {
                actionCooldowns["fire"].TimeToFire += Mathf.Clamp(lastKnownCooldownTime - Time.time, 0.0f, maximumCooldown);
            }
        }

        private void KnockbackPlayer()
        {
            if (enablePlayerKnockback) 
            {
                int chargeState = GetChargeState();
                float jumpModifier = 1.0f;
                float upwardsModifier = playerKnockbackVerticalMultiplier;

                if (player.jumping) //small boost if shooting right when jumping, like a rocket jump
                {
                    jumpModifier = playerKnockbackJumpMultiplier;
                }

                if (!player.gc.onGround) //intuitive knockback when in the air.
                {
                    upwardsModifier = 0.0f;
                }

                Vector3 localDirection = new Vector3(0, 0, 0);
                localDirection.x = playerKnockbackVector.x * (chargeState - 1) * (chargeLevel * playerKnockbackMultiplier);
                localDirection.y =  playerKnockbackVector.y * (chargeState - 1) * (chargeLevel * upwardsModifier);
                localDirection.z = jumpModifier * (-playerKnockbackVector.z * (chargeState - Mathf.Clamp01(1*upwardsModifier)) * (chargeLevel * playerKnockbackMultiplier));

                Vector3 forceVector = Vector3.ClampMagnitude(mainCam.transform.TransformDirection(localDirection), playerKnockbackMaxRange);

                player.rb.velocity += forceVector;
            }
        }

        //This is yucky but its fine. No, fix it.
        private void VineBoom()
        {
            GameObject vineBoomNoise = GameObject.Instantiate<GameObject>(new GameObject(),this.transform);
            Destroy(vineBoomNoise, 3.0f);
            AudioSource vineBoomAudioSource = vineBoomNoise.AddComponent<AudioSource>();
            vineBoomNoise.AddComponent<DestroyAfterTime>();
            vineBoomAudioSource.playOnAwake = false;
            vineBoomAudioSource.loop = false;
            switch (GetChargeState())
            {
                case 1:
                    vineBoomAudioSource.clip = vineBoom_Standard;
                    vineBoomAudioSource.volume = 0.75f;
                    break;
                case 2:
                    vineBoomAudioSource.clip = vineBoom_Standard;
                    vineBoomAudioSource.volume = 1.0f;
                    break;
                case 3:
                    vineBoomAudioSource.clip = vineBoom_Loud;
                    vineBoomAudioSource.volume = 0.8f;
                    break;
                case 4:
                    vineBoomAudioSource.clip = vineBoom_Loud;
                    vineBoomAudioSource.volume = 1.0f;
                    break;
                case 5:
                    vineBoomAudioSource.clip = vineBoom_Loudest;
                    vineBoomAudioSource.volume = 1.2f;
                    break;
                case 6:
                    vineBoomAudioSource.clip = vineBoom_Loudest;
                    vineBoomAudioSource.volume = 2.0f;
                    break;
                default:
                    vineBoomAudioSource.clip = vineBoom_Standard;
                    vineBoomAudioSource.volume = 0.75f;
                    break;
            }
            vineBoomAudioSource.pitch = 1.0f;
            vineBoomAudioSource.spatialBlend = 1.0f;
            vineBoomAudioSource.maxDistance = 100000.0f;
            vineBoomAudioSource.dopplerLevel = 0.05f;
            vineBoomAudioSource.Play();
        }

        public override string GetDebuggingText()
        {
            string debug = base.GetDebuggingText();
            debug += $"CHARGE: {chargeLevel}\n";
            debug += $"MILESTONE: {GetChargeState(chargeLevel)}\n";
            debug += $"CHARGING: {charging}\n";
            return debug;
        }
        
    }
}