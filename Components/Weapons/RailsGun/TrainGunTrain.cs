using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Configgy;

namespace UltraFunGuns
{
    public class TrainGunTrain : MonoBehaviour, ICleanable
    {
        
        [SerializeField] private BoxCollider boxcastRef;

        [Configgy.Configgable("UltraFunGuns/Weapons/Conductor")]
        public static float moveSpeed = 150f;

        [Configgy.Configgable("UltraFunGuns/Weapons/Conductor")] 
        private static float hitCooldownTime = 0.35f;

        [Configgy.Configgable("UltraFunGuns/Weapons/Conductor")] 
        private static float enemyDamage = 3f;

        [Configgy.Configgable("UltraFunGuns/Weapons/Conductor")] 
        private static float rbLaunchForce = 85f;

        [Configgy.Configgable("UltraFunGuns/Weapons/Conductor")] 
        private static float travelDistance = 1000f;

        private Dictionary<Collider, float> hits = new Dictionary<Collider, float>();

        private float distanceTraveled = 0f;

        private GameObject owner;

        public void SetOwner(GameObject owner)
        {
            this.owner = owner;
        }

        private void FixedUpdate()
        {
            Vector3 position = transform.position;

            float moveDistance = moveSpeed * Time.fixedDeltaTime;
            Vector3 moveDelta = moveDistance * transform.forward;

            Vector3 boxSize = Vector3.Scale(boxcastRef.size, boxcastRef.transform.localScale);

            RaycastHit[] hits = Physics.BoxCastAll(boxcastRef.transform.position, boxSize*0.5f, transform.forward, boxcastRef.transform.rotation, moveDelta.magnitude, LayerMaskDefaults.Get(LMD.EnemiesAndPlayer));
            HitObjects(hits);
            position += moveDelta;

            transform.position = position;
            distanceTraveled += moveDistance;

            if(distanceTraveled >= travelDistance)
            {
                GameObject.Destroy(gameObject);
            }
        }

        private void HitObjects(RaycastHit[] hits)
        {
            foreach (RaycastHit hit in hits)
            {
                HitObject(hit);
            }
        }

        private void HitObject(RaycastHit hit)
        {
            if (hits.ContainsKey(hit.collider))
                if (Time.time-hits[hit.collider] < hitCooldownTime)
                    return;

            Vector3 hurtVector = hit.point - boxcastRef.transform.position;
            Vector3 launchForce = hurtVector.normalized *rbLaunchForce;
            launchForce.y = 1.25f*Mathf.Abs(launchForce.y);


            if (hit.collider.tag == "Player")
            {
                //HUrt player
                NewMovement.Instance.Launch(launchForce);
                NewMovement.Instance.GetHurt(30, false, 2, false, true);
                hits[hit.collider] = Time.time;
            }

            if (hit.collider.gameObject.layer == 10 || hit.collider.gameObject.layer == 11 || hit.collider.gameObject.layer == 12 || hit.collider.gameObject.layer == 20)
            {
                if (hit.collider.IsColliderEnemy(out EnemyIdentifier eid))
                {
                    hits[hit.collider] = Time.time;
                    eid.hitter = "TrainGun";
                    eid.DeliverDamage(eid.gameObject, hurtVector.normalized, hit.point, enemyDamage, true, 0f, owner, false);
                }
            }
        }

        public void Cleanup()
        {
            GameObject.Destroy(gameObject);
        }
    }
}
