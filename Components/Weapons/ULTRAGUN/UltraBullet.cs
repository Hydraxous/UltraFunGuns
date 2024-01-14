using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using UltraFunGuns.Util;
using UnityEngine;
using UnityEngine.UI;

namespace UltraFunGuns
{
    public class UltraBullet : MonoBehaviour, IUFGInteractionReceiver, ICleanable, IUFGBeamInteractable
    {
        //I hate Unity I hate Unity I hate Unity I hate Unity I hate Unity I hate Unity I hate Unity I hate Unity I hate Unity 
        private Transform thrustFX => ubrf.thrustFX;
        private Transform fallFX => ubrf.fallFX;
        private Transform parryThrustFX => ubrf.parryThrustFX;
        private MeshRenderer bulletMesh => ubrf.bulletMesh;
        private Material easterEggMaterial => ubrf.easterEggMaterial;

        /*
        * Literally the most annoying bug I've ever experienced with Unity. It cost me hours of my sanity, so grab some popcorn and read my story.
        * For some reason, Unity DLL reference serialization completely sucks. It's the worst. I have no idea why, but for some reason Unity REFUSES to serialize
        * the parryThrustFX field for this component. This issue is not present for any other components in the entire mod. The only solution I have found to fixing it
        * is to rename the field to something completely random, then roll a d20 and pray for a Nat 20. Only then, will the reference serialize and I will
        * actually be able to build the assetbundle and use the reference in-game. Otherwise, it just breaks! So... Thanks Unity. <3
        */

        [SerializeField] private UltraBulletReferenceFix _ubrf;

        private UltraBulletReferenceFix ubrf
        {
            get
            {
                if(_ubrf == null)
                    _ubrf = GetComponent<UltraBulletReferenceFix>();

                return _ubrf;
            }
        }

        private float maxPower;
        public float Power { get; private set; }

        [Configgy.Configgable("UltraFunGuns/Weapons/UltraGun/UltraBullet")] 
        public static float PowerDecayRate = 30f;

        [Configgy.Configgable("UltraFunGuns/Weapons/UltraGun/UltraBullet")]
        public static float Damage = 1.2f;

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

        private Vector3 lastVelocity;
        private Vector3 startDirection;
        public bool Exploded { get; private set; }

        private bool isMortar = false;


