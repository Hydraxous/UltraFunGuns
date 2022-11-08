using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    public class CanExplosion : MonoBehaviour
    {
        public int explosionType;

        public GameObject bulletTrail;
        public Transform[] explosionFX;

        public int penetrations = 2;

        public float forceMultiplier = 1.0f;
        public float shrapnelDamage = 1.0f;
        public float aoeDamage = 0.15f;
        public float aoeRange = 1.0f;

        public float maxShrapnelRange = 200.0f;
        public float shrapnelSize = 0.25f;

        public int explosionSize = 0;

        private CanExplosionProfile[] explosionProfiles = new CanExplosionProfile[] {
            null,
            new CanExplosionProfile(0.15f, 24, 5.0f, false),
            new CanExplosionProfile(0.2f, 64, 3.0f, false),
            new CanExplosionProfile(0.3f, 128, 1.0f, true),
        };

        /* Strengths
         * 0 - dies normal - tiny aoe damage
         * 1 - parried after enemyBonk - shrapnel spread
         * 2 - parried after other can Hit - bigger denser shrapnel spread
         * 3 - shot with railgun - 360 shrapnel spread
         */

        private void Awake()
        {
            List<Transform> newFXs = new List<Transform>();
            for(int i =0;i<4;i++)
            {
                newFXs.Add(transform.Find(string.Format("FX/{0}", i)));
                newFXs[i].gameObject.SetActive(false);
            }

            explosionFX = newFXs.ToArray();
        }

        public void Explode(int size)
        {
            explosionSize = size;
            explosionFX[size].gameObject.SetActive(true);
            DoBoom();
            if(explosionSize > 0)
            {
                DoRays(explosionProfiles[explosionSize]);
            }
        }

        private void DoBoom()
        {

        }

        private void DoRays(CanExplosionProfile profile)
        {
            for(int i=0;i<profile.rayAmount; i++)
            {
                Vector3 direction = Vector3.zero;
                if(profile.sphere)
                {
                    direction = UnityEngine.Random.insideUnitSphere;
                }
                else
                {
                    Vector2 rand = UnityEngine.Random.insideUnitCircle;
                    rand *= Mathf.Sign(UnityEngine.Random.Range(-1.0f, 1.0f));

                    direction = transform.TransformDirection(new Vector3(rand.x, rand.y, profile.rayChoke));
                }

                Shoot(new Ray(transform.position,direction));

            }
        }

        private void Shoot(Ray direction)
        {
            bool penetration = true;

            List<EnemyIdentifier> hitEnemies = new List<EnemyIdentifier>();

            RaycastHit[] hits = Physics.SphereCastAll(direction, shrapnelSize, maxShrapnelRange, LayerMask.GetMask("Limb", "BigCorpse", "Outdoors", "Environment", "Default", "Projectile"));
            hits = HydraUtils.SortRaycastHitsByDistance(hits);
            if (hits.Length > 0)
            {
                if (!(hits.Length == 1 && hits[0].collider.gameObject.name == "CameraCollisionChecker"))
                {
                    int endingHit = 0;
                    hits = HydraUtils.SortRaycastHitsByDistance(hits);
                    for (int i = 0; i < hits.Length; i++)
                    {
                        Ray hitRay = new Ray();
                        hitRay.origin = transform.position;
                        hitRay.direction = hits[i].point - transform.position;

                        if (hitEnemies.Count > penetrations)
                        {
                            penetration = false;
                        }

                        endingHit = i;

                        if ((hits[i].collider.gameObject.layer == 24 || hits[i].collider.gameObject.layer == 25 || hits[i].collider.gameObject.layer == 8))
                        {
                            break;
                        }

                        if (hits[i].collider.gameObject.TryGetComponent<EnemyIdentifierIdentifier>(out EnemyIdentifierIdentifier enemyIDID))
                        {
                            if (!hitEnemies.Contains(enemyIDID.eid))
                            {
                                hitEnemies.Add(enemyIDID.eid);
                                enemyIDID.eid.DeliverDamage(hits[i].collider.gameObject, hitRay.direction * forceMultiplier, hits[i].point, shrapnelDamage, true, 0.0f, this.gameObject);
                            }
                            if (!penetration)
                            {
                                break;
                            }
                        }
                        else if (hits[i].collider.gameObject.TryGetComponent<EnemyIdentifier>(out EnemyIdentifier enemyID))
                        {
                            if (!hitEnemies.Contains(enemyID))
                            {
                                hitEnemies.Add(enemyID);
                                enemyID.DeliverDamage(hits[i].collider.gameObject, hitRay.direction * forceMultiplier, hits[i].point, shrapnelDamage, true, 0.0f, this.gameObject);
                            }
                            if (!penetration)
                            {
                                break;
                            }
                        }

                        if (hits[i].collider.gameObject.TryGetComponent<Breakable>(out Breakable breakable))
                        {
                            breakable.Break();
                            if (!penetration)
                            {
                                break;
                            }
                        }

                        if (hits[i].collider.gameObject.TryGetComponent<ThrownEgg>(out ThrownEgg egg))
                        {
                            egg.Explode(10.0f);
                            if (!penetration)
                            {
                                break;
                            }
                        }

                        if (hits[i].collider.gameObject.TryGetComponent<ThrownDodgeball>(out ThrownDodgeball dodgeBall))
                        {
                            dodgeBall.ExciteBall(1);
                            if (!penetration)
                            {
                                break;
                            }
                        }

                        if (hits[i].collider.gameObject.TryGetComponent<Grenade>(out Grenade grenade))
                        {
                            MonoSingleton<TimeController>.Instance.ParryFlash();
                            grenade.Explode();
                            if (!penetration)
                            {
                                break;
                            }
                        }

                        if (hits[i].collider.gameObject.TryGetComponent<Projectile>(out Projectile projectile))
                        {
                            projectile.Explode();
                            if (!projectile.friendly)
                            {
                                MonoSingleton<TimeController>.Instance.ParryFlash();
                                MonoSingleton<StyleHUD>.Instance.AddPoints(10, "hydraxous.ultrafunguns.fingergunprojhit", this.gameObject, null);
                            }
                            if (!penetration)
                            {
                                break;
                            }
                        }
                    }
                    CreateBulletTrail(transform.position, hits[endingHit].point, hits[endingHit].normal, hits[endingHit].collider.transform);
                }
                else//todo clean this.
                {
                    Ray missray = new Ray();
                    missray.origin = transform.position;
                    missray.direction = transform.TransformDirection(0, 0, 1);

                    Vector3 endPoint = missray.GetPoint(maxShrapnelRange);
                    CreateBulletTrail(transform.position, endPoint, missray.direction * -1);
                }
            }
            else
            {
                Ray missray = new Ray();
                missray.origin = transform.position;
                missray.direction = transform.TransformDirection(0, 0, 1);

                Vector3 endPoint = missray.GetPoint(maxShrapnelRange);
                CreateBulletTrail(transform.position, endPoint, missray.direction * -1);
            }

            List<EnemyIdentifier> hitListCopy = new List<EnemyIdentifier>();
            foreach (EnemyIdentifier enemee in hitEnemies)
            {
                if (!enemee.dead && !enemee.exploded)
                {
                    hitListCopy.Add(enemee);
                }
            }
            hitEnemies = hitListCopy;

            if (hitEnemies.Count > penetrations)
            {
                //MonoSingleton<TimeController>.Instance.HitStop(0.10f);
                //MonoSingleton<StyleHUD>.Instance.AddPoints(250, "hydraxous.ultrafunguns.fingergunfullpenetrate", this.gameObject, null);
            }
            else if (hitEnemies.Count > 0)
            {
                //MonoSingleton<StyleHUD>.Instance.AddPoints(10 * hitEnemies.Count, "hydraxous.ultrafunguns.fingergunhit", this.gameObject, null, hitEnemies.Count);
            }
        }

        private void CreateBulletTrail(Vector3 startPosition, Vector3 endPosition, Vector3 normal)
        {
            GameObject newBulletTrail = Instantiate<GameObject>(bulletTrail, endPosition, Quaternion.identity);
            newBulletTrail.transform.up = normal;
            LineRenderer line = newBulletTrail.GetComponent<LineRenderer>();
            Vector3[] linePoints = new Vector3[2] { startPosition, endPosition };
            line.SetPositions(linePoints);
        }

        private void CreateBulletTrail(Vector3 startPosition, Vector3 endPosition, Vector3 normal, Transform parent)
        {
            GameObject newBulletTrail = Instantiate<GameObject>(bulletTrail, endPosition, Quaternion.identity);
            newBulletTrail.transform.up = normal;
            LineRenderer line = newBulletTrail.GetComponent<LineRenderer>();
            Vector3[] linePoints = new Vector3[2] { startPosition, endPosition };
            line.SetPositions(linePoints);
            newBulletTrail.transform.parent = parent;
        }

        public class CanExplosionProfile
        {
            public float rayWidth;
            public int rayAmount;
            public float rayChoke;
            public bool sphere;
            public CanExplosionProfile(float rayWidth, int rayAmount, float rayChoke, bool sphere)
            {
                this.rayWidth = rayWidth;
                this.rayAmount = rayAmount;
                this.rayChoke = rayChoke;
                this.sphere = sphere;
            }
            
        }
    }
}
