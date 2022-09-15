using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UltraFunGuns
{
    public class SonicReverberator : MonoBehaviour
    {

        public GameObject bang; //set by data loader

        public AudioClip vB_standard, vB_loud, vB_loudest;
        //public float splatTimer = 0.05f;
        //public float splatThreshold = 20.0f;
        //public bool gravityEnemyModifier = true;

        private NewMovement player;
        private Transform mainCam;
        private Transform firePoint;

        private List<float> chargeMilestones = new List<float> { 2.0f, 5.0f, 10.0f, 20.0f, 60.0f };

        public bool skipConeCheck = true;
        public bool noCd = false;
        public bool enablePlayerKnockback = true;

        public float rotationSpeed = 0.01f;
        public float chargeLevel = 0.0f;
        public float chargeSpeedMultiplier = 2.8f;
        public float chargeDecayMultiplier = 0.25f;
        private float blastForceMultiplier = 100.0f;
        private float blastForceUpwardsMultiplier = 30.0f;
        private float hitBoxRadiusMultiplier = 0.75f;
        private float blastOriginZOffset = 0.49f;

        public Vector3 playerKnockbackVector = new Vector3(0,2.0f,8.0f);
        private float playerKnockbackMultiplier = 0.8f;
        private float playerKnockbackVerticalMultiplier = 1.2f;
        private float playerKnockbackJumpMultiplier = 1.5f;
        private float playerKnockbackMaxRange = 5000.0f;

        public bool charging = false;
        private bool canFire = true;

        private Animator capsuleAnimator;
        private Animator pistonAnimator;
        private Animator moyaiAnimator;
        private Animator gunAnimator;

        public float timeUntilFire = 0.0f;
        private float cooldownRate = 0.17f;
        private float minimumCooldown = 0.75f;
        private float maximumCooldown = 600.0f;
        private float lastKnownCooldown = 0.0f;

        private void Start()
        {
            HelpChildren();
            mainCam = MonoSingleton<CameraController>.Instance.transform;
            player = transform.GetComponentInParent<NewMovement>();
        }

        private void Update()
        {
            canFire = CanShoot();

            if (MonoSingleton<InputManager>.Instance.InputSource.Fire1.IsPressed && canFire)
            {
                charging = true;
                chargeLevel += Time.deltaTime*chargeSpeedMultiplier;
            }
            else
            {
                charging = false;
                chargeLevel = Mathf.Clamp((chargeLevel - (Time.deltaTime*chargeDecayMultiplier)), 0.0f, Mathf.Infinity);
            }

            if (MonoSingleton<InputManager>.Instance.InputSource.Fire2.WasPerformedThisFrame && canFire)
            {
                if (chargeLevel >= 2.0f && canFire)
                {
                    Fire();
                }
            }
            DoAnimations();
        }


        private void HelpChildren()
        {
            transform.Find("viewModelWrapper/MoyaiGun/OuterGyroBearing/InnerGyroBearing").gameObject.AddComponent<GyroRotator>();
            transform.Find("viewModelWrapper/MoyaiGun/OuterGyroBearing/InnerGyroBearing/OuterGyro/MiddleGyro").gameObject.AddComponent<GyroRotator>();
            transform.Find("viewModelWrapper/MoyaiGun/OuterGyroBearing/InnerGyroBearing/OuterGyro/MiddleGyro/InnerGyro").gameObject.AddComponent<GyroRotator>();
            transform.Find("viewModelWrapper/MoyaiGun/OuterGyroBearing/InnerGyroBearing/OuterGyro/MiddleGyro/InnerGyro/Moyai").gameObject.AddComponent<GyroRotator>();
            firePoint = transform.Find("viewModelWrapper/FirePoint");

            capsuleAnimator = transform.Find("viewModelWrapper/MoyaiGun/Capsule").GetComponent<Animator>();
            moyaiAnimator = transform.Find("viewModelWrapper/MoyaiGun/OuterGyroBearing/InnerGyroBearing/OuterGyro/MiddleGyro/InnerGyro/Moyai").GetComponent<Animator>();
            pistonAnimator = transform.Find("viewModelWrapper/MoyaiGun/PistonBase").GetComponent<Animator>();
            gunAnimator = transform.Find("viewModelWrapper/MoyaiGun").GetComponent<Animator>();
            

        }

        private bool CanShoot()
        {
            if (timeUntilFire <= 0.0f || noCd)
            {
                return true;
            }else
            {
                timeUntilFire -= Time.deltaTime;
                return false;
            }
        }

        private void DoAnimations()
        {
            gunAnimator.SetBool("CanShoot", canFire);
            capsuleAnimator.SetBool("Charging", charging);
            gunAnimator.SetBool("Charging", charging);
            
            int chargeState = GetChargeState(chargeLevel);
            gunAnimator.SetFloat(("ChargeLevel"), Mathf.Clamp((chargeLevel / 5.0f), 1f, 6f)); //clamps multiplier speed to prevent over animation
            gunAnimator.SetInteger("ChargeState", chargeState);
            pistonAnimator.SetInteger("ChargeLevel", chargeState);
            capsuleAnimator.SetInteger("ChargeLevel", chargeState);
            moyaiAnimator.SetFloat("ChargeLevel", chargeLevel + 0.5f);
        }


        //Gets point close to camera and a point far away that scales on charge level, then it casts a capsule and knocks back or destroys everything in the radius.
        private void Fire()
        {
            int chargeState = GetChargeState();

            gunAnimator.SetInteger("ChargeState", chargeState);
            gunAnimator.SetTrigger("Shoot");
            capsuleAnimator.SetTrigger("Shoot");
            moyaiAnimator.SetTrigger("Shoot");
            pistonAnimator.SetTrigger("Shoot");
            
            SonicReverberatorExplosion BOOM = GameObject.Instantiate<GameObject>(bang, firePoint.position, Quaternion.identity).GetComponent<SonicReverberatorExplosion>();
            BOOM.transform.forward = firePoint.TransformDirection(new Vector3(0,0,1));
            BOOM.power = chargeLevel;
            BOOM.powerState = chargeState;

            Vector3 blastOrigin = mainCam.transform.TransformPoint(new Vector3(0, 0, blastOriginZOffset + (chargeLevel * 0.1f))); //Origin of the blast basically next to the camera
            Vector3 blastEye = mainCam.transform.TransformPoint(new Vector3(0,0, (chargeState*blastOriginZOffset) + chargeLevel)); //End of the blast zone scales with charge
            Vector3 visionVector = mainCam.transform.TransformPoint(new Vector3(0,0,blastOriginZOffset)); //Player Vision vector based on camera
     
            VineBoom(); // :)
            //TODO hitstop here
            //TODO camera shake here

            RaycastHit[] hits = Physics.CapsuleCastAll(blastOrigin, blastEye, 6.0f + (chargeLevel * hitBoxRadiusMultiplier),visionVector);
            foreach (RaycastHit hit in hits)
            {
                if (Vector3.Dot((hit.collider.transform.position - visionVector).normalized, visionVector) > 0 || skipConeCheck) //Checks if target is in front of player.
                {
                    if (hit.collider.TryGetComponent<EnemyIdentifier>(out EnemyIdentifier enemy))
                    {
                        EffectEnemy(enemy, blastOrigin);
                    } else if (hit.collider.TryGetComponent<Glass>(out Glass glass))
                    {
                        glass.Shatter();
                    }
                    else if (hit.collider.TryGetComponent<Breakable>(out Breakable breakable))
                    {
                        breakable.Break();
                    }
                    /*OLD IMPLEMENTATION.
                    Component[] components = hit.collider.gameObject.GetComponents<Component>();
                    for (int i = 0; i < components.Length; i++)
                    {
                        switch (components[i])
                        {
                            case EnemyIdentifier enemy:
                                EffectEnemy(enemy, blastOrigin);
                                break;
                            case Glass glass:
                                glass.Shatter();
                                break;
                            case Breakable breakable:
                                breakable.Break();
                                break;
                        }
                    }
                    */
                }
            }

            KnockbackPlayer();

            timeUntilFire = Mathf.Clamp((chargeLevel * (cooldownRate + chargeLevel * (cooldownRate - (cooldownRate / 1.03f)))), minimumCooldown, maximumCooldown); //Somewhat hyperbolic cooldown time based on charge level
            chargeLevel = 0;
            charging = false;
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
            lastKnownCooldown = Time.time;
        }


        private void OnEnable()
        {
            if (timeUntilFire > 0.0f)
            {
                timeUntilFire = Mathf.Clamp(timeUntilFire+(lastKnownCooldown - Time.time), 0.0f, maximumCooldown);
            }
        }


        private void EffectEnemy(EnemyIdentifier enemy, Vector3 blastOrigin)
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
                    if(!enemy.gameObject.GetComponent<Turret>().aiming)
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
                default:
                    canKnockback = false;
                    break;
            }

            if (canKnockback)
            {
                DoKnockback(enemy.gameObject, blastOrigin);
                if(GetChargeState() < chargeMilestones.Count)
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

        private void DoKnockback(GameObject obj, Vector3 forceOrigin) //TODO IGBalancing
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

                Rigidbody body = enemy.gameObject.GetComponent<Rigidbody>();
                body.isKinematic = false;
                body.useGravity = false;
                body.velocity += forceVector;
                body.velocity += upVector;
                Debug.Log(enemy.enemyType.ToString() + "  " + body.velocity);
                SplatOnImpact splatt = enemy.gameObject.AddComponent<SplatOnImpact>();
                //splatt.invincibilityTimer = splatTimer;
                //splatt.velocityToSplatThreshold = splatThreshold;
            }
        }

        private void KnockbackPlayer() //TODO IGBalancing PLEASE....
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

                //player.Launch(forceVector); busted AND stinky. Fix hakita pls
                player.rb.velocity += forceVector;
                //Debug.Log(localDirection);
                //Debug.Log(forceVector);
            }
        }

        private void VineBoom()
        {
            GameObject vineBoomNoise = GameObject.Instantiate<GameObject>(new GameObject(),this.transform);
            Destroy(vineBoomNoise, 3.0f);
            AudioSource vineBoomAudioSource = vineBoomNoise.AddComponent<AudioSource>();
            vineBoomAudioSource.playOnAwake = false;
            vineBoomAudioSource.loop = false;
            switch (GetChargeState())
            {
                case 1:
                    vineBoomAudioSource.clip = vB_standard;
                    vineBoomAudioSource.volume = 0.5f;
                    break;
                case 2:
                    vineBoomAudioSource.clip = vB_standard;
                    vineBoomAudioSource.volume = 0.8f;
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
                    vineBoomAudioSource.volume = 0.5f;
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