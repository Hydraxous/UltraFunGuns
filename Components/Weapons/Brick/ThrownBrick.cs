using System;
using System.Collections;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

namespace UltraFunGuns
{
    public class ThrownBrick : MonoBehaviour, IUFGInteractionReceiver, ICleanable
    {
        [UFGAsset("BrickBreakFX")] private static GameObject brickBreakFX;
        [UFGAsset("TennisHit")] private static AudioClip tennisHit;

        private Animator thrownBrickAnimator;
        private Transform brickMesh;
        private Brick brickShooter;
        private Rigidbody rb;

        private UltraFunGunBase.ActionCooldown soundCooldown = new UltraFunGunBase.ActionCooldown(0.05f);
        private UltraFunGunBase.ActionCooldown damageCooldown = new UltraFunGunBase.ActionCooldown(0.25f);
        private UltraFunGunBase.ActionCooldown hitLossCooldown = new UltraFunGunBase.ActionCooldown(0.5f);
        private UltraFunGunBase.ActionCooldown guidedCooldown = new UltraFunGunBase.ActionCooldown(0.2f);

        private bool sleeping = false;
        private bool guided = false;

        private int hitsRemaining = 25;
        private int enemyParries = 0;
        private bool parried;

        private static int HitsRemaining = 25;

        [Commands.UFGDebugMethod("Remaining Set", "Set htings")]
        public static void RemainingsSet()
        {
            HitsRemaining = 50000;
        }

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            thrownBrickAnimator = GetComponent<Animator>();
            brickMesh = GetComponentInChildren<MeshRenderer>()?.transform;
        }

        private void Start()
        {
            hitsRemaining = HitsRemaining;
        }

        public void SetBrickGun(Brick brickGun)
        {
            brickShooter = brickGun;
            if(thrownBrickAnimator != null)
            {
                thrownBrickAnimator.Play("StormPulse", 0, 0);
            }
        }

        public void Update()
        {
            if(hitsRemaining <= 0)
            {
                Break();
            }

            CheckFlock();
        }

        private void LateUpdate()
        {
            if(brickShooter == null || thrownBrickAnimator == null)
            {
                return;
            }

            thrownBrickAnimator.SetBool("Storm", brickShooter.StormActive);

            if(brickMesh == null)
            {
                return;
            }

            if(brickShooter.StormActive)
            {
                Vector3 newLocalPos = UnityEngine.Random.insideUnitSphere;
                newLocalPos *= brickShooter.Brickshake;

                brickMesh.localPosition = newLocalPos;
            }else
            {
                brickMesh.localPosition = Vector3.MoveTowards(brickMesh.localPosition, Vector3.zero, 0.015f);
            }
        }

        private void CheckFlock()
        {
            if (brickShooter == null || rb == null)
            {
                return;
            }

            if (!brickShooter.StormActive || (enemyParries > 0 && parried == false))
            {
                return;
            }

            sleeping = false;
            guided = false;
            enemyParries = 0;

            Vector3 flockTarget = brickShooter.GetPointPosition();

            Vector3 newDirection = flockTarget - transform.position;

            rb.velocity = newDirection * brickShooter.BrickstormFlockSpeed;
        }

        private void CheckStorm()
        {
            if (brickShooter == null || rb == null)
            {
                return;
            }

            if(!brickShooter.StormActive)
            {
                return;
            }

            Transform orbitPoint = brickShooter.transform;

            Vector3 masterPos = orbitPoint.position;
            Quaternion masterRot = orbitPoint.rotation;

            Vector3 myPos = transform.position;

            Vector3 toMaster = masterPos - myPos;

            Vector3 tangetFromPlayer = Vector3.Cross(toMaster, Vector3.up);

            tangetFromPlayer.Normalize();

            tangetFromPlayer *= brickShooter.BrickstormSpeed.x;
            tangetFromPlayer += Vector3.up * brickShooter.BrickstormSpeed.y * Mathf.Sign(masterPos.y - myPos.y) * Time.deltaTime; //vertical bias

            float distanceFromMaster = toMaster.magnitude;

            if(distanceFromMaster > brickShooter.BrickstormOffset + 0.35f || distanceFromMaster < brickShooter.BrickstormOffset - 0.35f)
            {
                float offsetBias = brickShooter.BrickstormPullSpeed * -Mathf.Sign(brickShooter.BrickstormOffset - distanceFromMaster);
                tangetFromPlayer += toMaster.normalized * offsetBias * Time.deltaTime;
            }

            Vector3 newVelocity = brickShooter.PlayerVelocity;
            newVelocity += tangetFromPlayer;

            rb.velocity = newVelocity;

        }

        [UFGAsset("BrickImpactRock")] static AudioClip collisionSound;
        [UFGAsset("BrickImpactFlesh")] static AudioClip fleshHitSound;

