using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{

    //Pylon of the focalyzer alternate. It will target what the player does.
    public class FocalyzerPylonAlternate : MonoBehaviour
    {
        //public Animator animator;
        public Animator laserAnimator;

        public FocalyzerAlternate focalyzer;
        public LineRenderer refractedLaser;

        public Transform targetPoint;
        public Transform laserOriginPoint;

        UltraFunGunBase.ActionCooldown damageCooldown = new UltraFunGunBase.ActionCooldown(0.25f);
        UltraFunGunBase.ActionCooldown pylonChecker = new UltraFunGunBase.ActionCooldown(0.25f);
        UltraFunGunBase.ActionCooldown sphereDamageCooldown = new UltraFunGunBase.ActionCooldown(0.05f);

        public LayerMask laserHitMask;

        public int refractionCount = 0;
        public float laserBeamWidth = 0.1f;

        private float lifeTime = 12.0f;
        private float lifeTimeLeft = 0.0f;

        private enum LaserHitType {enemy, nothing, solid, interactable}

        void Start()
        {
            lifeTimeLeft = lifeTime + Time.time;
            refractedLaser = GetComponentInChildren<LineRenderer>();
            laserAnimator = refractedLaser.GetComponent<Animator>();
            laserOriginPoint = transform.Find("FocalyzerPylonRemake/RefractorVisual");
            laserOriginPoint.gameObject.AddComponent<AlwaysLookAtCamera>().speed = 0.0f;
        }

        void Update()
        {
            laserAnimator.SetBool("Active", true);
            if (lifeTimeLeft < Time.time)
            {
                Shatter();
            }

            FireLaser();
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

            Vector3[] laserPoints = new Vector3[2] { laserOriginPoint.position, Vector3.zero};
            Vector3 laserEndNormal;

            if (endHitIndex > -1 && hits[endHitIndex].collider.name != gameObject.name)
            {
                laserPoints[1] = hits[endHitIndex].point;
                laserEndNormal = hits[endHitIndex].normal;
            }
            else
            {
                Ray missRay = new Ray();
                missRay.origin = laserOriginPoint.position;
                missRay.direction = laserPath;

                laserPoints[1] = missRay.GetPoint(focalyzer.laserMaxRange);
                laserEndNormal = laserPath * -1;
            }

            BuildLaser(laserPoints, laserEndNormal);

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

        void OnDestroy()
        {
            focalyzer.OnPylonDeath();
        }
    }
}
