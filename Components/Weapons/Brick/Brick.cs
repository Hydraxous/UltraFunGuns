using Configgy;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UltraFunGuns
{
    [WeaponAbility("Brick", "Throw brick with <color=orange>Fire 1</color>.", 0, RichTextColors.aqua)]
    [WeaponAbility("Mind Conduit", "Hold <color=orange>Fire 2</color> to assimilate the brick mind.", 1, RichTextColors.lime)]
    [WeaponAbility("Mortar", "Parry the brick to toss it properly.", 2, RichTextColors.red)]
    [UFGWeapon("Brick", "Brick", 0, true, WeaponIconColor.Red)]
    public class Brick : UltraFunGunBase
    {
        [UFGAsset("ThrownBrick")]
        private static GameObject thrownBrickPrefab;

        [UFGAsset("Whoosh_22")]
        private static AudioClip whoosh;

        public Vector2 BrickstormSpeed { get; private set; } = new Vector2(5.0f, 5.0f);

        [Configgable("Weapons/Brick")]
        public static float BrickstormOffset = 3.5f;

        [Configgable("Weapons/Brick")]
        public static float BrickstormPullSpeed = 15.0f;

        [Configgable("Weapons/Brick")]
        public static float BrickstormFlockSpeed = 8.0f;
        

        public float Brickshake { get; private set; } = 0.075f;
        
        public float BrickParryFlightTime { get; private set; } = 0.85f;

        [Configgable("Weapons/Brick")]
        public static float throwForce = 75.0f;

        [Configgable("Weapons/Brick")]
        public static float flockMaxRange = 10.0f;

        [Configgable("Weapons/Brick")]
        public static float targetPointSurfaceOffset = 0.3f;
        
        private bool ready = false;

        private bool throwingBrick = false;

        private List<Rigidbody> thrownBricks = new List<Rigidbody>();

        private ActionCooldown throwCooldown = new ActionCooldown(0.53f, true);
        private ActionCooldown stormCooldown = new ActionCooldown(0.45f, true);


        public bool StormActive { get; private set; }

        public override void OnAwakeFinished()
        {
            ready = thrownBrickPrefab != null;
        }

        public Vector3 PlayerVelocity { get; private set; }

        public override void GetInput()
        {
            if(MonoSingleton<InputManager>.Instance.InputSource.Fire1.WasPerformedThisFrame && throwCooldown.CanFire() && !om.paused && !throwingBrick)
            {
                throwCooldown.AddCooldown();
                throwingBrick = true;
                StartCoroutine(BrickThrower());
            }

            if(MonoSingleton<InputManager>.Instance.InputSource.Fire2.IsPressed && stormCooldown.CanFire() && !om.paused && !throwingBrick)
            {
                StormActive = true;
                PlayerVelocity = player.rb.velocity;
                Ray aimRay = new Ray(mainCam.position, mainCam.forward);

                flockPosition = aimRay.GetPoint(flockMaxRange);

                if (HydraUtils.SphereCastAllMacro(aimRay.origin, 0.18f, aimRay.direction, Mathf.Infinity, out RaycastHit[] hits))
                {
                    if(FilterHitpoint(hits, out Vector3 hitPos))
                        flockPosition = hitPos;
                }

            }
            else if (StormActive)
            {
                StormActive = false;
                stormCooldown.AddCooldown();
            }
        }

        protected override void DoAnimations()
        {
            animator.SetBool("Storm", StormActive);
        }

        private bool FilterHitpoint(RaycastHit[] hits, out Vector3 hitPoint)
        {
            hitPoint = Vector3.zero;

            if(hits.Length <= 0)
            {
                return false;
            }

            foreach(RaycastHit hit in hits)
            {
                if(hit.collider.gameObject.TryGetComponent<ThrownBrick>(out ThrownBrick brick))
                {
                    continue;
                }

                if(EnemyTools.IsColliderEnemy(hit.collider, out EnemyIdentifier eid))
                {
                    if(eid.weakPoint != null)
                    {
                        hitPoint = eid.weakPoint.transform.position;
                    }else
                    {
                        hitPoint = hit.point;
                    }
                    break;
                }
                else if(hit.distance < flockMaxRange)
                {
                    hitPoint = hit.point + (hit.normal * targetPointSurfaceOffset);
                    break;
                }
            }

            return hitPoint != Vector3.zero;
        }

        private void ThrowBrick()
        {
            if(ready)
            {
                Ray aimRay = (weaponIdentifier.duplicate) ? new Ray(firePoint.position, mainCam.forward) : HydraUtils.GetProjectileAimVector(mainCam, firePoint, 0.2f);

                GameObject newBrick = GameObject.Instantiate(thrownBrickPrefab, firePoint.position, UnityEngine.Random.rotation);
                newBrick.transform.forward = aimRay.direction;
                if(newBrick.TryGetComponent<Rigidbody>(out Rigidbody rb))
                {
                    if(!thrownBricks.Contains(rb))
                    {
                        thrownBricks.Add(rb);
                        whoosh.PlayAudioClip(UnityEngine.Random.Range(0.6f, 0.9f));
                    }

                    rb.velocity = aimRay.direction * throwForce;
                }

                if(newBrick.TryGetComponent<ThrownBrick>(out ThrownBrick brick))
                {
                    brick.SetBrickGun(this);
                    brick.RandomRoll();
                }
            }
            
            throwingBrick = false;
        }

        private IEnumerator BrickThrower()
        {
            animator.Play("Throw", 0, 0);
            float timer = 0.116666667f;
            while(timer > 0.0f)
            {
                yield return new WaitForEndOfFrame();
                timer -= Time.deltaTime;
            }
            ThrowBrick();
        }

        private Vector3 flockPosition = new Vector3();

        public Vector3 GetPointPosition()
        {
            return flockPosition;
        }

        private void OnEnable()
        {
            animator.Play("Equip", 0, 0);

            ThrownBrick[] bricksOut = GameObject.FindObjectsOfType<ThrownBrick>();
            for(int i =0; i< bricksOut.Length; i++)
            {
                if (bricksOut[i] == null)
                {
                    continue;
                }

                bricksOut[i].SetBrickGun(this);
            }
        }

        public override string GetDebuggingText()
        {
            string debug = base.GetDebuggingText();
            debug += $"BRICKSTORM: {StormActive}\n";
            debug += $"BRICKS: {thrownBricks.Count}\n";
            debug += $"THROW_CD: {throwCooldown}\n";
            debug += $"STORM_CD: {stormCooldown}";
            return debug;
        }
    }

}