using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UltraFunGuns
{
    [UFGWeapon("Tricksniper", "Tricksniper", 3, true, WeaponIconColor.Green)]
    [WeaponAbility("Fire", "Press <color=orange>Fire 1</color> to fire.", 0, RichTextColors.aqua)]
    [WeaponAbility("Optical Zoom", "Hold <color=orange>Fire 2</color> to engage zoom.", 1, RichTextColors.lime)]
    [WeaponAbility("Noscope!!", "Spinning before shooting directly increases damage and accuracy while not using <color=lime>Optical Zoom</color>.", 2, RichTextColors.yellow)]

    //TODO optimization
    public class Tricksniper : UltraFunGunBase
    {
        [UFGAsset("TrickshotReaction_0")] private static AudioClip trickshotReaction0;
        [UFGAsset("TrickshotReaction_1")] private static AudioClip trickshotReaction1;
        [UFGAsset("TrickshotReaction_2")] private static AudioClip trickshotReaction2;
        [UFGAsset("TrickshotReaction_3")] private static AudioClip trickshotReaction3;

        private static AudioClip[] trickshotReactions = new AudioClip[] { trickshotReaction0, trickshotReaction1, trickshotReaction2, trickshotReaction3 };
        [UFGAsset("TricksniperMuzzleFX")] public static GameObject muzzleFX { get; private set; }
        [UFGAsset("ReflectedSniperShot")] private static GameObject reflectedSniperShot;
        public GameObject scopeUI;
        public GameObject viewModelWrapper;

        public float maxTargetAngle = 150.0f;
        public float spreadTightness = 3.0f;
        public float bulletPenetrationChance = 20.0f; // 100 Percentage

        public int revolutions = 0;
        public float rotationalAngleMultiplier = 4.5f;

        public float maxRange = 2000.0f;

        public float turnCountThreshold = 6.0f;
        public int revolveCountThreshold = 18;

        public int turnsCompleted = 0;
        public float lastRecordedRotation = 0.0f;

        public bool scopedIn = false;
        public float scopeInTime = 0.25f;
        public float scopeTime = 0.0f;

        private float timeScopedIn = 0.0f;

        public float spinCooldownMinThreshold = 5.0f;

        private ActionCooldown fireCooldown = new ActionCooldown(0.65f, true);
        private ActionCooldown turnExpiry = new ActionCooldown(0.35f);

        private bool playReloadAnimWhenUnscoped;

        public override void OnAwakeFinished()
        {
            scopeUI = transform.Find("ScopeUI").gameObject;
            viewModelWrapper = transform.Find("viewModelWrapper").gameObject;
            scopeUI.SetActive(false);
        }

        public override void GetInput()
        {
            if (MonoSingleton<InputManager>.Instance.InputSource.Fire1.WasPerformedThisFrame && fireCooldown.CanFire() && !om.paused)
            {
                fireCooldown.AddCooldown();
                Shoot();
            }

            bool rightClickPressed = (MonoSingleton<InputManager>.Instance.InputSource.Fire2.IsPressed);

            if (rightClickPressed && !om.paused)
            {
                scopeTime = Mathf.Clamp(scopeTime + Time.deltaTime, 0.0f, scopeInTime);
            }
            else if (!om.paused)
            {
                scopeTime = Mathf.Clamp(scopeTime - Time.deltaTime, 0.0f, scopeInTime);
            }

            scopedIn = (scopeTime >= scopeInTime);

            if(scopedIn)
            {
                viewModelWrapper.SetActive(false);
                scopeUI.SetActive(true);
            }else
            {
                viewModelWrapper.SetActive(true);
                scopeUI.SetActive(false);
            }

            CheckSpin();

            timeScopedIn = (scopedIn) ? timeScopedIn + Time.deltaTime: 0.0f;
        }

        private Vector3 lastLook = Vector3.forward;
        private float currentSpin = 0.0f;
        
        private void CheckSpin()
        {
            Vector3 lookVector = mainCam.transform.forward;
            lookVector.y = 0;
            lookVector.Normalize();

            float lookDelta = Vector3.SignedAngle(lastLook, lookVector, Vector3.up);

            lastLook = lookVector;

            if(Mathf.Abs(lookDelta) >= spinCooldownMinThreshold)
            {
                turnExpiry.AddCooldown();
            }

            currentSpin += lookDelta;

            if (turnExpiry.CanFire())
            {
                revolutions = 0;
                currentSpin = 0.0f;
            }   

            if(Mathf.Abs(currentSpin) >= 360f)
            {
                currentSpin -= 360.0f * Mathf.Sign(currentSpin);
                ++revolutions;
            }

        }


        private bool GetTarget(out Vector3 targetDirection) //Return true if enemy target found.
        {
            List<EnemyIdentifier> possibleTargets = new List<EnemyIdentifier>();
            List<Transform> targetPoints = new List<Transform>();

            TargetQuery targetQuery = new TargetQuery()
            {
                queryOrigin = mainCam.position,
                queryDirection = mainCam.forward,
                checkLineOfSight = true,
                maxRange = maxRange,
                lineOfSightCollider = player.playerCollider,
                lineOfSightErrorMargin = 1.2f
            };

            IUFGInteractionReceiver[] foundReceivers = GameObject.FindObjectsOfType<MonoBehaviour>().OfType<IUFGInteractionReceiver>().Where(x=>x!=null).Where(y=>y.Targetable(targetQuery)).OrderBy(z=>Vector3.Distance(z.GetPosition(),mainCam.position)).ToArray();

            if(foundReceivers.Length > 0)
            {
                targetDirection = foundReceivers[0].GetPosition()-mainCam.position;
                return true;
            }

            GameObject[] enemyObjectsActive = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (GameObject enemyObject in enemyObjectsActive)
            {
                if (enemyObject.TryGetComponent<EnemyIdentifier>(out EnemyIdentifier enemyFound))
                {
                    if (!enemyFound.dead && !possibleTargets.Contains(enemyFound))
                    {
                        possibleTargets.Add(enemyFound);
                        Transform enemyTargetPoint;
                        if (enemyFound.weakPoint != null && enemyFound.weakPoint.activeInHierarchy)
                        {
                            enemyTargetPoint = enemyFound.weakPoint.transform;
                        }
                        else
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

                        //Cone check
                        Vector3 directionToEnemy = (enemyTargetPoint.position - mainCam.position).normalized;
                        Vector3 lookDirection = mainCam.TransformDirection(Vector3.forward).normalized;

                        float currentTargetAngle = (revolutions * (rotationalAngleMultiplier * (revolutions + 1.25f)));
                        if(Vector3.Angle(directionToEnemy, lookDirection) <= Mathf.Clamp(currentTargetAngle, 0.0f, maxTargetAngle) || revolutions > 7)
                        {
                            targetPoints.Add(enemyTargetPoint);
                        }
                    }
                }
            }

            //Get closest target
            int closestIndex = -1;
            float closestDistance = Mathf.Infinity;
            for (int i = 0; i < targetPoints.Count; i++)
            {
                Vector3 distance = targetPoints[i].position - mainCam.position;
                if (distance.sqrMagnitude < closestDistance)
                {
                    closestIndex = i;
                    closestDistance = distance.sqrMagnitude;
                }
            }

            if (closestIndex > -1)
            {
                targetDirection = targetPoints[closestIndex].position - mainCam.position;
                return true;
            }
            else //No targets found.
            {
                targetDirection = Vector3.zero;
                return false;
            }
        }

        private void Shoot()
        {    
            bool penetration = scopedIn;
            bool enemyFound = false;
            Ray shootRay = new Ray(mainCam.position, mainCam.forward);

            if (revolutions > 0)
            {
                if (GetTarget(out Vector3 targetDir))
                {
                    enemyFound = true;
                    shootRay.direction = targetDir;

                    Vector3 leveledSample = targetDir;
                    leveledSample.y = 0.0f;

                    Vector3 cameraLeveledSample = shootRay.direction;
                    cameraLeveledSample.y = 0.0f;


                    if(Vector3.Dot(leveledSample.normalized, cameraLeveledSample.normalized) < 0.0f)
                    {
                        Quaternion newLook = Quaternion.LookRotation(new Vector3(leveledSample.x, shootRay.direction.y, leveledSample.z), Vector3.up);
                        HydraUtils.SetPlayerRotation(newLook);
                    }
                }
                penetration = (bulletPenetrationChance >= UnityEngine.Random.Range(0.0f, 100.0f) || penetration);
            }    

            if(!scopedIn && !enemyFound)
            {
                float randomDirectionX = UnityEngine.Random.Range(-1.0f, 1.0f);
                float randomDirectionY = UnityEngine.Random.Range(-1.0f, 1.0f);

                shootRay.direction = mainCam.TransformDirection(randomDirectionX, randomDirectionY, spreadTightness);
                penetration = true;
            }

            DoHit(shootRay, penetration);

            Instantiate<GameObject>(muzzleFX, (scopedIn) ? mainCam.position : firePoint.position, Quaternion.identity).transform.forward = (scopedIn) ? mainCam.forward : firePoint.forward;

            if(!scopedIn)
            {
                animator.Play("Fire", 0, 0);
            }else
            {
                playReloadAnimWhenUnscoped = true;
            }
            CameraController.Instance.CameraShake(1.1f);
        }

        private void DoHit(Ray hitRay, bool penetration)
        {
            float damageAmount = (scopedIn) ? 1.5f : 3.0f;

            RaycastHit[] hits = Physics.RaycastAll(hitRay, maxRange, LayerMask.GetMask("Limb", "BigCorpse", "Outdoors", "Environment", "Default"));
            if (hits.Length > 0)
            {
                if (!(hits.Length == 1 && hits[0].collider.gameObject.name == "CameraCollisionChecker"))
                {
                    int endingHit = 0;
                    hits = HydraUtils.SortRaycastHitsByDistance(hits);
                    for (int i = 0; i < hits.Length; i++)
                    {
                        endingHit = i;

                        if ((hits[i].collider.gameObject.layer == 24 || hits[i].collider.gameObject.layer == 25 || hits[i].collider.gameObject.layer == 8))
                        {
                            if ( Mathf.Abs(Vector3.Dot(hitRay.direction, hits[i].normal)) < 0.25f)
                            {
                                DoHit(new Ray(hitRay.origin, Vector3.Reflect(hitRay.direction, hits[i].normal)), penetration);
                            }
                            break;
                        }

                        if (hits[i].collider.IsColliderEnemy(out EnemyIdentifier eid))
                        {
                            eid.DeliverDamage(eid.gameObject, hitRay.direction, hits[i].point, damageAmount, true, damageAmount, gameObject);
                            if(scopedIn)
                            {
                                if(timeScopedIn < 0.085f)
                                    WeaponManager.AddStyle(50, "tricksniperquickscope", gameObject, eid);
                            }else
                            {
                                bool trick = revolutions > 0;
                                if(trick && trickshotReactions != null)
                                {
                                    trickshotReactions[UnityEngine.Random.Range(0, trickshotReactions.Length)].PlayAudioClip();
                                }

                                WeaponManager.AddStyle((trick) ? 100 : 30, (trick) ? "tricksniper360" : "tricksnipernoscope", gameObject, eid);
                            }

                            if (!penetration)
                            {
                                break;
                            }
                        }

                        if (hits[i].collider.gameObject.TryGetComponent<Breakable>(out Breakable breakable))
                        {
                            breakable.Break();
                            if (!penetration)
                            {
                                break;
                            }
                        }

                        if (hits[i].collider.gameObject.TryFindComponent<IUFGInteractionReceiver>(out IUFGInteractionReceiver uFGInteractionReceiver))
                        {
                            UFGInteractionEventData eventData = new UFGInteractionEventData()
                            {
                                tags = new string[] { "shot", "pierce", "heavy", (scopedIn) ? "" : "god" },
                                data = "",
                                interactorPosition = mainCam.position,
                                direction = hitRay.direction,
                                invokeType = typeof(Tricksniper),
                                power = 60f
                            };

                            if(uFGInteractionReceiver.Interact(eventData) && !penetration)
                            {
                                break;
                            }
                        }

                        if (hits[i].collider.gameObject.TryFindComponent<Grenade>(out Grenade grenade))
                        {
                            MonoSingleton<TimeController>.Instance.ParryFlash();
                            grenade.Explode();
                            if (!penetration)
                            {
                                break;
                            }
                        }

                        if (hits[i].collider.gameObject.TryFindComponent<Coin>(out Coin coin))
                        {
                            coin.DelayedReflectRevolver(hits[i].point, reflectedSniperShot);
                        }
                    }
                    CreateBulletTrail((scopedIn) ? mainCam.position : firePoint.position, hits[endingHit].point, hits[endingHit].normal);
                    return;
                }
            }
        }

        private void CreateBulletTrail(Vector3 startPosition, Vector3 endPosition, Vector3 normal)
        {
            if(Prefabs.BulletTrail == null)
            {
                return;
            }

            GameObject newBulletTrail = Instantiate<GameObject>(Prefabs.BulletTrail, endPosition, Quaternion.identity);
            newBulletTrail.transform.up = normal;
            LineRenderer line = newBulletTrail.GetComponent<LineRenderer>();
            Vector3[] linePoints = new Vector3[2] { startPosition, endPosition };
            line.SetPositions(linePoints);
        }

        protected override void DoAnimations()
        {
            animator.SetFloat("ScopeAmount", scopeTime/scopeInTime);
            if(!scopedIn && scopeTime <= 0.0f && playReloadAnimWhenUnscoped)
            {
                playReloadAnimWhenUnscoped = false;
                animator.Play("Tricksniper_Reload", 0, 0);
            }
        }

        private void OnEnable()
        {
            animator.Play("Equip",0 ,0);
        }

        public override string GetDebuggingText()
        {
            string debug = base.GetDebuggingText();
            debug += $"SPIN: {currentSpin}\n";
            debug += $"TURN_CD: {turnExpiry}\n";
            debug += $"REVOLUTIONS: {revolutions}\n";
            debug += $"SCOPE: ({scopeTime/scopeInTime}) {scopedIn}\n";
            return debug;
        }

    }
}
