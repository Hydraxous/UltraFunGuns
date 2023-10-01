using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.AI;

namespace UltraFunGuns
{
    public class EnemyOverride : MonoBehaviour
    {
        public EnemyIdentifier Enemy { get; private set; }

        private NavMeshAgent navMeshAgent;

        private Animator animator;

        private Drone drone;
        private Zombie zombie;
        private Machine machine;
        private Statue statue;
        private SpiderBody spiderBody;

        private Rigidbody[] childRigidbodies;

        private Collider primaryCollider;
        private Rigidbody primaryRigidbody;

        private List<Action<Collision>> onCollisionEvents = new List<Action<Collision>>();
        private List<Action> onDeathEvents = new List<Action>();

        private Dictionary<Renderer, Material[]> startMaterials = new Dictionary<Renderer, Material[]>();
        private Renderer[] renderers;

        private List<StyleEntry> styleEntries = new List<StyleEntry>();

        public bool RagdollEnabled { get; private set; }

        public bool Frozen { get; private set; }

        private bool initialized;

        private void Awake()
        {
            if (!initialized)
                ForceInitialize();
        }

        public void ForceInitialize()
        {
            if (initialized)
                return;

            initialized = true;

            Enemy = GetComponent<EnemyIdentifier>();
            navMeshAgent = GetComponent<NavMeshAgent>();
            animator = GetComponent<Animator>();

            drone = GetComponent<Drone>();
            zombie = GetComponent<Zombie>();
            machine = GetComponent<Machine>();
            spiderBody = GetComponent<SpiderBody>();
            statue = GetComponent<Statue>();

            Enemy.onDeath.AddListener(ExecuteOnDeathEvents);

            GetPhysicsComponents();
            GetRenderComponents();
        }

        private void GetPhysicsComponents()
        {
            primaryCollider = GetComponent<Collider>();
            childRigidbodies = GetComponentsInChildren<Rigidbody>();
        }

        private void GetRenderComponents()
        {
            renderers = GetComponentsInChildren<Renderer>(true);
            foreach(Renderer renderer in renderers)
            {
                if(!startMaterials.ContainsKey(renderer))
                {
                    startMaterials.Add(renderer, renderer.materials);
                }
            }
        }

        public void ResetMaterials()
        {
            foreach(Renderer renderer in renderers)
            {
                if(renderer == null)
                {
                    continue;
                }

                if(!startMaterials.ContainsKey(renderer))
                {
                    continue;
                }

                renderer.materials = startMaterials[renderer];
            }
        }

        public void SetFrozen(bool frozen)
        {
            Frozen = frozen;
            SetComponents(!frozen);
            if(RagdollEnabled && frozen)
            {
                SetKinematic(frozen);
            }
        }

        public void SetAllMaterials(Material[] newMaterials)
        {
            foreach (KeyValuePair<Renderer, Material[]> keyValuePair in startMaterials)
            {
                if(keyValuePair.Key == null)
                {
                    continue;
                }

                keyValuePair.Key.materials = newMaterials;
            }
        }

        public void SetAllMaterial(Material material)
        {
            foreach (KeyValuePair<Renderer, Material[]> keyValuePair in startMaterials)
            {
                if (keyValuePair.Key == null)
                {
                    continue;
                }

                keyValuePair.Key.material = material;
            }
        }

        public void Knockback(Vector3 force)
        {
            if(!RagdollEnabled)
            {
                return;
            }

            foreach(Rigidbody childRb in childRigidbodies)
            {
                if(childRb != null)
                {
                    childRb.velocity = force;
                }
            }
        }

        public void AddStyleEntryOnDeath(StyleEntry entry, bool allowMultiple = false)
        {
            if (Enemy != null)
                entry.EnemyIdentifier = Enemy;

            if(!allowMultiple)
            {
                if (styleEntries.Where(x => x.Key == entry.Key).ToList().Count > 0)
                {
                    styleEntries[0].LifeTime += entry.LifeTime;
                    return;
                }
            }

            styleEntries.Add(entry);
        }

