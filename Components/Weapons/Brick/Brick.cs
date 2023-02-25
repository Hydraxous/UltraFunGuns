using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UltraFunGuns;
using UnityEngine;

namespace UltraFunGuns
{
    [WeaponAbility("Brick", "Throw brick with <color=orange>Fire 1</color>.", 0, RichTextColors.aqua)]
    [WeaponAbility("Brick Storm", "Hold <color=orange>Fire 2</color> to assimilate the brick mind.", 1, RichTextColors.red)]
    [FunGun("Brick", "Brick", 0, true, WeaponIconColor.Red)]
    public class Brick : UltraFunGunBase
    {
        [UFGAsset("ThrownBrick")]
        private static GameObject thrownBrickPrefab;

        [UFGAsset("Whoosh_22")]
        private static AudioClip whoosh;

        public Vector2 BrickstormSpeed { get; private set; } = new Vector2(5.0f, 5.0f);
        public float BrickstormOffset { get; private set; } = 3.5f;
        public float BrickstormPullSpeed { get; private set; } = 15.0f;
        public float BrickstormFlockSpeed { get; private set; } = 50.0f;

        //TODO make bricks glow
        //TODO make bricks fragment
        //TODO make bricks more angry
        //TODO more sounds
        //TODO Icon

        public float force = 75.0f;
        public float spawnOffset = 0.25f;
        public float flockMaxRange = 150.0f;
        private bool ready = false;

        private List<Rigidbody> thrownBricks = new List<Rigidbody>();

        private ActionCooldown throwCooldown = new ActionCooldown(0.93f, true);
        private ActionCooldown stormCooldown = new ActionCooldown(0.45f, true);

        public bool StormActive { get; private set; }

        public override void OnAwakeFinished()
        {

            ready = thrownBrickPrefab != null;

        }

        public Vector3 PlayerVelocity { get; private set; }

        public override void GetInput()
        {
            if(MonoSingleton<InputManager>.Instance.InputSource.Fire1.WasPerformedThisFrame && throwCooldown.CanFire())
            {
                throwCooldown.AddCooldown();
                ThrowBrick();
            }

            if(MonoSingleton<InputManager>.Instance.InputSource.Fire2.IsPressed && stormCooldown.CanFire())
            {
                StormActive = true;
                PlayerVelocity = player.rb.velocity;
                Ray aimRay = new Ray(mainCam.position, mainCam.forward);

                if(HydraUtils.SphereCastMacro(aimRay.origin, 0.1f, aimRay.direction, flockMaxRange, out RaycastHit hit))
                {
                    flockPosition = hit.point;
                }
                else
                {
                    flockPosition = aimRay.GetPoint(flockMaxRange);
                }
            }
            else if (StormActive)
            {
                StormActive = false;
                stormCooldown.AddCooldown();
            }

            
        }

        private void ThrowBrick()
        {
            if(ready)
            {
                Ray aimRay = HydraUtils.GetProjectileAimVector(mainCam, firePoint, 0.2f);

                GameObject newBrick = GameObject.Instantiate(thrownBrickPrefab, firePoint.position, Quaternion.identity);
                newBrick.transform.forward = aimRay.direction;
                if(newBrick.TryGetComponent<Rigidbody>(out Rigidbody rb))
                {
                    if(!thrownBricks.Contains(rb))
                    {
                        thrownBricks.Add(rb);
                        HydraUtils.PlayAudioClip(whoosh,UnityEngine.Random.Range(0.6f,0.9f));
                    }

                    rb.velocity = aimRay.direction * force;
                }

                if(newBrick.TryGetComponent<ThrownBrick>(out ThrownBrick brick))
                {
                    brick.SetBrickGun(this);
                }
            }
        }

        private Vector3 flockPosition = new Vector3();

        public Vector3 GetPointPosition()
        {
            return flockPosition;
        }


        private void OnEnable()
        {
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
    }

}