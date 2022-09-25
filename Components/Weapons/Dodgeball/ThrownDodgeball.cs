using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    public class ThrownDodgeball : MonoBehaviour
    {
        private UltraFunGunBase.ActionCooldown hurtCooldown = new UltraFunGunBase.ActionCooldown(0.1f);
        private UltraFunGunBase.ActionCooldown hitSoundCooldown = new UltraFunGunBase.ActionCooldown(0.015f);
        private UltraFunGunBase.ActionCooldown maxRangeReflectCooldown = new UltraFunGunBase.ActionCooldown(0.5f);

        public GameObject dodgeballPopFXPrefab;
        public GameObject impactSound;

        public Transform ballMesh;

        public Transform homingTarget;
        public EnemyIdentifier homingTargetEID;

        public Dodgeball dodgeballWeapon;

        public Rigidbody rb;

        private Animator animator;

        private Vector3 sustainedVelocity = Vector3.zero;
        private Vector3 oldSustainedVelocity = Vector3.zero;

        private NewMovement player;

        public float reboundVelocityMultiplier = 2.5f;
        public int remainingBounces = 50;
        public int timesBounced = 19; //TODO fix this, it is set to 20 for the purpose of scaling properly when it is first thrown/bounced there is a better way I can feel it.
        public float lifeTime = 15.0f;
        public float hitDamageMultiplier = 0.03f;
        public float exciteSpeedMultiplier = 1.15f;
        public float exciteDamageMultiplier = 1.35f;
        public int timesExcited = 0;
        public float maxBallDistance = 4000000.0f; //2000m^2


        public bool isHoming = false;
        public bool isExcited = false;
        public bool beingPulled = false;

        private AudioSource bigHitSound;
        private AudioSource homingSound;
        private AudioSource exciteSound;
        private AudioSource recallBonkSound;
        private AudioSource excitedHitSound;
        private AudioSource glassImpactSound;
        private AudioSource altExciteHitSound;


        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
           
            animator = GetComponent<Animator>();
            ballMesh = transform.Find("DodgeballMesh");
            HydraLoader.prefabRegistry.TryGetValue("DodgeballPopFX", out dodgeballPopFXPrefab);
            HydraLoader.prefabRegistry.TryGetValue("DodgeballImpactSound", out impactSound);
            bigHitSound = transform.Find("Audios/BigHit").GetComponent<AudioSource>();
            homingSound = transform.Find("Audios/HomingSound").GetComponent<AudioSource>();
            exciteSound = transform.Find("Audios/ExciteSound").GetComponent<AudioSource>();
            recallBonkSound = transform.Find("Audios/RecallBonk").GetComponent<AudioSource>();
            excitedHitSound = transform.Find("Audios/ExcitedHit").GetComponent<AudioSource>();
            glassImpactSound = transform.Find("Audios/GlassImpact").GetComponent<AudioSource>();
            altExciteHitSound = transform.Find("Audios/BallinHitSound").GetComponent<AudioSource>();

        }

        private void Start()
        {
            player = MonoSingleton<NewMovement>.Instance;
            homingSound.Play();
            homingSound.Pause();
        }

        private void Update()
        {
            lifeTime -= Time.deltaTime;
            if(lifeTime <= 0.0f && !beingPulled && !isHoming)
            {
                Pop();
            }
        }

        private void FixedUpdate()
        {
            if(isHoming && !beingPulled)
            {
                DoHomingAttack();
            }
            rb.velocity = sustainedVelocity;
        }

        private void LateUpdate()
        {
            oldSustainedVelocity = sustainedVelocity;
            animator.SetBool("Excited", isExcited);
            Vector3 toPlayer = player.transform.position - transform.position;
            if(toPlayer.sqrMagnitude >= maxBallDistance && maxRangeReflectCooldown.CanFire())
            {
                maxRangeReflectCooldown.AddCooldown();
                SetSustainVelocity(toPlayer);
            }
        }

        public void AddBounces(int bouncesToAdd)
        {
            timesBounced += bouncesToAdd;
        }

        public void ExciteBall(int excitementToAdd = 1, bool doHitstop = true)
        {
            MonoSingleton<CameraController>.Instance.CameraShake(2.5f);
            exciteSound.Play();
            if (doHitstop)
            {
                MonoSingleton<TimeController>.Instance.HitStop(0.2f);
            }
            timesExcited += excitementToAdd;
            lifeTime += 3.0f;
            MonoSingleton<StyleHUD>.Instance.AddPoints(0, "hydraxous.ultrafunguns.dodgeballparry", dodgeballWeapon.gameObject, null, timesExcited);

            isExcited = true;
            remainingBounces = (int) (remainingBounces * (1.25f * excitementToAdd));
            
            SetSustainVelocity(sustainedVelocity);
            isHoming = TryGetHomingTarget();
        }

        private bool TryGetHomingTarget()
        {
            List<EnemyIdentifier> possibleTargets = new List<EnemyIdentifier>();
            List<Transform> targetPoints = new List<Transform>();
            GameObject[] enemyObjectsActive = GameObject.FindGameObjectsWithTag("Enemy");
            foreach(GameObject enemyObject in enemyObjectsActive)
            {
                if(enemyObject.TryGetComponent<EnemyIdentifier>(out EnemyIdentifier enemyFound))
                {
                    if(!enemyFound.dead && !possibleTargets.Contains(enemyFound))
                    {
                        possibleTargets.Add(enemyFound);
                        Transform enemyTargetPoint;
                        if(enemyFound.weakPoint != null && enemyFound.weakPoint.activeInHierarchy)
                        {
                            enemyTargetPoint = enemyFound.weakPoint.transform;
                        }else
                        {
                            EnemyIdentifierIdentifier enemyFoundIdentifier = enemyFound.GetComponentInChildren<EnemyIdentifierIdentifier>();
                            if (enemyFoundIdentifier)
                            {
                                enemyTargetPoint = enemyFoundIdentifier.transform;
                            }
                            else
                            {
                                enemyTargetPoint = enemyFound.transform;
                            }
                        }
                        
                        //LOS CHECK
                        Vector3 directionToEnemy = enemyTargetPoint.position - transform.position;
                        if (Physics.Raycast(transform.position, directionToEnemy, out RaycastHit rayHit, directionToEnemy.magnitude, LayerMask.GetMask("Limb", "BigCorpse", "Outdoors", "Environment", "Default")))
                        {
                            if (rayHit.collider.gameObject.TryGetComponent<EnemyIdentifier>(out EnemyIdentifier losEnemyID) || rayHit.collider.gameObject.TryGetComponent<EnemyIdentifierIdentifier>(out EnemyIdentifierIdentifier losEnemyIDID))
                            { 
                                targetPoints.Add(enemyTargetPoint);
                            }
                        }

                    }
                }
            }

            int closestIndex = -1;
            float closestDistance = Mathf.Infinity;
            for(int i = 0; i < targetPoints.Count; i++)
            {
                Vector3 distance = targetPoints[i].position - transform.position;
                if(distance.sqrMagnitude < closestDistance)
                {
                    closestIndex = i;
                    closestDistance = distance.sqrMagnitude;
                }
            }

            if(closestIndex > -1)
            {
                homingTarget = targetPoints[closestIndex];
                homingTargetEID = homingTarget.GetComponent<EnemyIdentifier>();
                return true;
            }else
            {
                return false;
            }
            
        }

        private void DoHomingAttack()
        {
            if(homingTarget == null)
            {
                StopHomingAttack();
                return;
            }

            if (homingTargetEID == null)
            {
                if (!homingTarget.TryGetComponent<EnemyIdentifier>(out homingTargetEID))
                {
                    if(!homingTarget.TryGetComponent<EnemyIdentifierIdentifier>(out EnemyIdentifierIdentifier homingTargetEIDID))
                    {
                        StopHomingAttack();
                        return;
                    }
                    homingTargetEID = homingTargetEIDID.eid;
                }
            }

            if (homingTargetEID.dead)
            {
                StopHomingAttack();
                return;
            }

            SetSustainVelocity(homingTarget.position - transform.position); //sets velocity to point at the enemy it's tracking.
            homingSound.UnPause();
        }

        private void StopHomingAttack()
        {
            homingTarget = null;
            homingTargetEID = null;
            homingSound.Pause();
            isHoming = false;
        }

        public void Pop()
        {
            GameObject.Instantiate<GameObject>(dodgeballPopFXPrefab, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }

        private void DoHit(Collision col)
        {
            animator.Play("ThrownDodgeballSquish");
            EnemyIdentifier enemy = null;
            GameObject objectHit = null;
            AudioSource clipToPlay = null;

            if (col.collider.TryGetComponent<EnemyIdentifier>(out enemy))
            {
                objectHit = enemy.gameObject;
            }

            if(col.collider.TryGetComponent<EnemyIdentifierIdentifier>(out EnemyIdentifierIdentifier enemyIDID))
            {
                enemy = enemyIDID.eid;
                objectHit = enemyIDID.gameObject;
            }

            if(col.collider.TryGetComponent<Breakable>(out Breakable breakable))
            {
                breakable.Break();
            }

            if (col.collider.TryGetComponent<Glass>(out Glass glass))
            {
                glass.Shatter();
                glassImpactSound.Play();
            }

            if (enemy != null && hurtCooldown.CanFire())
            {
                if(!enemy.dead)
                {
                    int pointsToAdd = 0;
                    string pointID = "hydraxous.ultrafunguns.dodgeballhit";
                    bool tryExplode = false;

                    hurtCooldown.AddCooldown();

                    if (beingPulled)
                    {
                        pointsToAdd += 10;
                        pointID = "hydraxous.ultrafunguns.dodgeballreversehit";
                        clipToPlay = recallBonkSound;
                    }

                    if (isExcited)
                    {
                        pointsToAdd += 20;
                        clipToPlay = bigHitSound;
                        tryExplode = true;
                    }

                    if(isHoming)
                    {
                        pointsToAdd += 30;
                        pointID = "hydraxous.ultrafunguns.dodgeballparryhit";
                        if(dodgeballWeapon.basketBallMode)
                        {
                            altExciteHitSound.Play();
                        }else
                        {
                            excitedHitSound.Play();
                        }
                        MonoSingleton<TimeController>.Instance.HitStop(Mathf.Clamp(0.1f * timesExcited,0.0f,1.4f));
                    }

                    float currentExciteMultiplier = Mathf.Clamp((exciteDamageMultiplier * timesExcited),1.0f,Mathf.Infinity);
                    enemy.DeliverDamage(objectHit, sustainedVelocity.normalized, col.GetContact(0).point, sustainedVelocity.magnitude * hitDamageMultiplier * currentExciteMultiplier, tryExplode, 0, dodgeballWeapon.gameObject);
                    MonoSingleton<StyleHUD>.Instance.AddPoints(pointsToAdd, pointID, dodgeballWeapon.gameObject, enemy);
                    StopHomingAttack();
                }
                else
                {
                    UnityEngine.Physics.IgnoreCollision(col.collider, GetComponent<SphereCollider>());
                }
                
            }

            if(hitSoundCooldown.CanFire())
            {
                --remainingBounces;
                ++timesBounced;
                hitSoundCooldown.AddCooldown();
                if(clipToPlay != null)
                {
                    clipToPlay.Play();
                }
                else
                {
                    GameObject.Instantiate<GameObject>(impactSound, transform);
                }
                
            }

        }

        private void OnCollisionEnter(Collision col)
        {
            if (col.gameObject.name == "Player")
            {
                return;
            }
            
            DoHit(col);

            Vector3 reflectorNormal;
            if (col.contacts.Length > 1)
            {
                int randomIndex = UnityEngine.Random.Range(0, col.contacts.Length);
                reflectorNormal = col.GetContact(randomIndex).normal;
            }
            else
            {
                reflectorNormal = col.GetContact(0).normal;
            }
            SetSustainVelocity(Vector3.Reflect(sustainedVelocity, col.GetContact(0).normal));

            if (remainingBounces <= 0)
            {
                Pop();
            }

        }

        public void SetSustainVelocity(Vector3 velocity, bool overrideCalculation = false)
        {
            if(overrideCalculation)
            {
                sustainedVelocity = velocity;
            }else
            {
                sustainedVelocity = CalculateVelocity(velocity);       
            }
            transform.forward = sustainedVelocity;
        }

        private Vector3 CalculateVelocity(Vector3 direction)
        {
            if(isExcited)
            {
                return direction.normalized * (reboundVelocityMultiplier * timesBounced + 1) * (exciteSpeedMultiplier * timesExcited);
            }
            else
            {
                return direction.normalized * (reboundVelocityMultiplier * timesBounced + 1);
            }
            
        }

        private void OnTriggerEnter(Collider col)
        {
            if(col.gameObject.name == "DodgeballCatcher")
            {
                StopHomingAttack();
                dodgeballWeapon.CatchBall();
                Destroy(gameObject);
            }
            else if(col.gameObject.TryGetComponent<DeathZone>(out DeathZone zone) || col.gameObject.TryGetComponent<OutOfBounds>(out OutOfBounds oob))
            {
                StopHomingAttack();
                Pop();
            }
        }

        private void OnDestroy()
        {
            if(dodgeballWeapon != null)
            {
                dodgeballWeapon.dodgeBallActive = false;
            }    
        }

    }
}
