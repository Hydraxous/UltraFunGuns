using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using UnityEngine;
using UltraFunGuns.Datas;

namespace UltraFunGuns
{
    //Throwable projectile that bounces off of things, does damage to enemies and multiplies it's velocity every time it hits something. Should be really funny.
    //Also can force push/pull it with right click
    [UFGWeapon("Dodgeball", "ULTRABALLER", 1, true, WeaponIconColor.Red)]
    [WeaponAbility("Recall", "Pull the thrown ball back to you by holing <color=orange>Fire 2</color>", 2, RichTextColors.lime)]
    [WeaponAbility("Soft-Ball", "Press <color=orange>Fire 2</color> while holding the ball to throw the ball softly.",1, RichTextColors.aqua)]
    [WeaponAbility("Full-Ball", "Press <color=orange>Fire 1</color> to throw the ball. Holding the button will throw the ball faster and harder.", 0, RichTextColors.aqua)]
    [WeaponAbility("Excite", "Firing upon or punching the ball will send it into a fit of excitement.", 3, RichTextColors.red)]
    public class Dodgeball : UltraFunGunBase
    {
        ActionCooldown pullCooldown = new ActionCooldown(0.25f, true);

        public ThrownDodgeball activeDodgeball;

        [UFGAsset("ThrownDodgeball")] public static GameObject ThrownDodgeballPrefab { get; private set; }

        private GameObject catcherCollider;

        private bool throwingBall = false;
        private bool pullingBall = false;
        private bool chargingBall = false;

        public bool dodgeBallActive = false;
        public float throwForce = 90.0f;
        public float softThrowForce = 30.0f;

        public float pullForce = 150.0f;

        private float chargeMultiplier = 1.42f;

        private float minCharge = 1.0f;
        private float currentCharge = 1.0f;
        private float maxCharge = 4.0f;

        private float pullTimer = 0.0f;

        public bool basketBallMode = false;
        private Material standardSkin;
        [UFGAsset("BasketballMaterial")] private static Material basketballSkin;

        //TODO optiminzation
        public override void OnAwakeFinished()
        {
            basketBallMode = Data.Config.Data.BasketBallMode;
            standardSkin = transform.Find("viewModelWrapper/Armature/Upper_Arm/Forearm/Hand/DodgeballMesh").GetComponent<MeshRenderer>().material;
            SetSkin(!basketBallMode);
            catcherCollider = transform.Find("firePoint/DodgeballCatcher").gameObject;
            catcherCollider.SetActive(false);
            catcherCollider.gameObject.layer = 14;
        }

        //press fire1 to throw, hold it to charge, hold right click to recall ball towards you.
        public override void GetInput()
        {   
            if (!om.paused)
            {
                //primary Charge input
                if (MonoSingleton<InputManager>.Instance.InputSource.Fire1.IsPressed && !throwingBall && !pullingBall && !dodgeBallActive)
                {
                    chargingBall = true;
                    currentCharge = Mathf.Clamp(currentCharge + (Time.deltaTime * chargeMultiplier), minCharge, maxCharge);
                }
                else if (chargingBall && !throwingBall && !pullingBall)
                {
                    StartCoroutine(ThrowDodgeball(false));
                    pullCooldown.AddCooldown(0.5f);
                }

                //Soft throw
                if(MonoSingleton<InputManager>.Instance.InputSource.Fire2.WasPerformedThisFrame && !throwingBall && !pullingBall && !dodgeBallActive && !chargingBall)
                {
                    StartCoroutine(ThrowDodgeball(true));
                    pullCooldown.AddCooldown(0.5f);
                }

                //Force push/pull ball
                if (MonoSingleton<InputManager>.Instance.InputSource.Fire2.IsPressed && !chargingBall && !throwingBall && dodgeBallActive && pullCooldown.CanFire())
                {
                    ForceDodgeball(true);
                    pullTimer += Time.deltaTime;
                }
                else if (pullingBall && dodgeBallActive)
                {
                    pullCooldown.AddCooldown();
                    ForceDodgeball(false);
                }else
                {
                    pullTimer = 0.0f;
                    pullingBall = false;
                    catcherCollider.SetActive(false);
                }


                if(WeaponManager.SecretButton.WasPerformedThisFrame)
                {
                    DoSecret();
                }

                if(Input.GetKeyDown(KeyCode.Equals) && !chargingBall && !throwingBall && dodgeBallActive && UltraFunGuns.DebugMode)
                {
                    activeDodgeball.ExciteBall();
                }
            }
        }

        public override void DoSecret()
        {
            if (Data.SaveInfo.Data.basketballHighScore < 100) //If its unlocked :)
                return;

            basketBallMode = !basketBallMode;
            Data.Config.Data.BasketBallMode = basketBallMode;
            Data.Config.Save();
            SetSkin(basketBallMode);
        }

