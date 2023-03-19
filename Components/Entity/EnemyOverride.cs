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

        private List<Action<Collision>> onCollisionEvents;


        private Dictionary<Renderer, Material[]> startMaterials = new Dictionary<Renderer, Material[]>();
        private Renderer[] renderers;

        public bool RagdollEnabled { get; private set; }

        public bool Frozen { get; private set; }

        private void Awake()
        {
            Enemy = GetComponent<EnemyIdentifier>();
            navMeshAgent = GetComponent<NavMeshAgent>();
            animator = GetComponent<Animator>();

            drone = GetComponent<Drone>();
            zombie = GetComponent<Zombie>();
            machine = GetComponent<Machine>();
            spiderBody = GetComponent<SpiderBody>();
            statue = GetComponent<Statue>();


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
            {
                return;
            }

            if (Enemy.dead)
            {
                return;
            }

            if (navMeshAgent != null)
            {
                navMeshAgent.enabled = active;
            }

            if (drone != null)
            {
                drone.enabled = active;
            }

            if (machine != null)
            {
                machine.enabled = active;
            }

            if (spiderBody != null)
            {
                spiderBody.enabled = active;
            }

            if(statue != null)
            {
                statue.enabled = active;
            }

            if(zombie != null)
            {
                zombie.enabled = active;
            }

            if(animator != null)
            {
                animator.enabled = active;
            }
        }

        public void SetKinematic(bool active)
        {

            if (primaryCollider != null)
            {
                primaryCollider.enabled = active;
            }

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
            {
                return false;
            }
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
    }

    public static class EnemyUtil
    {
        public static EnemyOverride Override(this EnemyIdentifier eid)
        {
            if(eid.TryGetComponent<EnemyOverride>(out EnemyOverride newOverride))
            {
                return newOverride;
            }

            return eid.gameObject.AddComponent<EnemyOverride>();
        }
    }
}
