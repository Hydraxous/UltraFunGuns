using System;
using System.Collections;
using UnityEngine;

namespace UltraFunGuns
{

    //Pylon of the focalyzer. When shot by a player using a focalyzer it will attempt to refract the laser to deal more damage.
    /*PLANNED:
     Shatter if punched.
     Move towards player if hit with grapple.
         */
    public class FocalyzerPylon : MonoBehaviour, IUFGInteractionReceiver, ICleanable, IRevolverBeamShootable
    {
        [UFGAsset("FocalyzerPylonShatterFX_Red")] public static GameObject PylonShatterFX { get; private set; }

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
        public float parryForce = 200.0f;

        private float lifeTime = 16.0f;
        private float lifeTimeLeft = 0.0f;

        public bool disco = false;
        private bool dying = false;
        private AudioSource discoAudio;

        private Rigidbody rb;

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
            rb = GetComponent<Rigidbody>();
        }

        void Update()
        {
            animator.SetBool("Refracting", refracting);
            laserAnimator.SetBool("Active", refracting);
            if (lifeTimeLeft < Time.time || focalyzer == null)
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
                egg.Explode();
                hitType = LaserHitType.interactable;
            }

            if (hit.collider.gameObject.TryGetComponent<Grenade>(out Grenade grenade))
            {
                grenade.Explode();
                hitType = LaserHitType.interactable;
            }

            if (hit.collider.gameObject.TryGetComponent<EnemyIdentifierIdentifier>(out EnemyIdentifierIdentifier Eii))
            {
                if (damageCooldown.CanFire())
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
            RaycastHit[] sphereHits = Physics.SphereCastAll(transform.position, AOERadius, transform.position);
            if (sphereHits.Length > 0)
            {
                foreach(RaycastHit sphereHit in sphereHits)
                {
                    if (sphereHit.collider.gameObject.TryGetComponent<ThrownEgg>(out ThrownEgg egg))
                    {
                        egg.Explode(); //TODO CHANGE
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
                            ++enemyHits;
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
                    switch(LaserHit(hit, laserPath, 0.3f, 0, false))
                    {
                        case LaserHitType.solid:
                            goto StopLoop;
                        case LaserHitType.enemy:
                            enemyHits++;
                            break;
                        case LaserHitType.nothing:
                            break;
                        case LaserHitType.interactable:
                            interactableHits++;
                            break;
                    }
                }
                StopLoop:
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
                    hits = HydraUtils.SortRaycastHitsByDistance(hits);
                    int counter = -1;
                    foreach (RaycastHit hit in hits)
                    {
                        ++counter;
                        switch(LaserHit(hit, randomizedDirection, 6.0f/pylonManager.GetPylonCount(), 2.0f, true))
                        {
                        case LaserHitType.solid:
                            goto StopLoop;
                        case LaserHitType.enemy:
                            discoHits++;
                            break;
                        case LaserHitType.nothing:
                            break;
                        case LaserHitType.interactable:
                            discoHits++;
                            break;
                        }
                    }
                    StopLoop:
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
                
            }else if(targetPylon == null)
            {
                targetPylon = pylonManager.GetRefractorTarget(this);
            }

            if(discoHits > 0)
            {
                MonoSingleton<StyleHUD>.Instance.AddPoints(15, "hydraxous.ultrafunguns.discohit", focalyzer.gameObject, null, discoHits);
            }

            if (enemyHits > 0 || interactableHits > 0)
            {
                MonoSingleton<StyleHUD>.Instance.AddPoints(2 + (50*(interactableHits/1)), "hydraxous.ultrafunguns.refractionhit", focalyzer.gameObject, null, enemyHits + interactableHits);

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
            if (dying)
                return;
            

            dying = true;
            Instantiate(PylonShatterFX, transform.position, Quaternion.identity);
            Prefabs.BonusBreakSound.Asset.PlayAudioClip(transform.position, 1.1f, 1.0f, 0.6f);
            Destroy(gameObject);
        }

        //Removes itself from the global pylon list when it dies.
        void OnDisable()
        {
            if (pylonManager != null)
            {
                pylonManager.RemovePylon(this);
            }
        }

        void OnDestroy()
        {
            if(pylonManager != null)
            {
                pylonManager.RemovePylon(this);
            }
        }


        public bool Interact(UFGInteractionEventData interaction)
        {
            string invoker = interaction.invokeType.Name;
            if(invoker != "Focalyzer" && invoker != "FocalyzerAlternate")
            {
                Shatter();
                return true;
            }

            return false;
        }

        public bool Parry(Vector3 origin, Vector3 aimVector)
        {
            rb.AddForce(aimVector.normalized * parryForce, ForceMode.Impulse);
            return true;
        }

        public Vector3 GetPosition()
        {
            return transform.position;
        }

        public bool Targetable(TargetQuery targetQuery)
        {
            return false;
        }
        public void Cleanup()
        {
            Shatter();
        }

        public void OnRevolverBeamHit(RevolverBeam beam, ref RaycastHit hit)
        {
            Shatter();
        }

        public bool CanRevolverBeamHit(RevolverBeam beam, ref RaycastHit hit)
        {
            return true;
        }
    }
}
