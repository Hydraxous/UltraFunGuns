using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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
    [FunGun("SonicReverberator", "Sonic Reverberator", 0, true, WeaponIconColor.Blue)]
    public class SonicReverberator : UltraFunGunBase
    {
        public GameObject bang;

        public AudioClip vB_standard, vB_loud, vB_loudest;

        private AudioSource chargeIncrease, chargeDecrease, chargeFinal;

        private List<float> chargeMilestones = new List<float> { 2.0f, 5.0f, 10.0f, 20.0f, 60.0f };

        public bool skipConeCheck = true, enablePlayerKnockback = true;

        public float baseGyroRotationSpeed = 0.01f;
        public float currentGyroRotationSpeed = 0.01f;
        public float gyroRotationSpeedModifier = 0.1f;

        public float chargeLevel = 0.0f;
        public float chargeSpeedMultiplier = 2.8f;
        public float chargeDecayMultiplier = 0.25f;

        //TODO try to remove this.
        private float blastForceMultiplier = 100.0f;
        private float blastForceUpwardsMultiplier = 30.0f;
        private float hitBoxRadiusMultiplier = 0.75f;
        private float blastOriginZOffset = 0.49f;

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

        public override void OnAwakeFinished()
        {
            pistons = transform.Find("viewModelWrapper/MoyaiGun/Animated/PistonBase").gameObject.AddComponent<VisualCounterAnimator>();
            pistons.Initialize(4,"PistonHolder{0}/Pusher", true);
            chargeIncrease = transform.Find("viewModelWrapper/Audios/ChargeIncrease").GetComponent<AudioSource>();
            chargeDecrease = transform.Find("viewModelWrapper/Audios/ChargeDecrease").GetComponent<AudioSource>();
            chargeFinal = transform.Find("viewModelWrapper/Audios/ChargeFinal").GetComponent<AudioSource>();
        }


        private void Start()
        {
            LoadData();
            HelpChildren();
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
        }


        private void HelpChildren()
        {
            transform.Find("viewModelWrapper/MoyaiGun/Animated/OuterGyroBearing/InnerGyroBearing").gameObject.AddComponent<GyroRotator>();
            transform.Find("viewModelWrapper/MoyaiGun/Animated/OuterGyroBearing/InnerGyroBearing/OuterGyro/MiddleGyro").gameObject.AddComponent<GyroRotator>();
            transform.Find("viewModelWrapper/MoyaiGun/Animated/OuterGyroBearing/InnerGyroBearing/OuterGyro/MiddleGyro/InnerGyro").gameObject.AddComponent<GyroRotator>();
            transform.Find("viewModelWrapper/MoyaiGun/Animated/OuterGyroBearing/InnerGyroBearing/OuterGyro/MiddleGyro/InnerGyro/Moyai").gameObject.AddComponent<GyroRotator>();    
            moyaiAnimator = transform.Find("viewModelWrapper/MoyaiGun/Animated/OuterGyroBearing/InnerGyroBearing/OuterGyro/MiddleGyro/InnerGyro/Moyai").GetComponent<Animator>();
        }


        private void LoadData()
        {

            HydraLoader.dataRegistry.TryGetValue("vB_standard", out UnityEngine.Object vB_standard_obj);
            vB_standard = (AudioClip)vB_standard_obj;

            HydraLoader.dataRegistry.TryGetValue("vB_loud", out UnityEngine.Object vB_loud_obj);
            vB_loud = (AudioClip)vB_loud_obj;

            HydraLoader.dataRegistry.TryGetValue("vB_loudest", out UnityEngine.Object vB_loudest_obj);
            vB_loudest = (AudioClip)vB_loudest_obj;

            HydraLoader.prefabRegistry.TryGetValue("SonicReverberationExplosion", out bang);

        }

        protected override void DoAnimations()
        {
            currentGyroRotationSpeed = baseGyroRotationSpeed * (chargeLevel*gyroRotationSpeedModifier);
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

        //RULES FOR HITREG
        /*
         * 1. Enemy must be within range.
         * 1b. Enemy must be in line of sight.
         * 2. Enemy must not be dead.
         * 3. Enemy must be within target angle.
         * - Zombie tags will get knockback
         * - Drones will start to crash
         * - Stone creatures will take damage
         * - Fullcharge will instakill everything.
         * - 
         * 4. The closer the enemy is the stronger the force applied to them will be.
         * 5. Any rigidbodies which aren't enemies should get knocked back if they aren't kinematic
         * 6. Any Dodgeballs should be excited with the corresponding charge state
         * 7. Any projectiles should be reflected using the player look direction as a normal
         */
        //Gets all enemies, dodgeballs, rigidbodies, etc. acts on them accordingly provided they are within range, los, and determined vision angle.
        private void Fire()
        {
            int chargeState = GetChargeState();
            
            animator.Play("SonicReverberator_Shoot");
            pistons.DisplayCount = chargeState;
            SonicReverberatorExplosion BOOM = GameObject.Instantiate<GameObject>(bang, firePoint.position, Quaternion.identity).GetComponent<SonicReverberatorExplosion>();
            BOOM.transform.forward = firePoint.TransformDirection(new Vector3(0,0,1));
            BOOM.power = chargeLevel;
            BOOM.powerState = chargeState;
     
            VineBoom(); // does the thing

            float currentAngle = Mathf.Clamp(chargeLevel, minTargetAngle, maxTargetAngle);

            List<TargetObject> targets = new List<TargetObject>();

            GameObject[] enemyObjectsActive = GameObject.FindGameObjectsWithTag("Enemy");
            targets = HydraUtils.GetTargetsFromGameObjects(enemyObjectsActive);

            //MonoSingleton<TimeController>.Instance.HitStop(0.25f * (chargeState - 1));
            //MonoSingleton<CameraController>.Instance.CameraShake(1.5f * (chargeState - 1));
            //TODO restore old code. operate fine
            for(int i = 0; i < targets.Count; i++)
            {
                EffectTarget(targets[i], currentAngle);
            }

            KnockbackPlayer();

            actionCooldowns["fire"].AddCooldown(Mathf.Clamp((chargeLevel * (cooldownRate + chargeLevel * (cooldownRate - (cooldownRate / 1.03f)))), minimumCooldown, maximumCooldown)); //Somewhat hyperbolic cooldown time based on charge level capped at 10 mins maximum.
            chargeLevel = 0;
            lastChargeState = 0;
            charging = false;
        }

        private bool EffectTarget(TargetObject target, float angle)
        {
            Ray targetRay = new Ray();
            targetRay.origin = mainCam.transform.position;
            targetRay.direction = target.gameObject.transform.position - mainCam.transform.position;

            if (!HydraUtils.LineOfSightCheck(targetRay.origin, target.gameObject.transform.position))
            {
                return false;
            }

            if(!HydraUtils.ConeCheck(mainCam.TransformDirection(0,0,1).normalized, targetRay.direction, angle))
            {
                return false;
            }
            HydraLogger.Log("UFG: SONIC GUN: Effect enemy called on " + target.gameObject.name + "|T: " + target.targetType.ToString());
            switch (target.targetType)
            {
                case TargetObject.TargetType.Dodgeball:
                    target.targetPoint.GetComponent<ThrownDodgeball>().ExciteBall(GetChargeState());
                    break;
                case TargetObject.TargetType.Zombie:
                    target.targetPoint.GetComponent<Zombie>().KnockBack(targetRay.direction*chargeLevel);
                    break;
                default:
                    break;
            }

            return false;
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

        //TODO FUCKING HELL FIX THIS AWFUL SHIT
        //TODO FIX THIS 
        //TODO STOP WRITING MORE TODO COMMENTS AND FUCKING DO IT TOFUCKINGDAY
        //I'm gonna keep writing TODO messages you cannot stop me, you are in the past, you have no control of my actions.
        //TODO fix this
        private void EffectEnemy(EnemyIdentifier enemy, Vector3 blastOrigin) //TODO optimize this. Update: F*** optimization, redo this garbage entirely.
        {
            try
            {
                if (!enemy.dead)
                { 
                EnemyType enemyType = enemy.enemyType;
                bool canKnockback = false;
                bool boss = enemy.gameObject.TryGetComponent<BossIdentifier>(out BossIdentifier b);
                switch (enemyType)
                {
                    case EnemyType.Drone:
                        canKnockback = true;
                        break;
                    case EnemyType.Virtue:
                        canKnockback = true;
                        break;
                    case EnemyType.Soldier:
                        canKnockback = true;
                        break;
                    case EnemyType.Stray:
                        canKnockback = true;
                        break;
                    case EnemyType.Stalker:
                        canKnockback = true;
                        break;
                    case EnemyType.Swordsmachine:
                        canKnockback = true;
                        break;
                    case EnemyType.Streetcleaner:
                        canKnockback = true;
                        break;
                    case EnemyType.V2:
                        canKnockback = true;
                        boss = true;
                        break;
                    case EnemyType.Filth:
                        canKnockback = true;
                        break;
                    case EnemyType.Gabriel:
                        canKnockback = true;
                        boss = true;
                        break;
                    case EnemyType.Turret:
                        if (!enemy.gameObject.GetComponent<Turret>().aiming)
                        {
                            canKnockback = true;
                        }
                        break;
                    case EnemyType.Schism:
                        canKnockback = true;
                        break;
                    case EnemyType.Minos:
                        boss = true;
                        break;
                    case EnemyType.MinosPrime:
                        boss = true;
                        break;
                    case EnemyType.V2Second:
                        boss = true;
                        break;
                    case EnemyType.Wicked:
                        boss = true;
                        break;
                    case EnemyType.Leviathan:
                        boss = true;
                        break;
                    case EnemyType.GabrielSecond:
                        boss = true;
                        break;
                    case EnemyType.HideousMass:
                        boss = true;
                        break;
                    case EnemyType.Ferryman:
                        boss = true;
                        break;
                    default:
                        canKnockback = false;
                        break;
                }

                int chargeState = GetChargeState();
                if (canKnockback)
                {
                    DoKnockback(enemy.gameObject, blastOrigin);
                    if (chargeState < chargeMilestones.Count && chargeState >= chargeMilestones.Count / 1.5f)
                    {
                        MonoSingleton<StyleHUD>.Instance.AddPoints(10, "hydraxous.ultrafunguns.vibecheck", this.gameObject, enemy, -1, "", "");
                    }
                }
                    if (GetChargeState() >= chargeMilestones.Count)
                    {
                        if (boss || enemy.TryGetComponent<BossIdentifier>(out BossIdentifier bossId))
                        {
                            if (enemy.TryGetComponent<MinosPrime>(out MinosPrime mp) || enemy.TryGetComponent<MinosBoss>(out MinosBoss minosBoss)) //hehe style :)
                            {
                                MonoSingleton<StyleHUD>.Instance.AddPoints(500, "hydraxous.ultrafunguns.minoskill", this.gameObject, enemy, -1, "", "");
                            }
                            else if (enemy.TryGetComponent<Gabriel>(out Gabriel gaybe) || enemy.TryGetComponent<GabrielSecond>(out GabrielSecond gaybe2))
                            {
                                MonoSingleton<StyleHUD>.Instance.AddPoints(500, "hydraxous.ultrafunguns.gabrielkill", this.gameObject, enemy, -1, "", "");
                            }
                            else if (enemy.TryGetComponent<Wicked>(out Wicked wicked))
                            {
                                MonoSingleton<StyleHUD>.Instance.AddPoints(20000, "hydraxous.ultrafunguns.wickedkill", this.gameObject, enemy, -1, "", "");
                            }
                            else if (enemy.TryGetComponent<V2>(out V2 v2))
                            {
                                MonoSingleton<StyleHUD>.Instance.AddPoints(500, "hydraxous.ultrafunguns.v2kill", this.gameObject, enemy, -1, "", "");
                            }
                            else
                            {
                                MonoSingleton<StyleHUD>.Instance.AddPoints(500, "hydraxous.ultrafunguns.vaporized", this.gameObject, enemy, -1, "<b>BOSS </b>", "");
                            }
                            enemy.health = 0;
                        }
                        else
                        {
                            enemy.Explode();
                            MonoSingleton<StyleHUD>.Instance.AddPoints(100, "hydraxous.ultrafunguns.vaporized", this.gameObject, enemy, -1, "", "");
                        }
                    }
                }
            }catch(System.Exception e)
            {
                //TODO Remove this try catch and fix this properly you degenerate.
            }
        }

        //TODO eat shit and die
        private void DoKnockback(GameObject obj, Vector3 forceOrigin) //TODO Fix this algorithm so it actually works
        {
            int chargeState = GetChargeState();
            EnemyIdentifier enemy;
            float distanceFromBlast = Vector3.Distance(obj.transform.position, forceOrigin);
            float distanceMultiplier = 10.0f / distanceFromBlast;
            Vector3 forceVector = (obj.transform.position - forceOrigin).normalized * ((blastForceMultiplier * chargeState) + (chargeLevel * chargeState));
            Vector3 upVector = new Vector3(0, blastForceUpwardsMultiplier * chargeState, 0);
            forceVector *= distanceMultiplier;

            if (obj.TryGetComponent<EnemyIdentifier>(out enemy))
            {
                enemy.gameObject.GetComponent<NavMeshAgent>().enabled = false;
                Rigidbody body = enemy.gameObject.GetComponent<Rigidbody>();
                body.isKinematic = false;
                body.useGravity = false;
                body.velocity += forceVector;
                body.velocity += upVector;
                HydraLogger.Log(enemy.enemyType.ToString() + "  " + body.velocity);
                SplatOnImpact splatt = enemy.gameObject.AddComponent<SplatOnImpact>();
                //splatt.invincibilityTimer = splatTimer;
                //splatt.velocityToSplatThreshold = splatThreshold;
            }
        }

        private void KnockbackPlayer() //TODO IGBalancing PLEASE.... Update: no its fine. :^)
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

        //This is yucky but its fine.
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
                    vineBoomAudioSource.clip = vB_standard;
                    vineBoomAudioSource.volume = 0.75f;
                    break;
                case 2:
                    vineBoomAudioSource.clip = vB_standard;
                    vineBoomAudioSource.volume = 1.0f;
                    break;
                case 3:
                    vineBoomAudioSource.clip = vB_loud;
                    vineBoomAudioSource.volume = 0.8f;
                    break;
                case 4:
                    vineBoomAudioSource.clip = vB_loud;
                    vineBoomAudioSource.volume = 1.0f;
                    break;
                case 5:
                    vineBoomAudioSource.clip = vB_loudest;
                    vineBoomAudioSource.volume = 1.2f;
                    break;
                case 6:
                    vineBoomAudioSource.clip = vB_loudest;
                    vineBoomAudioSource.volume = 2.0f;
                    break;
                default:
                    vineBoomAudioSource.clip = vB_standard;
                    vineBoomAudioSource.volume = 0.75f;
                    break;
            }
            vineBoomAudioSource.pitch = 1.0f;
            vineBoomAudioSource.spatialBlend = 1.0f;
            vineBoomAudioSource.maxDistance = 100000.0f;
            vineBoomAudioSource.dopplerLevel = 0.05f;
            vineBoomAudioSource.Play();
        }
    }
}