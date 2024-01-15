using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UltraFunGuns.Components.Entity
{
    //Real life basketballs magically return to the hoop right?
    public class RealisticBasketball : MonoBehaviour, ICleanable
    {
        public Rigidbody Rigidbody { get; private set; }

        private BasketballHoop hoop;

        public Vector3 LastVelocity { get; private set; }
        public float LastThrowDist { get; private set; }
        private float rollSpeed = 4.0f;

        private bool goingBack = false;

        private UltraFunGunBase.ActionCooldown awayCheckCD = new UltraFunGunBase.ActionCooldown(20.0f);

        private void Start()
        {
            Rigidbody = GetComponent<Rigidbody>();
            hoop = GameObject.FindObjectOfType<BasketballHoop>();
            BasketballHoop.EnableBasketballHoop.OnValueChanged += gameObject.SetActive;
            gameObject.SetActive(BasketballHoop.EnableBasketballHoop.Value);
        }

        private void Update()
        {
            if(goingBack && Rigidbody != null && hoop != null)
            {
                Vector3 toHoop = hoop.GetHoopPos() - transform.position;
                float distance = toHoop.magnitude;
                if (distance > 30f)
                {
                    Rigidbody.velocity += (toHoop.normalized*rollSpeed) * ((Rigidbody.velocity.magnitude < 5.0f) ? 1 : 0);

                }else
                {
                    goingBack = false;
                }
            }
        }

        private void LateUpdate()
        {
            if(Rigidbody != null)
            {
                LastVelocity = Rigidbody.velocity;
                CheckAway();
            }

            if (transform.position.sqrMagnitude > 25000000f) //kill if OoB ~ 5000 units from 0,0,0
            {
                Cleanup();
            }
        }

        private void CheckAway()
        {
            if (!awayCheckCD.CanFire() || hoop == null || transform.parent != null)
                return;

            awayCheckCD.AddCooldown();

            Vector3 toHoop = hoop.GetHoopPos() - transform.position;
            float distance = toHoop.magnitude;
            if (distance > 40f)
            {
                goingBack = true;
            }
        }

        public void Thrown()
        {
            if (hoop != null)
            {
                LastThrowDist = (hoop.GetHoopPos() - transform.position).magnitude;
                CheckSlamDunk();
            }
        }


        private void CheckSlamDunk()
        {
            HydraLogger.Log("Dunk: Trying!");

            CameraController cc = CameraController.Instance;
            Vector3 hoopPos = hoop.GetHoopPos();
            Vector3 camPos = cc.transform.position;
            Vector3 camToHoop = hoopPos - cc.transform.position;
            float horizontalDistance = Mathf.Abs(new Vector2(camToHoop.x, camToHoop.z).magnitude);
            float verticalDistance = Mathf.Abs(camToHoop.z);

            HydraLogger.Log($"Dunk: Range {horizontalDistance}:{verticalDistance}!");


            if (verticalDistance> 3.9f || horizontalDistance > 2.5f) //Not in range.
                return;

            HydraLogger.Log("Dunk: In range!");

            NewMovement player = NewMovement.Instance;

            if (player.gc.onGround || !player.gc.heavyFall) //Not slamming.
                return;

            HydraLogger.Log("Dunk: Slamming!");


            if (!(Vector3.Dot((camPos-hoopPos).normalized,Vector3.up) >= -0.1f)) // below rim.
                return;

            HydraLogger.Log("Dunk: Above rim!");

            if (!(Vector3.Dot(camToHoop.normalized, cc.transform.forward) > 0.83f))
                return;

            hoop.SlamDunk();
            transform.position = hoopPos + (Vector3.down * 0.25f);
            Rigidbody.velocity = Vector3.down * 100.0f;
        }

        public void Cleanup()
        {
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            BasketballHoop.EnableBasketballHoop.OnValueChanged -= gameObject.SetActive;
        }
    }
}