        private void OnCollisionEnter(Collision col)
        {
            if (sleeping)
            {
                return;
            }

            if (guided && guidedCooldown.CanFire())
            {
                guided = false;
            }

            float force = col.relativeVelocity.magnitude;

            if (force <= 3.0f)
            {
                //return;
            }

            //enemy hit check
            if (CheckEnemyHit(col))
            {
                return;
            }

            if (enemyParries > 0 && !parried)
            {
                if (CheckPlayerHit(col))
                {
                    NewMovement.Instance.GetHurt(30*enemyParries, false, 1.0f, true, true);
                    collisionSound.PlayAudioClip(col.GetContact(0).point, 1.0f + Mathf.Clamp(0.15f*enemyParries,0.5f,3.0f), 1.0f, 0.0f);
                    Break();
                    return;
                }
            }

            if (collisionSound != null && soundCooldown.CanFire())
            {
                soundCooldown.AddCooldown();
                collisionSound.PlayAudioClip(col.GetContact(0).point, UnityEngine.Random.Range(0.75f, 1.25f), 1.0f, 1.0f);
            }

            //Regular collisions should decrease hit counter.
            if (hitLossCooldown.CanFire())
            {
                --hitsRemaining;
                hitLossCooldown.AddCooldown();
            }

            if(!brickShooter.StormActive)
            {
                if (col.gameObject.tag == "Floor" || col.gameObject.layer == LayerMask.GetMask("Environment"))
                {
                    Sleep();
                }
            }
        }

        //After its parried by v2 or training bot it will damage the player
        private bool CheckPlayerHit(Collision col)
        {
            HydraLogger.Log(HydraUtils.CollisionInfo(col), DebugChannel.Warning);
            if(col.gameObject.tag == "Player")
            {
                return true;
            }
            return false;
        }

        private bool CheckEnemyHit(Collision col)
        {
            if(!damageCooldown.CanFire())
            {
                return false;
            }

            if(EnemyTools.IsCollisionEnemy(col, out EnemyIdentifier eid))
            {
                if (eid.bigEnemy && !brickShooter.StormActive && parried)
                {
                    //If the enemy is V2 or training bot, the brick should fly back at the player lol.
                    if (UnityEngine.Random.value > Mathf.Clamp(0.75f/(enemyParries-2),0f,0.75f))
                    {
                        EnemyParry();
                        damageCooldown.AddCooldown();
                        return false;
                    }
                }

                float damage = 1.0f+(enemyParries*50f);
                if(brickShooter.StormActive)
                {
                    eid.Override().AddStyleEntryOnDeath(new StyleEntry(4, "brickmindkill", 2.0f, brickShooter.gameObject));
                }
                eid.DeliverDamage(eid.gameObject, rb.velocity, col.GetContact(0).point, damage, true, 1.0f, brickShooter.gameObject);
                hitsRemaining -= 5;
                damageCooldown.AddCooldown();
                fleshHitSound.PlayAudioClip(UnityEngine.Random.Range(0.8f, 1.1f));

                //If brick kills enemy lob at another enemy or return it to the player
                if (eid.health <= 0.0f && hitsRemaining > 0 && parried)
                {
                    if(EnemyTools.TryGetHomingTarget(transform.position, out Transform homingTarget, out EnemyIdentifier eid2))
                    {
                        if(eid2 != eid)
                        {
                            GuidedLob(homingTarget,Vector3.up*0.25f);
                            //LobAtTarget(homingTarget.position);
                        }
                    }else
                    {
                        parried = false;
                        enemyParries = 0;
                        LobAtPlayer();
                    }
                }

                return true;
            }

            if (brickShooter == null)
            {
                return false;
            }

            if(brickShooter.StormActive)
            {
                if(col.gameObject.TryGetComponent<ThrownBrick>(out ThrownBrick thrownBrick))
                {
                    Physics.IgnoreCollision(col.GetContact(0).thisCollider, col.GetContact(0).otherCollider);
                }
            }

            return false;
        }

        private bool broken;

        public void Break()
        {
            if (broken)
                return;

            broken = true;

            if(brickBreakFX != null)
            {
                GameObject.Instantiate<GameObject>(brickBreakFX, transform.position, Quaternion.identity);
            }
            Destroy(gameObject);
        }

        //TODO this is broken.
        public void Shot(BeamType beamType)
        {
            Break();
        }

        private void LobAtTarget(Vector3 target)
        {
            //Vector3 newVelocity = HydraUtils.GetVelocityTrajectory(transform.position, target, brickShooter.BrickParryFlightTime);
            Trajectory brickTrajectory = new Trajectory(transform.position, target, brickShooter.BrickParryFlightTime/(enemyParries+1));
            Vector3 newVelocity = brickTrajectory.GetRequiredVelocity();
            rb.velocity = newVelocity;

            RandomRoll();
            Visualizer.DrawRay(transform.position, newVelocity, brickShooter.BrickParryFlightTime);
        }

