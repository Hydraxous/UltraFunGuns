using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    //Throwable projectile that bounces off of things, does damage to enemies and multiplies it's velocity every time it hits something. Should be really funny.
    public class Dodgeball : UltraFunGunBase
    {
        public ThrownDodgeball activeDodgeball;

        public GameObject thrownDodgeballPrefab;

        private SphereCollider catcherCollider;

        private bool throwingBall = false;
        private bool pullingBall = false;
        private bool chargingBall = false;

        public bool dodgeBallActive = false;
        public float throwForce = 50.0f;

        private float pullForce = 130.0f;

        private float chargeMultiplier = 0.952f;

        private float minCharge = 1.0f;
        private float currentCharge = 1.0f;
        private float maxCharge = 3.0f;

        public override void OnAwakeFinished()
        {
            HydraLoader.prefabRegistry.TryGetValue("ThrownDodgeball", out thrownDodgeballPrefab);
            weaponIcon.variationColor = 1;
            catcherCollider = firePoint.transform.Find("DodgeballCatcher").GetComponent<SphereCollider>();
            catcherCollider.enabled = false;
        }

        //press fire1 to throw, hold it to charge, hold right click to recall ball towards you.
        public override void GetInput()
        {
            if(!om.paused)
            {
                if (MonoSingleton<InputManager>.Instance.InputSource.Fire1.IsPressed && !throwingBall && !pullingBall && !dodgeBallActive)
                {
                    chargingBall = true;
                    currentCharge = Mathf.Clamp(currentCharge + (Time.deltaTime * chargeMultiplier), minCharge, maxCharge);
                }
                else if (chargingBall && !throwingBall && !pullingBall && !om.paused)
                {
                    StartCoroutine(ThrowDodgeball());
                }


                if (MonoSingleton<InputManager>.Instance.InputSource.Fire2.IsPressed && !throwingBall && dodgeBallActive)
                {
                    ForceDodgeball(true);
                }
                else if (pullingBall && dodgeBallActive)
                {
                    ForceDodgeball(false);
                }

            }
        }

        private void ForceDodgeball(bool pull)
        {
            Vector3 forceVelocity = (catcherCollider.transform.position - activeDodgeball.gameObject.transform.position).normalized * pullForce;

            if (!pull)
            {
                forceVelocity *= -1;
            }
            pullingBall = pull;
            catcherCollider.enabled = pull;
            activeDodgeball.sustainedVelocity = forceVelocity;
        }

        IEnumerator ThrowDodgeball()
        {
            throwingBall = true;
            chargingBall = false;
            currentCharge = 0;
            animator.Play("DodgeballThrow");
            yield return new WaitForSeconds(0.15f);
            dodgeBallActive = true;
            activeDodgeball = GameObject.Instantiate<GameObject>(thrownDodgeballPrefab, firePoint.position, Quaternion.identity).GetComponent<ThrownDodgeball>();
            activeDodgeball.dodgeballWeapon = this;
            Ray ballDirection = new Ray();
            activeDodgeball.gameObject.transform.forward = mainCam.transform.forward;
            if (Physics.Raycast(mainCam.transform.position, mainCam.TransformDirection(0, 0, 1), out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("Projectile", "Limb", "BigCorpse", "Environment", "Outdoors", "Armor", "Default")))
            {
                ballDirection.origin = firePoint.position;
                ballDirection.direction = hit.point - firePoint.position;
            }else
            {
                ballDirection.origin = mainCam.transform.position;
                ballDirection.direction = mainCam.TransformDirection(0, 0, 1);
            }
            Vector3 dodgeBallVelocity = (ballDirection.direction * throwForce) * currentCharge;
            activeDodgeball.sustainedVelocity = dodgeBallVelocity;
            throwingBall = false;
            
        }

        public void CatchBall()
        {
            pullingBall = false;
            animator.Play("DodgeballCatchball");     
        }

        public override void DoAnimations()
        {
            animator.SetBool("ChargingBall", chargingBall);
            animator.SetBool("PullingBall", pullingBall);
            animator.SetBool("CantThrow", dodgeBallActive);
        }

        private void OnDisable()
        {
            throwingBall = false;
            chargingBall = false;
            pullingBall = false;
        }

        private void OnEnable()
        {
            //animator.Play("DodgeballEquipAny");
        }
    }
}
