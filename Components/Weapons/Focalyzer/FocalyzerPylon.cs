using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    public class FocalyzerPylon : MonoBehaviour
    {
        public Animator animator;
        public bool refracting = false;
        public Focalyzer focalyzer;
        public FocalyzerLaserController pylonManager;
        public FocalyzerPylon targetPylon;

        UltraFunGunBase.ActionCooldown damageCooldown = new UltraFunGunBase.ActionCooldown(0.25f);

        public int refractionCount = 0;

        private float lifeTime = 8.0f;
        private float lifeTimeLeft = 0.0f;

        void Start()
        {
            lifeTimeLeft = lifeTime + Time.time;
            animator = GetComponent<Animator>();
            transform.Find("FocalyzerCrystalVisual/RefractorVisual").gameObject.AddComponent<AlwaysLookAtCamera>().speed = 0.0f;
            pylonManager.AddPylon(this);
        }

        void Update()
        {
            animator.SetBool("Refracting", refracting);
            if (lifeTimeLeft < Time.time || !pylonManager.gameObject.activeInHierarchy)
            {
                Shatter();
            }


        }

        public bool Refract(Vector3 origin, Vector3 hitPoint)
        {
            pylonManager.AddLinePosition(origin);
            ++refractionCount;
            refracting = true;

            FocalyzerPylon pylon = pylonManager.GetRefractorTarget(this);
            if (pylon != null && pylon != this)
            {
                Vector3 refractDirection = pylon.gameObject.transform.position - transform.position;
                RaycastHit[] hits = Physics.RaycastAll(transform.position, refractDirection, refractDirection.magnitude, 117460224);
                foreach (RaycastHit hit in hits)
                {

                    if (hit.collider.gameObject.TryGetComponent<ThrownEgg>(out ThrownEgg egg))
                    {
                        egg.Explode();
                    }

                    if (hit.collider.gameObject.TryGetComponent<Grenade>(out Grenade grenade))
                    {
                        grenade.Explode();
                    }

                    if (hit.collider.gameObject.TryGetComponent<EnemyIdentifierIdentifier>(out EnemyIdentifierIdentifier Eii))
                    {
                        if(damageCooldown.CanFire())
                        {
                            damageCooldown.AddCooldown();
                            Eii.eid.DeliverDamage(Eii.eid.gameObject, refractDirection, hit.point, 0.025f, false);
                        }
                    }

                    return pylon.Refract(transform.position, pylon.transform.position);
                }
                
            }

            if(pylon == this)
            {
                pylonManager.AddLinePosition(hitPoint);
                Vector3 pylonNormal = MonoSingleton<NewMovement>.Instance.transform.position - hitPoint;
                pylonManager.BuildLine(pylonNormal);
                return true;
            }else
            {
                refractionCount = 0;
                refracting = false;
                return false;
            }
        }

        void Shatter()
        {
            Destroy(this);
        }

        void OnDisable()
        {
            pylonManager.RemovePylon(this);
        }

        void OnDestroy()
        {
            pylonManager.RemovePylon(this);
        }
    }
}
