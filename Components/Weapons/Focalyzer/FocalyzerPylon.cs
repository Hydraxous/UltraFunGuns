using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{

    //Pylon of the focalyzer. When shot by a player using a focalyzer it will attempt to refract the laser to deal more damage.
    /*PLANNED:
     Shatter if punched.
     Move towards player if hit with grapple.
         */
    public class FocalyzerPylon : MonoBehaviour
    {
        public Animator animator;
        public Animator laserAnimator;

        public bool refracting = true;
        public Focalyzer focalyzer;
        public FocalyzerLaserController pylonManager;
        public LineRenderer refractedLaser;
        public FocalyzerPylon targetPylon;

        public Transform targetPoint;

        private Vector3 randomizedDirection = Vector3.forward;

        UltraFunGunBase.ActionCooldown damageCooldown = new UltraFunGunBase.ActionCooldown(0.25f);
        UltraFunGunBase.ActionCooldown randomDirectionCooldown = new UltraFunGunBase.ActionCooldown(0.025f);
        UltraFunGunBase.ActionCooldown pylonChecker = new UltraFunGunBase.ActionCooldown(0.25f);
        UltraFunGunBase.ActionCooldown sphereDamageCooldown = new UltraFunGunBase.ActionCooldown(0.05f);

        public LayerMask laserHitMask;

        public int refractionCount = 0;
        public float AOERadius = 3.5f;
        public float laserBeamWidth = 0.1f;

        private float lifeTime = 16.0f;
        private float lifeTimeLeft = 0.0f;

        public bool disco = false;

        private AudioSource discoAudio;

        private enum LaserHitType {enemy, nothing, solid, interactable}

        void Start()
        {
            discoAudio = transform.Find("DiscoAudio").gameObject.GetComponent<AudioSource>();
            lifeTimeLeft = lifeTime + Time.time;
            animator = GetComponent<Animator>();
            refractedLaser = GetComponentInChildren<LineRenderer>();
            laserAnimator = refractedLaser.GetComponent<Animator>();
            transform.Find("FocalyzerCrystalVisual/RefractorVisual").gameObject.AddComponent<AlwaysLookAtCamera>().speed = 0.0f;
            pylonManager.AddPylon(this);
            disco = (UnityEngine.Random.Range(0.0f, 100.0f) <= 5.0f);
            discoAudio.Play();
        }

        void Update()
        {
            animator.SetBool("Refracting", refracting);
            laserAnimator.SetBool("Active", refracting);
            if (lifeTimeLeft < Time.time)
            {
                Shatter();
            }

            if(disco && refracting && targetPylon == this)
            {
                discoAudio.UnPause();
            }else
            {
                discoAudio.Pause();
            }

            FireLaser();
        }

        //Checks if the laser is being fired already to prevent buildup of unwanted coroutines.
        public void DoRefraction(FocalyzerPylon hitPylon, bool playerLaser = false)
        {
            return; //TODO remove this whole thing and fix integration
            if(!refracting)
            {
                StartCoroutine(RefractLaser(hitPylon, playerLaser));
            }  
        }

        //Fires a laser while the player is shooting it with the Focalyzer.
        IEnumerator RefractLaser(FocalyzerPylon pylonHit, bool playerLaser = false)
        {
            refracting = true;
            while (focalyzer.hittingAPylon)
            {
                try
                {
                    if (pylonChecker.CanFire())
                    {
                        pylonChecker.AddCooldown();
                        targetPylon = pylonManager.GetRefractorTarget(pylonHit);
                    }
                    FireLaser();
                }
                catch (Exception e)
                {
                    //TODO FIX THIS IDK WHAT IS CAUSING IT.
                }

                yield return new WaitForEndOfFrame();
            }
            refracting = false;
        }

        //Executes a laser hit on given information. These lasers DO penetrate through enemies, grenades, etc, but do not work if line of sight is broken. UPDATE: los check is weird idfk
        private LaserHitType LaserHit(RaycastHit hit, Vector3 castDirection, float damageMultiplier, float critMultiplier = 0, bool tryExplode = false)
        {
            LaserHitType hitType = LaserHitType.nothing;
            if (hit.collider.gameObject.layer == 24 || hit.collider.gameObject.layer == 25 || hit.collider.gameObject.layer == 8 || hit.collider.gameObject.layer == 0)
            {
                return LaserHitType.solid;
            }

            if (hit.collider.gameObject.TryGetComponent<Breakable>(out Breakable breakable))
            {
                breakable.Break();        
            }

            if (hit.collider.gameObject.TryGetComponent<ThrownEgg>(out ThrownEgg egg))
            {
                egg.Explode(8.0f);
                hitType = LaserHitType.interactable;
            }

            if (hit.collider.gameObject.TryGetComponent<Grenade>(out Grenade grenade))
            {
                grenade.Explode();
                hitType = LaserHitType.interactable;
            }

            if (hit.collider.gameObject.TryGetComponent<EnemyIdentifierIdentifier>(out EnemyIdentifierIdentifier Eii))
            {
                if (damageCooldown.CanFire() && !Eii.eid.dead)
                {
                    damageCooldown.AddCooldown();
                    Eii.eid.DeliverDamage(Eii.eid.gameObject, castDirection, hit.point, damageMultiplier, tryExplode, critMultiplier);
                    hitType = LaserHitType.enemy;
                }
            }
            return hitType;
        }

        //Controls the laser of the pylon. Deals AOE and big damage to enemies caught in the beam.
        public void FireLaser()
        {
            int enemyHits = 0;
            int interactableHits = 0;
            int discoHits = 0;

            //AOE Damage
            RaycastHit[] sphereHits = Physics.SphereCastAll(transform.position, AOERadius, Vector3.zero, 0.001f);
            if (sphereHits.Length > 0)
            {
                foreach (RaycastHit sphereHit in sphereHits)
                {
                    if (sphereHit.collider.gameObject.TryGetComponent<ThrownEgg>(out ThrownEgg egg))
                    {
                        egg.Explode(1.0f); //TODO CHANGE
                    }

                    if (sphereHit.collider.gameObject.TryGetComponent<Grenade>(out Grenade grenade))
                    {
                        grenade.Explode();
                    }

                    if (sphereHit.collider.gameObject.TryGetComponent<EnemyIdentifierIdentifier>(out EnemyIdentifierIdentifier Eii) && sphereDamageCooldown.CanFire())
                    {
                        if (!Eii.eid.dead)
                        {
                            sphereDamageCooldown.AddCooldown();
                            Vector3 damageDirection = Eii.eid.gameObject.transform.position - transform.position;
                            Eii.eid.DeliverDamage(Eii.eid.gameObject, damageDirection, Eii.eid.gameObject.transform.position, 0.35f, false);
                            ++enemyHits;
                        }
                    }
                }

            }

            int endHitIndex = -1;
            Vector3 laserPath = targetPoint.transform.position - transform.position;
            RaycastHit[] hits = Physics.SphereCastAll(transform.position, laserBeamWidth, laserPath, focalyzer.laserMaxRange, laserHitMask);
            hits = HydraUtils.SortRaycastHitsByDistance(hits);
            foreach (RaycastHit hit in hits)
            {
                ++endHitIndex;
                if(LaserHit(hit, laserPath, 0.3f, 0, false) != LaserHitType.nothing)
                {
                    break;
                }
            }

            transform.forward = laserPath.normalized;

            Vector3[] laserPoints = new Vector3[2] { transform.position, Vector3.zero};
            Vector3 laserEndNormal;
            if (hits.Length > 0)
            {
                laserPoints[1] = hits[endHitIndex].point;
                laserEndNormal = hits[endHitIndex].normal;
            }
            else
            {
                Ray missRay = new Ray();
                missRay.origin = transform.position;
                missRay.direction = laserPath;

                laserPoints[1] = missRay.GetPoint(focalyzer.laserMaxRange);
                laserEndNormal = laserPath * -1;
            }

            BuildLaser(laserPoints, laserEndNormal);

            if (discoHits > 0)
            {
                //MonoSingleton<StyleHUD>.Instance.AddPoints(15, "hydraxous.ultrafunguns.discohit", focalyzer.gameObject, null, discoHits);
            }

            if (enemyHits > 0 || interactableHits > 0)
            {
                //MonoSingleton<StyleHUD>.Instance.AddPoints(2 + (50 * (interactableHits / 1)), "hydraxous.ultrafunguns.refractionhit", focalyzer.gameObject, null, enemyHits + interactableHits);
            }
        }

        //Constructs the laser visually. Doesn't actually do anything mechanically.
        void BuildLaser(Vector3[] laser, Vector3 normal)
        {
            refractedLaser.SetPositions(laser);
            refractedLaser.gameObject.transform.position = laser[1];
            refractedLaser.gameObject.transform.up = normal;
        }

        //TODO break animation
        void Shatter()
        {
            Destroy(gameObject);
        }

        //Removes itself from the global pylon list when it dies.
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
