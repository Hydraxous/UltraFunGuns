using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    [WeaponAbility("Discharge", "Launch physics objects <color=orange>Fire 1</color>", 0, RichTextColors.aqua)]
    [WeaponAbility("Grabbity Well", "Pull physics objects <color=orange>Fire 2</color>", 1, RichTextColors.aqua)]
    [UFGWeapon("GrabbityGun", "Grabbity Gun (Experimental)", 3, true, WeaponIconColor.Yellow, false)]
    public class GrabbityGun : UltraFunGunBase
    {
        public float forceOnRb = 150.0f;
        public float forwardBias = 5.0f;
        public float groupForce = 2.0f;
        public float groupMaxRange = 10.0f;

        public bool setVelo = false;

        public float maxRange = 10.0f;
        public float minAngle = 0.60f; //45 angle


        private ActionCooldown primaryFireDelay = new ActionCooldown(0.4f, true), secondaryDelay = new ActionCooldown(0.2f, true);

        private bool grabbiting = false;

        private List<EnemyOverride> enemyOverrides = new List<EnemyOverride>();

        public override void GetInput()
        {
            if (primaryFireDelay.CanFire() && InputManager.Instance.InputSource.Fire1.WasPerformedThisFrame)
            {
                primaryFireDelay.AddCooldown();
                FirePrimary();
            }

            if (WeaponManager.SecretButton.WasPerformedThisFrame)
            {
                setVelo = !setVelo;
            }
        }

        void FixedUpdate()
        {
            bool fire2Pressed = InputManager.Instance.InputSource.Fire2.IsPressed;
            if (secondaryDelay.CanFire())
            {
                if (fire2Pressed)
                {
                    grabbiting = true;
                    SuckInStuff();
                }
                else if (grabbiting)
                {
                    ResetEnemies();
                    secondaryDelay.AddCooldown();
                }
            }
        }

        private void SuckInStuff()
        {
            //CameraController.Instance.CameraShake(0.25f);
            Vector3 startPosition = mainCam.position;
            Vector3 forwardOffset = mainCam.forward;
            startPosition += (forwardOffset * maxRange);

            Visualizer.DrawLine(0.02f, mainCam.position, startPosition);

            Collider[] hitColliders = Physics.OverlapSphere(startPosition, groupMaxRange);
            //Visualizer.DrawSphere(startPosition, groupMaxRange, 0.02f);

            for (int i = 0; i < hitColliders.Length; i++)
            {
                Pull(hitColliders[i], startPosition);
            }
        }

        private void ResetEnemies()
        {
            if(enemyOverrides.Count <= 0)
            {
                return;
            }

            foreach(EnemyOverride enemyOverride in enemyOverrides)
            {
                if(enemyOverride.Enemy == null || enemyOverride == null)
                {
                    continue;
                }

                if(enemyOverride.Enemy.dead)
                {
                    continue;
                }

                enemyOverride.SetRagdoll(false);
            }

            enemyOverrides.Clear();
        }

        public override void FirePrimary()
        {
            Vector3 startPosition = mainCam.transform.position;
            Vector3 forwardOffset = mainCam.transform.rotation * Vector3.forward;
            startPosition += forwardOffset*maxRange;

            Collider[] hitColliders = Physics.OverlapSphere(startPosition, maxRange);
            //Visualizer.DrawSphere(startPosition, maxRange, 1.0f);
            CameraController.Instance.CameraShake(0.0075f * forceOnRb);

            for (int i = 0; i < hitColliders.Length; i++)
            {
                DoHit(hitColliders[i], startPosition);
            }
        }

        private void Pull(Collider collider, Vector3 checkOrigin)
        {

            if (!collider.TryGetComponent<Rigidbody>(out Rigidbody rigidbody))
            {
                return;
            }

            if (rigidbody == NewMovement.Instance.rb)
            {
                return;
            }

            Vector3 camPos = mainCam.position;
            Vector3 pullDirection = Vector3.zero;

            if (collider.IsColliderEnemy(out EnemyIdentifier eid))
            {
                EnemyOverride enemyOverride = eid.Override();
                if (!enemyOverrides.Contains(enemyOverride))
                {
                    enemyOverrides.Add(enemyOverride);
                    enemyOverride.SetRagdoll(true);
                    enemyOverride.AddCollisionEvent((col) =>
                    {

                        if (!enemyOverrides.Contains(enemyOverride))
                        {
                            return;
                        }

                        switch (col.gameObject.layer)
                        {
                            case 0:
                            case 8:
                                TryDamage();
                                break;
                            default:
                                break;
                        }

                        void TryDamage()
                        {
                            if (!eid.CanBeDamaged())
                            {
                                return;
                            }

                            float impactForce = col.relativeVelocity.magnitude;

                            if (impactForce < 5.0f)
                            {
                                return;
                            }

                            eid.DeliverDamage(eid.gameObject, col.relativeVelocity, col.GetContact(0).point, impactForce, false, 0.0f, gameObject);
                        }
                    });
                }
            }



            pullDirection = checkOrigin - (rigidbody.centerOfMass + rigidbody.transform.position);


            if (setVelo)
            {
                rigidbody.velocity = pullDirection * groupForce;
            }
            else
            {
                rigidbody.velocity += pullDirection * groupForce;
            }

            Visualizer.DrawRay(rigidbody.transform.position + rigidbody.centerOfMass, rigidbody.velocity, 0.5f);

        }

        private void DoHit(Collider collider, Vector3 checkOrigin)
        {
            Vector3 camPos = mainCam.position;
            Vector3 camForward = mainCam.forward;

            Vector3 hitPoint = collider.ClosestPointOnBounds(checkOrigin);
            Vector3 camToHitPoint = hitPoint - camPos;

            float angle = Vector3.Dot(camToHitPoint.normalized, camForward);

            if( angle < minAngle)
            {
                return;
            }

            if(!collider.TryGetComponent<Rigidbody>(out Rigidbody rigidbody))
            {
                return;
            }


            Vector3 shootRay = camToHitPoint.normalized + (camForward.normalized * forwardBias);

            shootRay = shootRay.normalized * forceOnRb;

            if (collider.TryGetComponent<Projectile>(out Projectile projectile))
            {        
                projectile.transform.forward = shootRay;
                projectile.speed *= forceOnRb * 0.01f;
            }

            rigidbody.AddForceAtPosition(shootRay, hitPoint, ForceMode.Impulse);

            
        }

    }
}
