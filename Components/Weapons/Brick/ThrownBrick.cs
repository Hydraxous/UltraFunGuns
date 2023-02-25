using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

namespace UltraFunGuns
{
    public class ThrownBrick : MonoBehaviour
    {
        private Brick brickShooter;
        private Rigidbody rb;

        private UltraFunGunBase.ActionCooldown soundCooldown = new UltraFunGunBase.ActionCooldown(0.05f);

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        public void SetBrickGun(Brick brickGun)
        {
            brickShooter = brickGun;
        }

        public void Update()
        {
            CheckFlock();
        }

        private void CheckFlock()
        {
            if (brickShooter == null || rb == null)
            {
                return;
            }

            if (!brickShooter.StormActive)
            {
                return;
            }

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

        private void OnCollisionEnter(Collision col)
        {
            CheckHit(col);

            if (collisionSound != null && soundCooldown.CanFire())
            {
                soundCooldown.AddCooldown();

                Vector3 forceDir = col.relativeVelocity;
                float force = forceDir.magnitude;

                if(force > 4.0f)
                {
                    HydraUtils.PlayAudioClip(collisionSound, col.GetContact(0).point, UnityEngine.Random.Range(0.8f, 1.1f), 1.0f, 1.0f);
                }
            }
        }

        private void CheckHit(Collision col)
        {
            EnemyIdentifier enemy = null;

            if (col.gameObject.TryGetComponent<EnemyIdentifierIdentifier>(out EnemyIdentifierIdentifier enemyPart))
            {
                enemy = enemyPart.eid;
            }
            else
            {
                col.gameObject.TryGetComponent<EnemyIdentifier>(out enemy);
            }

            if (enemy != null)
            {
                if (!enemy.dead)
                {
                    enemy.DeliverDamage(enemy.gameObject, rb.velocity, col.GetContact(0).point, 1.0f, true, 1.0f, brickShooter.gameObject);
                    HydraUtils.PlayAudioClip(collisionSound, UnityEngine.Random.Range(0.8f, 1.1f), 1.0f);

                }
            }
        }
    }
}