        private void Start()
        {
            if(bulletMesh != null)
            {
                if(UnityEngine.Random.Range(0, 100) == 0)
                {
                    bulletMesh.material = easterEggMaterial;
                }
            }
            rb = GetComponent<Rigidbody>();
            startDirection = transform.forward;
            isMortar = Vector3.Dot(startDirection, Vector3.up) > 0.75f;
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

            HydraLogger.Log($"Bullet divided from {gameObject.name}");

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

            if (parryThrustFX != null)
                parryThrustFX.gameObject.SetActive(false);

            if (fallFX != null)
                fallFX.gameObject.SetActive(true);
            
            originWeapon = null;

            Instantiate(Prefabs.BlackSmokeShockwave, (thrustFX == null) ? transform.position : thrustFX.position , Quaternion.Inverse(transform.rotation));

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

            Visualizer.DrawRay(transform.position + (Rigidbody.velocity.normalized * 0.4f), Rigidbody.velocity * Time.fixedDeltaTime, 0.0166f);

            //Reflect when hitting armor (this does NOT work for some reason) TODO
            if (Physics.Raycast(transform.position + (Rigidbody.velocity.normalized * 0.4f), Rigidbody.velocity * Time.fixedDeltaTime, out RaycastHit hit, Rigidbody.velocity.magnitude * Time.fixedDeltaTime, LayerMask.GetMask("Armor", "Water")))
            {
                Visualizer.DrawSphere(hit.point, 0.5f, 5.0f);
                HydraLogger.Log("Hit amror");
                Vector3 newDirection = Vector3.Reflect(Rigidbody.velocity, hit.normal);
                SetDirection(newDirection);
            }

            if (Power > 0.0f && !Falling)
                Rigidbody.velocity = transform.forward * ((originWeapon != null) ? Power : maxPower);

            transform.forward = Rigidbody.velocity;
            lastVelocity = Rigidbody.velocity;

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

            if (Supercharged) //Lightning
            {
                explosion = Prefabs.UK_MindflayerExplosion.Asset;

                VirtualExplosion virtualExplosion = new VirtualExplosion(transform.position, 20f);
                EnemyIdentifier[] enemiesInRange = virtualExplosion.GetAffectedEnemies();
                for (int i = 0; i < enemiesInRange.Length; i++)
                {
                    Debug.LogWarning($"Enemyinrange_{i}");
                    enemiesInRange[i].Override().AddStyleEntryOnDeath(new StyleEntry(30, "ultragunsuperchargekill", 1.0f));
                    Vector3 enemyTargetPoint = enemiesInRange[i].GetTargetPoint();
                    enemiesInRange[i].DeliverDamage(enemiesInRange[i].gameObject, enemyTargetPoint - transform.position * 200.0f, enemyTargetPoint, 0.4f, true, 0.4f, (originWeapon) ? originWeapon.gameObject : null);
                }
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

            if (col.IsCollisionEnemy(out EnemyIdentifier eid))
            {
                HitEnemy(eid, col);
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

            if (parryThrustFX.gameObject != null)
                parryThrustFX.gameObject.SetActive(true);

            if (fallFX != null)
                fallFX.gameObject.SetActive(false);

            Instantiate(Prefabs.BlackSmokeShockwave, thrustFX.position, Quaternion.LookRotation(-transform.forward, Vector3.up));

            Destroy(GetComponent<DestroyAfterTime>());
        }

        private void HitEnemy(EnemyIdentifier eid, Collision col)
        {
            float crit = (Supercharged) ? 1.5f : 0.0f;
            float damageValue = (IsDivision) ? Damage * divisionScale : Damage;
            damageValue = (Supercharged) ? damageValue * 1.5f : damageValue;   

            
            if(Falling && Vector3.Dot(Vector3.down, lastVelocity) > 0.75f && Vector3.Dot(startDirection, Vector3.down) > 0.75f)
            {
                eid.Override().AddStyleEntryOnDeath(new StyleEntry(40, "ultragunaerialkill", 1.0f));
            }
            else
            {
                eid.Override().AddStyleEntryOnDeath(new StyleEntry(25, "ultragunkill", 2.0f));
            }

            if (Falling && Vector3.Dot(Vector3.down, lastVelocity) > 0.85f && isMortar && (Vector3.Dot(col.GetContact(0).normal, Vector3.up) > 0.25f)) //Bullet was shot into the air and landed on an enemies head.
            {
                WeaponManager.AddStyle(new StyleEntry(1000, "ultragunrainkill"));
                damageValue *= 1000.0f;
                eid.Explode();
                return;
            }

            eid.DeliverDamage(eid.gameObject, -col.GetContact(0).normal * col.relativeVelocity.magnitude * 100.0f, col.GetContact(0).point, damageValue, true, crit, (originWeapon != null) ? originWeapon.gameObject : null);

        }

        public bool Parry(Vector3 origin, Vector3 aimVector)
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
            if(interaction.ContainsAnyTag("electricity", "god", "sonic"))
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

        public void OnRevolverBeamHit(RevolverBeam beam, ref RaycastHit hit)
        {
            switch (beam.beamType)
            {
                case BeamType.Railgun:
                    SetDirection(transform.position - beam.transform.position);
                    Supercharge();
                    break;

                default:
                    TimeController.Instance.ParryFlash();
                    Explode();
                    break;
            }
        }

        public bool CanRevolverBeamHit(RevolverBeam beam, ref RaycastHit hit)
        {
            return !Exploded;
        }

        public bool CanRevolverBeamPierce(RevolverBeam beam)
        {
            switch(beam.beamType)
            {
                case BeamType.Railgun:
                    return true;
                default:
                    return false;
            }
        }
    }
}
