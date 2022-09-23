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

        public bool refracting = false;
        public Focalyzer focalyzer;
        public FocalyzerLaserController pylonManager;
        public LineRenderer refractedLaser;
        public FocalyzerPylon targetPylon;

        private Vector3 randomizedDirection = Vector3.forward;

        UltraFunGunBase.ActionCooldown damageCooldown = new UltraFunGunBase.ActionCooldown(0.25f);
        UltraFunGunBase.ActionCooldown randomDirectionCooldown = new UltraFunGunBase.ActionCooldown(0.025f);
        UltraFunGunBase.ActionCooldown pylonChecker = new UltraFunGunBase.ActionCooldown(0.25f);
        UltraFunGunBase.ActionCooldown sphereDamageCooldown = new UltraFunGunBase.ActionCooldown(0.05f);

        public LayerMask laserHitMask;

        public int refractionCount = 0;
        public float AOERadius = 3.5f;

        private float lifeTime = 16.0f;
        private float lifeTimeLeft = 0.0f;

        public bool disco = false;

        private AudioSource discoAudio;

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
        }

        //Checks if the laser is being fired already to prevent buildup of unwanted coroutines.
        public void DoRefraction(FocalyzerPylon hitPylon, bool playerLaser = false)
        {
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
                if (pylonChecker.CanFire())
                {
                    pylonChecker.AddCooldown();
                    try
                    {
                        targetPylon = pylonManager.GetRefractorTarget(pylonHit);
                    }
                    catch(Exception e)
                    {
                        //TODO FIX THIS IDK WHAT IS CAUSING IT.
                    }          
                }
                FireLaser();

                yield return new WaitForEndOfFrame();
            }
            refracting = false;
        }

        //Executes a laser hit on given information. These lasers DO penetrate through enemies, grenades, etc, but do not work if line of sight is broken. UPDATE: los check is weird idfk
        private bool LaserHit(RaycastHit hit, Vector3 castDirection, float damageMultiplier, float critMultiplier = 0, bool tryExplode = false)
        {
            if (hit.collider.gameObject.layer == 24 || hit.collider.gameObject.layer == 25 || hit.collider.gameObject.layer == 8 || hit.collider.gameObject.layer == 0)
            {
                return false;
            }

            if (hit.collider.gameObject.TryGetComponent<Breakable>(out Breakable breakable))
            {
                breakable.Break();
            }

            if (hit.collider.gameObject.TryGetComponent<ThrownEgg>(out ThrownEgg egg))
            {
                egg.Explode(8.0f);
            }

            if (hit.collider.gameObject.TryGetComponent<Grenade>(out Grenade grenade))
            {
                grenade.Explode();
            }

            if (hit.collider.gameObject.TryGetComponent<EnemyIdentifierIdentifier>(out EnemyIdentifierIdentifier Eii))
            {
                if (damageCooldown.CanFire())
                {
                    damageCooldown.AddCooldown();
                    Eii.eid.DeliverDamage(Eii.eid.gameObject, castDirection, hit.point, damageMultiplier, tryExplode, critMultiplier);
                }
            }
            return true;
        }

        //Controls the laser of the pylon. Deals AOE and big damage to enemies caught in the beam.
        public void FireLaser()
        {
            //AOE Damage
            RaycastHit[] sphereHits = Physics.SphereCastAll(transform.position, AOERadius, transform.position);
            if (sphereHits.Length > 0)
            {
                foreach(RaycastHit sphereHit in sphereHits)
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
                        if(!Eii.eid.dead)
                        {
                            sphereDamageCooldown.AddCooldown();
                            Vector3 damageDirection = Eii.eid.gameObject.transform.position - transform.position;
                            Eii.eid.DeliverDamage(Eii.eid.gameObject, damageDirection, Eii.eid.gameObject.transform.position, 0.35f, false);
                        }
                    }
                }
                
            }


            if (targetPylon != this)
            {
                Vector3 laserPath = targetPylon.transform.position - transform.position;
                RaycastHit[] hits = Physics.RaycastAll(transform.position, laserPath, laserPath.magnitude, laserHitMask);
                foreach (RaycastHit hit in hits)
                {  
                    if(!LaserHit(hit, laserPath, 0.3f))
                    {
                        break;
                    }
                }
                targetPylon.DoRefraction(this);
                Vector3[] laserPoints = new Vector3[] { transform.position, targetPylon.transform.position };
                BuildLaser(laserPoints, laserPath * -1);
            }
            else if(targetPylon == this) //TODO this goes in a random direction so fix it. Update: it's a feature :^) TODO: make the speed of the direction change go from slow to fast overtime for balancing.
            {
                if(randomDirectionCooldown.CanFire()) //delay between picking a direction
                {
                    randomDirectionCooldown.AddCooldown();
                    float randX = UnityEngine.Random.Range(-1f, 1f);
                    float randY = UnityEngine.Random.Range(-1f, 1f);
                    float randZ = UnityEngine.Random.Range(-1f, 1f);
                    randomizedDirection = transform.TransformDirection(randX, randY, randZ).normalized;
                    randomizedDirection *= 500;
                }
                
                RaycastHit[] hits = Physics.RaycastAll(transform.position, randomizedDirection, randomizedDirection.sqrMagnitude, laserHitMask);

                if (hits.Length > 0) //if nothing is hit it will fire the laser downward in worldspace.
                {
                    hits = focalyzer.SortHitsByDistance(hits);
                    int counter = -1;
                    foreach (RaycastHit hit in hits)
                    {
                        ++counter;
                        if (!LaserHit(hit, randomizedDirection, 6.0f/pylonManager.GetPylonCount(), 2.0f, true))
                        {
                            break;
                        }
                    }
                    Vector3[] laserPoints = new Vector3[] { transform.position, hits[counter].point };
                    BuildLaser(laserPoints, hits[counter].normal);
                }else
                {
                    if(Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), out RaycastHit hit, Mathf.Infinity, laserHitMask))
                    {
                        LaserHit(hit, randomizedDirection, 3.0f/pylonManager.GetPylonCount(), 1.0f, true);
                        BuildLaser(new Vector3[] { transform.position, hit.point }, hit.normal);
                    }
                    else
                    {
                        Vector3 towardsPlayer = MonoSingleton<CameraController>.Instance.transform.position - transform.position;
                        BuildLaser(new Vector3[] { transform.position, transform.position }, towardsPlayer);
                    }
                }
                
            }else
            {
                targetPylon = pylonManager.GetRefractorTarget(this);
            }
        }

        //Constructs the laser visually. Doesn't actually do anything mechanically.
        void BuildLaser(Vector3[] points, Vector3 normal)
        {
            refractedLaser.SetPositions(points);
            refractedLaser.gameObject.transform.position = points[1];
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
