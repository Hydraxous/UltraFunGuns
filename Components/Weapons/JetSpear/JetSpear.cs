using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    [WeaponInfo("JetSpear", "Jet Spear", 3, true, WeaponIconColor.Yellow)]
    public class JetSpear : UltraFunGunBase
    {

        public float jetForce = 1.5f, fuelRemaining = 0.75f, maxFuel = 0.75f, smoothing, smoothingSpeed = 0.15f, upforce = 150.0f, reboundDampen = 0.25f;

        private Rigidbody rb;

        private bool jetEnabled;

        public override void OnAwakeFinished()
        {
            rb = player.rb;
        }

        public override void GetInput()
        {
            if(MonoSingleton<InputManager>.Instance.InputSource.Fire1.IsPressed && fuelRemaining > 0.0f)
            {
                fuelRemaining = Mathf.Clamp(fuelRemaining - Time.deltaTime, 0.0f, maxFuel);
                smoothing = Mathf.Clamp(smoothing + Time.deltaTime, 0.0f, smoothingSpeed);
                jetEnabled = true;
            }else if(smoothing > 0.0f)
            {
                smoothing = Mathf.Clamp(smoothing - Time.deltaTime, 0.0f, smoothingSpeed);
                jetEnabled = true;
            }
            else
            {
                fuelRemaining = Mathf.Clamp(fuelRemaining + Time.deltaTime, 0.0f, maxFuel);
                jetEnabled = false;
            }

            if(MonoSingleton<InputManager>.Instance.InputSource.Fire2.WasPerformedThisFrame)
            {
                float speed = rb.velocity.magnitude * reboundDampen;
                Vector3 movementVector = -mainCam.forward;
                movementVector += Vector3.up * upforce;
                movementVector = movementVector.normalized * speed;
                rb.velocity = movementVector;
            }

        }

        private void FixedUpdate()
        {
            if(jetEnabled)
            {
                //Debug.Log("Applyin force!");
                rb.velocity += ((mainCam.forward) * jetForce) * (smoothing/smoothingSpeed);
            }
        }

        protected override void DoAnimations()
        {

        }
    }
}