        private void LobAtPlayer(bool damage = false)
        {
            NewMovement player = NewMovement.Instance;
            if(player == null)
            {
                return;
            }
            parried = false;
            GuidedLob(player.cc.transform);
            return;

            Vector3 playerPos = player.cc.transform.position;
            Vector3 velocity = player.rb.velocity * (brickShooter.BrickParryFlightTime*2);
            LobAtTarget(playerPos + velocity);
        }

        private void GuidedLob(Transform target)
        {
            GuidedLob(target, Vector3.zero);
        }

        private void GuidedLob(Transform target, Vector3 relativeOffset)
        {
            if(!guided && target != null)
            {
                RandomRoll();
                guided = true;
                guidedCooldown.AddCooldown();
                float flightTime = Mathf.Max(brickShooter.BrickParryFlightTime / ((enemyParries / 2.0f) + 1.0f),0.1f);
                StartCoroutine(FlightComputer(new Trajectory(transform.position, target.position, flightTime), target, relativeOffset));
            }
        }

        private IEnumerator FlightComputer(Trajectory trajectory, Transform guidedTarget, Vector3 relativeOffset)
        {
            float timer = trajectory.AirTime;
            trajectory.Origin += Vector3.up * 0.5f;
            transform.position = trajectory.GetPoint(Mathf.InverseLerp(trajectory.AirTime, 0.0f, timer));

            bool inRange = false;
            Vector3 lastTargetPos = guidedTarget.position;

            while (timer > 0.0f && guided && guidedTarget != null && !inRange)
            {
                lastTargetPos = guidedTarget.position + relativeOffset;
                trajectory.End = lastTargetPos;
                rb.position = trajectory.GetPoint(Mathf.InverseLerp(trajectory.AirTime, 0.0f, timer));
                inRange = (1.5f > Vector3.Distance(rb.position, trajectory.End));
                yield return new WaitForEndOfFrame();
                timer -= Time.deltaTime;
            }

            rb.velocity = ((lastTargetPos + relativeOffset) - rb.position).normalized * (enemyParries + 1)*40.0f;
        }

        public void RandomRoll()
        {
            Vector3 randomTorque = UnityEngine.Random.insideUnitSphere * 90.0f;
            rb.AddTorque(randomTorque, ForceMode.Impulse);
        }

        private void Parry(Vector3 inDirection)
        {
            parried = true;

            if(guided)
            {
                guided = false;
            }

            if (Prefabs.SmackFX != null)
            {
                Transform newFx = Instantiate<GameObject>(Prefabs.SmackFX, transform.position, Quaternion.identity).transform;
                newFx.localScale *= 3.0f;
            }

            if (EnemyTools.TryGetHomingTarget(transform.position, out Transform homingTarget, out EnemyIdentifier eid))
            {
                GuidedLob(homingTarget);
                //LobAtTarget(homingTarget.position);
            }
            else
            {
                //Just toss it forward, there are no enemies found.
                Vector3 biasBasis = CameraController.Instance.transform.rotation * Vector3.up;
                Vector3 fireDirection = inDirection.normalized + (biasBasis * 0.25f);
                Vector3 newVelocity = fireDirection * brickShooter.BrickstormFlockSpeed * 25.0f;
                rb.velocity = newVelocity;
                RandomRoll();
            }
        }

        public void EnemyParry()
        {
            hitsRemaining += hitsRemaining;
            
            if (Prefabs.SmackFX != null)
            {
                Transform newFx = Instantiate<GameObject>(Prefabs.SmackFX, transform.position, Quaternion.identity).transform;
                newFx.localScale *= 3.0f;
            }

            TimeController.Instance.ParryFlash();
            ++enemyParries;

            float soundPitch = Mathf.Clamp(0.85f + (0.15f * enemyParries),1.0f,3.0f);
            
            if(tennisHit != null)
                tennisHit.PlayAudioClip(soundPitch, 1.0f);

            LobAtPlayer();
        }

        private void Sleep()
        {
            sleeping = true;
            parried = false;
            enemyParries = 0;
        }

        public bool Parried(Vector3 aimVector)
        {
            if (rb == null || brickShooter == null || parried)
            {
                return false;
            }
            
            if(sleeping)
            {
                return false;
            }

            Parry(aimVector);
            return true;
        }

        public bool Interact(UFGInteractionEventData interaction)
        {
            return false;
        }

        public Vector3 GetPosition()
        {
            return transform.position;
        }

        public bool Targetable(TargetQuery query)
        {
            return false;
        }

        public void Cleanup()
        {
            Break();
        }
    }
}
