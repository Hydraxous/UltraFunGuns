using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace UltraFunGuns
{
    public class UltraBullet : MonoBehaviour, IUFGInteractionReceiver
    {
        [SerializeField] private Transform thrustFX, fallFX;

        private float maxPower;
        public float Power { get; private set; }
        public float PowerDecayRate = 30f;

        private Rigidbody rb;
        public Rigidbody Rigidbody 
        {
            get
            {
                if (rb == null)
                {
                    rb = GetComponent<Rigidbody>();
                }
                return rb;
            }
        }

        public bool Exploded { get; private set; }

        public void SetPower(float power)
        {
            Power = power;
            if(Power > maxPower)
            {
                maxPower = Power;
            }
        }

        public void SetDirection(Vector3 direction)
        {
            if (Rigidbody == null)
                return;

            transform.forward = direction;
            Rigidbody.velocity = direction.normalized * Rigidbody.velocity.magnitude;
        }

        private void FixedUpdate()
        {
            if (Rigidbody == null)
                return;


            if (Power > 0.0f)
                Rigidbody.velocity = transform.forward * maxPower;

            transform.forward = Rigidbody.velocity;


            Power -= Time.fixedDeltaTime * PowerDecayRate;
            Power = Mathf.Max(0, Power);
        }

        private void LateUpdate()
        {
            if (thrustFX != null)
                thrustFX.gameObject.SetActive(Power > 0.0f);

            if(fallFX != null)
                fallFX.gameObject.SetActive(Power <= 0.0f);

        }

        public void Explode()
        {
            if (Exploded)
                return;

            Exploded = true;
            Instantiate(Prefabs.UK_Explosion.Asset, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }

        private UltraFunGunBase.ActionCooldown collisionCD = new UltraFunGunBase.ActionCooldown(0.05f);

        private void OnCollisionEnter(Collision col)
        {
            if (!collisionCD.CanFire() || Exploded)
                return;

            if (!((LayerMaskDefaults.Get(LMD.EnemiesAndEnvironment) & (1 << col.gameObject.layer)) != 0))
            {
                return;
            }

            Explode();
        }

        public void Shot(BeamType beamType)
        {
            TimeController.Instance.ParryFlash();
            Explode();
        }

        public bool Parried(Vector3 aimVector)
        {
            return false;
        }

        public bool Interact(UFGInteractionEventData interaction)
        {
            if (interaction.ContainsAnyTag("shot", "explode"))
            {
                TimeController.Instance.ParryFlash();
                Explode();
                return true;
            }

            return false;
        }

        public Vector3 GetPosition()
        {
            return transform.position;
        }
    }
}