        private void ExecuteStyleEntries()
        {
            styleEntries = styleEntries.Where(x => x.valid).ToList();
            foreach(StyleEntry entry in styleEntries)
            {
                if (entry.AlreadyCounted)
                    continue;

                //Combine similar entries into one and total count and points
                List<StyleEntry> similarEntries = styleEntries.Where(x => x.Key == entry.Key && (x != entry)).ToList();

                for (int i = 0; i < similarEntries.Count; i++)
                {
                    if (similarEntries[i] == entry)
                        continue;

                    entry.Points += similarEntries[i].Points;
                    int similarCount = ((similarEntries[i].Count >= 1) ? similarEntries[i].Count : 1);
                    entry.Count = (entry.Count >= 1) ? entry.Count + similarCount : 1 + similarCount; //since count is default to -1, this is to correct for it.

                    similarEntries[i].AlreadyCounted = true;
                }

                if(similarEntries.Count > 0)
                {
                    Debug.LogWarning("Combined similar entries");
                }

                WeaponManager.AddStyle(entry);
            }
        }

        public void AddDeathCallback(Action action)
        {
            if(!onDeathEvents.Contains(action))
            {
                onDeathEvents.Add(action);
            }
        }

        private void ExecuteOnDeathEvents()
        {
            foreach(Action action in onDeathEvents)
            {
                action?.Invoke();
            }

            ExecuteStyleEntries();
        }

        public void EnableKnockback()
        {

        }

        public void DisableKnockback()
        {

        }

        public void SetRagdoll(bool active)
        {
            RagdollEnabled = active;
            SetComponents(!active);
            SetKinematic(!active);
        }

        public void SetComponents(bool active)
        {
            if (Enemy == null)
                return;

            if (Enemy.dead)
                return;

            if (navMeshAgent != null)
                navMeshAgent.enabled = active;

            if (drone != null)
                drone.enabled = active;

            if (machine != null)
                machine.enabled = active;

            if (spiderBody != null)
                spiderBody.enabled = active;

            if (statue != null)
                statue.enabled = active;

            if (zombie != null)
                zombie.enabled = active;

            if (animator != null)
                animator.enabled = active;
        }

        public void SetKinematic(bool active)
        {

            if (primaryCollider != null)
                primaryCollider.enabled = active;

            foreach(Rigidbody childRB in childRigidbodies)
            {
                if(childRB != null)
                {
                    childRB.isKinematic = active;
                    childRB.useGravity = !active;
                }

            }
        }

        public bool AddCollisionEvent(Action<Collision> collisionCallback)
        {
            if (collisionCallback == null || onCollisionEvents.Contains(collisionCallback))
                return false;

            onCollisionEvents.Add(collisionCallback);
            return true;
        }

        private void OnCollisionEnter(Collision col)
        {
            foreach(Action<Collision> action in onCollisionEvents)
            {
                action?.Invoke(col);
            }
        }


        public float GetHealth()
        {
            if (spiderBody != null)
                return spiderBody.health;

            if (drone != null)
                return drone.health;

            if (zombie != null)
                return zombie.health;

            if (machine != null)
                return machine.health;

            if (statue != null)
                return statue.health;

            return Enemy.health;
        }

    }

    public static class EnemyUtil
    {
        public static EnemyOverride Override(this EnemyIdentifier eid)
        {
            EnemyOverride enemyOverride = eid.gameObject.EnsureComponent<EnemyOverride>();
            enemyOverride.ForceInitialize();
            return enemyOverride;
        }
    }

    public class StyleEntry
    {
        public string Key, Prefix, Postfix;
        public int Points;
        public float LifeTime;
        public GameObject SourceWeapon;
        public EnemyIdentifier EnemyIdentifier;
        public int Count;
        public bool AlreadyCounted;

        private float timeCreated;

        public bool valid => (Time.time - timeCreated) < LifeTime;

        public StyleEntry(int points, string key, float lifeTime = 5.0f, GameObject sourceWeapon = null, EnemyIdentifier eid = null, int count = -1, string prefix = "", string postfix = "")
        {
            Key = key;
            Points = points;
            LifeTime = lifeTime;
            timeCreated = Time.time;
            Count = count;
            SourceWeapon = sourceWeapon;
            EnemyIdentifier = eid;
            Count = count;
            Prefix = prefix;
            Postfix = postfix;
        }

        public StyleEntry()
        {
            timeCreated = Time.time;
        }
    }
}
