using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace UltraFunGuns
{
    public class UltraBullet : MonoBehaviour, IUFGInteractionReceiver, ICleanable
    {
        [SerializeField] private Transform superchargeFX;
        
        [SerializeField] private Transform thrustFX, fallFX;


        private float maxPower;
        public float Power { get; private set; }
        public float PowerDecayRate = 30f;

        public float Damage = 1.2f;

        public bool Falling { get; private set; }
        public bool Supercharged { get; private set; }

        public bool IsDivision { get; private set; }

        private float divisionScale => (transform.localScale.x + transform.localScale.y + transform.localScale.z/3);

        private UltraGun originWeapon;

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


        private void Start()
        {
            rb = GetComponent<Rigidbody>();
        }


        public void SetPower(float power)
        {
            Power = power;
            if(Power > maxPower)
            {
                maxPower = Power;
            }
        }

        public void SetOriginWeapon(UltraGun gun)
        {
            originWeapon = gun;
        }

        public void Divide(int divideInto = 3)
        {
            if (divideInto < 2 || IsDivision)
                return;
            Vector3 storedVelocity = Rigidbody.velocity;
            Fall();
            Vector3 ringCenter = transform.position;

            Ring ring = new Ring(divideInto, divideInto*2f);

            HydraLogger.Log($"Ring rad {ring.Radius}", DebugChannel.Warning);

            float objectRadius = GetComponent<CapsuleCollider>().radius/divideInto;

            ring.SetCircumferenceFromObjectRadius(objectRadius);

            Vector3[] ringPositions = ring.GetPositions();
            for(int i=0; i< ringPositions.Length; i++)
            {
                ringPositions[i] = transform.rotation * ringPositions[i];
            }

            //ringPositions.Do(x => { x = transform.rotation * x; });
            float currentSpeed = storedVelocity.magnitude;

            transform.localScale /= divideInto;
            transform.position = ringCenter+ringPositions[0];
            Rigidbody.velocity = storedVelocity;
            IsDivision = true;

            HydraLogger.Log($"Bullet divided from {gameObject.name}", DebugChannel.Warning);

            for (int i = 1; i < ringPositions.Length; i++)
            {
                UltraBullet newBullet = Instantiate(gameObject, ringPositions[i]+ringCenter, transform.rotation).GetComponent<UltraBullet>();
                newBullet.IsDivision = true;
                newBullet.rb = newBullet.GetComponent<Rigidbody>();
                newBullet.rb.velocity = storedVelocity + ((newBullet.transform.position - ringCenter).normalized * (currentSpeed * 0.25f));
            }

            Rigidbody.velocity += (transform.position - ringCenter).normalized * (currentSpeed * 0.25f);
        }

        public void Fall()
        {
            if (Falling)
                return;

            if (Supercharged)
                Supercharged = false;

            Falling = true;

            if (thrustFX != null)
                thrustFX.gameObject.SetActive(false);

            if (superchargeFX != null)
                superchargeFX.gameObject.SetActive(false);

            if (fallFX != null)
                fallFX.gameObject.SetActive(true);
            
            originWeapon = null;

            Instantiate(Prefabs.BlackSmokeShockwave, thrustFX.position, Quaternion.Inverse(transform.rotation));

            gameObject.AddComponent<DestroyAfterTime>().TimeLeft = 25.0f;
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

            if (Power > 0.0f && !Falling)
                Rigidbody.velocity = transform.forward * ((originWeapon != null) ? Power : maxPower);

            transform.forward = Rigidbody.velocity;

            if (originWeapon != null)
            {
                SetPower(originWeapon.GetPower(this));
            }else
            {
                Power -= Time.fixedDeltaTime * PowerDecayRate;
                Power = Mathf.Max(0, Power);
            }   
        }

        private void LateUpdate()
        {
            if(!Falling && Power <= 0.0f)
            {
                Fall();
            }
        }

        public void Explode()
        {
            if (Exploded)
                return;

            Exploded = true;

            GameObject explosion = Prefabs.UK_Explosion.Asset;

            //mini Explosion
           
            if(Supercharged) //Lightning
            {
                explosion = Prefabs.UK_MindflayerExplosion.Asset;
            }

            if (IsDivision)
            {
                explosion = Prefabs.ShittyExplosionFX;
            }

            GameObject newExplosion = Instantiate(explosion, transform.position, Quaternion.identity);
            
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

            if(col.IsCollisionEnemy(out EnemyIdentifier eid))
            {
                float damageValue = (IsDivision) ? Damage * divisionScale : Damage;
                eid.DeliverDamage(eid.gameObject, -col.GetContact(0).normal * col.relativeVelocity.magnitude * 100.0f, col.GetContact(0).point, damageValue, true, 0, (originWeapon != null) ? originWeapon.gameObject : null);
            }

            Explode();
        }

        public void Shot(BeamType beamType)
        {
            TimeController.Instance.ParryFlash();

            if (beamType == BeamType.Railgun)
            {
                SetDirection(CameraController.Instance.transform.forward);
                Supercharge();
                return;
            }

            if(beamType == BeamType.MaliciousFace)
            {
                SetDirection(CameraController.Instance.transform.forward);
                Divide(8);
                return;
            }

            Explode();
        }

        public void Supercharge()
        {
            if (Supercharged)
                return;

            Supercharged = true;

            originWeapon = null;
            Falling = false;

            SetPower(150.0f);

            if (thrustFX != null)
                thrustFX.gameObject.SetActive(false);

            if (superchargeFX != null)
                superchargeFX.gameObject.SetActive(true);

            if (fallFX != null)
                fallFX.gameObject.SetActive(false);

            Instantiate(Prefabs.BlackSmokeShockwave, thrustFX.position, Quaternion.LookRotation(-transform.forward, Vector3.up));

            Destroy(GetComponent<DestroyAfterTime>());
        }

        public bool Parried(Vector3 aimVector)
        {
            SetDirection(aimVector);
            if(Falling)
            {
                Divide(4);
                //Supercharge();
            }
            return true;
        }

        public bool Interact(UFGInteractionEventData interaction)
        {
            if(interaction.ContainsAnyTag("electricity"))
            {
                SetDirection(interaction.direction);
                Supercharge();
                return true;
            }

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

        private void OnDestroy()
        {
            Explode();
        }

        public bool Targetable(TargetQuery query)
        {
            if (Exploded)
                return false;

            return query.CheckTargetable(transform.position);
        }

        public void Cleanup()
        {
            Explode();
        }
    }
}
