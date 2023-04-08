using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace UltraFunGuns
{
    public class RemoteBombExplosive : MonoBehaviour, IUFGInteractionReceiver
    {
        [UFGAsset("RemoteBomb_Explosive_Explosion")] public static GameObject ExplosionPrefab { get; private set; }

        private RemoteBomb weapon;
        private NewMovement player;
        private EnemyIdentifier stuckTarget;
        private Rigidbody rb;
        private Renderer indicatorLight;

        public float armTime = 0.05f;
        public float blinkInterval = 0.75f;
        public float blinkTime = 0.05f;
        public float parryForce = 150.0f;
        public float playerKnockbackMultiplier = 70.0f;
        public float rigidbodyForceMultiplier = 70.0f;
        public float explosionRadius = 6f;
        public float explosionDamage = 1.6f;
        public float explosionChainDelay = 0.20f;

        public float stuckDamageMultiplier = 1.5f;
        public float stickDamage = 0.05f;

        private bool thrown = false;
        private bool armed = false;
        private bool landed = false;
        private bool alive = true;

        private bool pushedPlayer = false;

        private AudioSource AC_armingBeep, AC_ambientBeep;

        private static Vector3 lastExplosionPosition = Vector3.zero;
        List<RemoteBombExplosive> chainedExplosives = new List<RemoteBombExplosive>();


        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            indicatorLight = transform.Find("BombMesh/Blinker").GetComponent<Renderer>();
            AC_armingBeep = transform.Find("Audios/Arm_Beep").GetComponent<AudioSource>();
            AC_ambientBeep = transform.Find("Audios/Beep").GetComponent<AudioSource>();
        }

        private void Start()
        {
            Events.OnPlayerDeath += () => Detonate(true);
        }

        public void Initiate(RemoteBomb remoteBomb, NewMovement newMovement)
        {
            this.weapon = remoteBomb;
            this.player = newMovement;
            Thrown();
        }

        private void Update()
        {
            if(alive && thrown)
            {
                if(weapon == null)
                {
                    Detonate(true);
                }
            }
        }

        public void SetVelocity(Vector3 newVelocity)
        {
            if (rb)
            {
                rb.velocity = newVelocity;
            }
        }

        public bool Parriable()
        {
            return (!landed && !armed);
        }

        public bool Parried(Vector3 direction)
        {
            if(!landed && !armed)
            {
                Arm();
                SetVelocity(direction.normalized * parryForce);
                return true;
            }

            return false;
        }

        private void Thrown()
        {
            thrown = true;
            StartCoroutine(ArmExplosive());
            StartCoroutine(DoIndicator());
        }

        private IEnumerator ArmExplosive()
        {
            float timer = armTime;
            while(timer > 0.0f && !armed)
            {
                timer -= Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
            Arm();
        }

        private IEnumerator DoIndicator()
        {
            if(indicatorLight != null)
            {
                while (alive)
                {
                    if (!armed)
                    {
                        indicatorLight.material.SetColor("_EmissiveColor", Color.green);
                        yield return new WaitForEndOfFrame();
                    }
                    else
                    {
                        float timer = blinkInterval;
                        indicatorLight.material.SetColor("_EmissiveColor", Color.black);

                        while (timer > 0.0f)
                        {
                            timer -= Time.deltaTime;
                            yield return new WaitForEndOfFrame();
                        }

                        timer = blinkTime;
                        indicatorLight.material.SetColor("_EmissiveColor", Color.red);
                        AC_ambientBeep.Play();

                        while (timer > 0.0f)
                        {
                            timer -= Time.deltaTime;
                            yield return new WaitForEndOfFrame();
                        }
                    }
                }
            }   
        }

        public bool CanDetonate()
        {
            return armed && alive;
        }

        private bool waitingToExplode = false;

        private bool ChainDetonate(RemoteBombExplosive source, float delay)
        {
            if (chainedExplosives.Contains(source) || !alive || waitingToExplode)
                return false;

            HydraLogger.Log($"Detonating after time! {delay}");
            waitingToExplode = true;
            StartCoroutine(DetonateAfterTime(delay));
            return true;
        }

        private IEnumerator DetonateAfterTime(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            Detonate(true);
        }

        public bool Detonate(bool force = false)
        {
            if ((force || armed) && alive)
            {
                Events.OnPlayerDeath -= () => Detonate(true);

                alive = false;
                GameObject newBoom = Instantiate<GameObject>(ExplosionPrefab, transform.position, Quaternion.identity);
                newBoom.transform.up = transform.forward;
                weapon.BombDetonated(this);

                DoExplosion();

                Destroy(gameObject);
                return true;
            }

            return false;
        }

        private void Arm()
        {
            armed = true;
            AC_armingBeep.Play();
        }


        private void DoExplosion()
        {
            List<EnemyIdentifier> hitEnemies = new List<EnemyIdentifier>();

            Rigidbody playerRB = null;

            Visualizer.DrawSphere(transform.position, explosionRadius, 2.0f);

            Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius, LayerMask.GetMask("Limb", "Projectile", "BigCorpse", "Armor", "Default", "Ignore Raycast"));
            if (colliders == null)
                return;

            foreach (Collider col in colliders)
            {
                if (col.tag == "Floor")
                    continue;

                Vector3 hitPoint = col.ClosestPoint(transform.position);
                Vector3 hitDirection = hitPoint - transform.position;

                float distance = hitDirection.magnitude;

                if (col.TryGetComponent<EnemyIdentifierIdentifier>(out EnemyIdentifierIdentifier eidID))
                {
                    if (!hitEnemies.Contains(eidID.eid) && !eidID.eid.dead)
                    {
                        float damageFalloff = Mathf.InverseLerp(explosionRadius, 0.0f, distance);
                        float damageAmount = (eidID.eid == stuckTarget) ? explosionDamage * damageFalloff * stuckDamageMultiplier : explosionDamage * damageFalloff;
                        eidID.eid.DeliverDamage(eidID.eid.gameObject, hitDirection.normalized, hitPoint, damageAmount, true, 0, weapon.gameObject);
                        hitEnemies.Add(eidID.eid);
                    }
                }

                if (col.TryGetComponent<EnemyIdentifier>(out EnemyIdentifier enemy))
                {
                    if (!hitEnemies.Contains(enemy) && !enemy.dead)
                    {
                        float damageFalloff = Mathf.InverseLerp(explosionRadius, 0.0f, distance);
                        float damageAmount = (enemy == stuckTarget) ? explosionDamage * damageFalloff * stuckDamageMultiplier : explosionDamage * damageFalloff;
                        enemy.DeliverDamage(enemy.gameObject, hitDirection.normalized, hitPoint, damageAmount, true, 0, weapon.gameObject);
                        hitEnemies.Add(enemy);
                    }
                }

                if (col.TryFindComponent<RemoteBombExplosive>(out RemoteBombExplosive remoteBombExplosive))
                {
                    float rangeDelay = Mathf.InverseLerp(0.0f, explosionRadius, distance);
                    if(chainedExplosives.Count > 100)
                    {
                        //There is no point in chaining any past 100 since there will be a nuke.
                        Destroy(remoteBombExplosive.gameObject);
                    }else if(remoteBombExplosive.ChainDetonate(this, explosionChainDelay * rangeDelay))
                    {
                        chainedExplosives.Add(remoteBombExplosive);
                    }
                }                

                if (col.TryGetComponent<NewMovement>(out NewMovement player) && !pushedPlayer)
                {
                    float damageFalloff = Mathf.InverseLerp(explosionRadius * 4, 0.0f, distance);

                    Vector3 playerPos = player.transform.position;
                    playerPos.y += 1.25f;

                    Vector3 pushDir = playerPos - transform.position;
                    playerRB = player.rb;

                    playerRB.velocity += (pushDir.normalized * (explosionDamage * damageFalloff)) * playerKnockbackMultiplier;
                    pushedPlayer = true;
                }

                if (col.TryGetComponent<Breakable>(out Breakable breakable))
                {
                    breakable.Break();
                }

                if(col.TryGetComponent<Glass>(out Glass glass))
                {
                    glass.Shatter();
                }

                if (col.TryGetComponent<Rigidbody>(out Rigidbody hitRb))
                {
                    bool applyForce = true;

                    if (playerRB != null)
                    {
                        if (hitRb == playerRB)
                        {
                            applyForce = false;
                        }
                    }

                    if (!hitRb.isKinematic && applyForce)
                    {
                        float damageFalloff = Mathf.InverseLerp(explosionRadius, 0.0f, distance);
                        hitRb.AddExplosionForce((explosionDamage * 50 * damageFalloff) * rigidbodyForceMultiplier, transform.position, explosionRadius);
                    }
                }

            }

            int chainCount = chainedExplosives.Count;

            if (waitingToExplode && (Vector3.Distance(lastExplosionPosition, transform.position) < 8.0f))
                return;

            switch(chainCount)
            {
                case 0: 
                    break;
                case var expression when (chainCount > 2 && chainCount < 6):
                    Instantiate(Prefabs.UK_Explosion.Asset, transform.position, Quaternion.identity);
                    lastExplosionPosition = transform.position;
                    break;
                case var expression when (chainCount > 6 && chainCount < 100):
                    Instantiate(Prefabs.UK_ExplosionMalicious.Asset, transform.position, Quaternion.identity);
                    lastExplosionPosition = transform.position;
                    break;
                case var expression when (chainCount > 100):
                    Instantiate(MysticFlare.MysticFlareExplosion, transform.position, Quaternion.identity);
                    lastExplosionPosition = transform.position;
                    break;
            }
        }


        private void StickToEnemy(Transform newParent, EnemyIdentifier enemy, Vector3 normal)
        {
            stuckTarget = enemy;
            enemy.DeliverDamage(enemy.gameObject, -normal, transform.position, stickDamage, false, 0, weapon.gameObject);
            StickToThing(newParent, normal);
        }

        private void StickToThing(Transform newParent, Vector3 normal)
        {
            landed = true;
            if(rb)
            {
                rb.isKinematic = true;
                rb.detectCollisions = false;
                rb.GetComponent<Collider>().enabled = false;
            }
            transform.parent = newParent;
            transform.forward = normal;
        }

        private bool CanStickToThing(GameObject thing)
        {
            if(thing.tag == "Floor" || thing.tag == "Wall")
            {
                return true;
            }

            switch(thing.layer)
            {
                case 8:
                    return true;
                case 10:
                    return true;
                case 11:
                    return true;
                case 24:
                    return true;
                case 25:
                    return true;
                case 26:
                    return true;
                default:
                    return false;
            }
        }

        private void OnCollisionEnter(Collision col)
        {
            if(landed || !armed)
            {
                return;
            }

            bool stick = CanStickToThing(col.gameObject);

            if(stick)
            {
                StickToThing(col.transform, col.GetContact(0).normal);
            }

            if (col.collider.TryGetComponent<EnemyIdentifierIdentifier>(out EnemyIdentifierIdentifier enmy))
            {
                if (!enmy.eid.dead)
                {
                    StickToEnemy(col.collider.transform, enmy.eid, col.GetContact(0).normal);
                    return;
                }
            }

            if (col.collider.TryGetComponent<EnemyIdentifier>(out EnemyIdentifier enemy))
            {
                if (!enemy.dead)
                {
                    StickToEnemy(col.collider.transform, enemy, col.GetContact(0).normal);
                    return;
                }
            }
        }

        public void Shot(BeamType beamType)
        {
            switch (beamType)
            {
                case BeamType.Railgun:
                    Detonate(true);
                    break;
                case BeamType.Revolver:
                    Detonate(false);
                    break;
                case BeamType.MaliciousFace:
                    Detonate(true);
                    break;
                case BeamType.Enemy:
                    Detonate(true);
                    break;
            }
        }

        public bool Interact(UFGInteractionEventData interaction)
        {
            if(interaction.ContainsAnyTag("shot", "explode"))
            {
                Detonate(true);
                return true;
            }

            return false;
        }

        public Vector3 GetPosition()
        {
            return transform.position;
        }

        public bool Targetable(TargetQuery targetQuery)
        {
            if (landed)
                return false;

            return targetQuery.CheckTargetable(transform.position);
        }
    }
}