        private void SetSkin(bool standard)
        {
            Material newSkin = !standard ? basketballSkin : standardSkin;
            if (newSkin != null)
            {
                transform.Find("viewModelWrapper/Armature/Upper_Arm/Forearm/Hand/DodgeballMesh").GetComponent<MeshRenderer>().material = newSkin;
                ThrownDodgeballPrefab.transform.Find("DodgeballMesh").GetComponent<MeshRenderer>().material = newSkin;
            }
        }

        private void FixedUpdate()
        {
            if (dodgeBallActive && activeDodgeball == null)
            {
                dodgeBallActive = false;
            }

            if (dodgeBallActive)
            {
                activeDodgeball.beingPulled = pullingBall; //Currently you can command the ball to you by swapping weapons when pulling, if this should be changed in the future set these to false in OnDisable
            }

            if(pullTimer >= 6.0f && dodgeBallActive) //Failsafe for missing ball to return to player.
            {
                CatchBall();
                Destroy(activeDodgeball.gameObject);            
            }
        }

        //Secondary fire action when the ball is in play

        private void ForceDodgeball(bool pull)
        {
            if (activeDodgeball != null)
            {
                Vector3 forceVelocity = (catcherCollider.transform.position - activeDodgeball.gameObject.transform.position).normalized * pullForce;
                MonoSingleton<CameraController>.Instance.CameraShake(0.025f);

                if (!pull)
                {
                    forceVelocity *= -1;
                }
                pullingBall = pull;
                catcherCollider.SetActive(pull);
                activeDodgeball.transform.forward = forceVelocity;
                activeDodgeball.SetSustainVelocity(forceVelocity, pull);
            }else
            {
                dodgeBallActive = false;
            }
        }

        IEnumerator ThrowDodgeball(bool softThrow, bool skipTiming = false)
        {
            throwingBall = true;
            chargingBall = false;
            animator.Play("DodgeballThrow");

            if(!skipTiming) //Lines up the thrown ball with the animation, looks nice.
            {
                yield return new WaitForSeconds(0.15f);
            }

            MonoSingleton<CameraController>.Instance.CameraShake(0.35f);

            dodgeBallActive = true;
            activeDodgeball = GameObject.Instantiate<GameObject>(ThrownDodgeballPrefab, firePoint.position, Quaternion.identity).GetComponent<ThrownDodgeball>();
            activeDodgeball.dodgeballWeapon = this;

            Ray ballDirection = HydraUtils.GetProjectileAimVector(mainCam, firePoint);
            activeDodgeball.gameObject.transform.forward = mainCam.forward;

            float currentThrowForce = throwForce;
            if (softThrow)
            {
                currentThrowForce = softThrowForce;
                currentCharge = 1.0f;
            }

            Vector3 dodgeBallVelocity = (ballDirection.direction * currentThrowForce) * currentCharge;
            activeDodgeball.SetSustainVelocity(dodgeBallVelocity, true);
            if(!softThrow)
            {
                activeDodgeball.AddBounces((int)(4 * currentCharge));
            }
            throwingBall = false;
            currentCharge = 0;
        }

        //Called by ThrownDodgeball object when it touches the catcher trigger.
        public void CatchBall()
        {
            MonoSingleton<CameraController>.Instance.CameraShake(0.35f);
            pullingBall = false;
            pullCooldown.TimeToFire = 0;
            pullTimer = 0.0f;
            animator.Play("DodgeballCatchball");
            transform.Find("Audios/CatchSound").GetComponent<AudioSource>().Play();
        }

        protected override void DoAnimations()
        {
            animator.SetBool("ChargingBall", chargingBall);
            animator.SetBool("PullingBall", pullingBall);
            animator.SetBool("CantThrow", dodgeBallActive);
        }

        private void OnDisable()
        {
            if (chargingBall && !throwingBall && !pullingBall) //Does not work since coroutine cant run on disabled obj too bad! WONTFIX
            {
                StartCoroutine(ThrowDodgeball(false,true));
            }

            if(dodgeBallActive)
            {
                activeDodgeball.beingPulled = false;
            }

            throwingBall = false;
            chargingBall = false;
            pullingBall = false;
        }
        
        private void OnDestroy()
        {
            if(dodgeBallActive && activeDodgeball != null)
            {
                activeDodgeball.Pop();
            }
        }

        public override string GetDebuggingText()
        {
            string debug = base.GetDebuggingText();
            debug += $"BALL_OUT: {dodgeBallActive}\n";
            if (dodgeBallActive)
                debug += $"BALL_LEVEL: {activeDodgeball.timesExcited}\n";
            debug += $"THROW_CHARGE: {currentCharge}\n";
            debug += $"PULL_TIMER: {pullTimer}\n";
            debug += $"PULL_CD: {pullCooldown}\n";
            debug += $"BBALL: {basketBallMode}\n";
            return debug;
        }
    }
}
