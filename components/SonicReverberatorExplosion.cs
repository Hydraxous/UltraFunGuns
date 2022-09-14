using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UltraFunGuns
{
    public class SonicReverberatorExplosion : MonoBehaviour
    {

        public float power = 0.0f; //assigned by SonicReverberator class
        public int powerState = 0; //assigned by SonicReverberator class
        private float lifeTime = 0.6f;
        private float moveSpeed = 150.0f;
        private GameObject hitBox;
        private Vector3 startScale = new Vector3(0.5f, 0.5f, 1f);
        private Vector3 endScale = new Vector3(3.5f, 3.5f, 1.0f);
        private float lifeTimeLeft = 0.0f;
        private Rigidbody rb;

        private void Awake()
        {
            rb = gameObject.GetComponent<Rigidbody>();
            hitBox = transform.Find("Hitbox").gameObject;
        }

        private void Start()
        {
            lifeTimeLeft = lifeTime + Time.time;
            Destroy(gameObject, lifeTime);
        }


        private void Update()
        {
            if (hitBox != null)
            {
                float life = Mathf.Lerp(1, 0, (lifeTimeLeft - Time.time) / lifeTime);
                float powerQuotient = (power / Mathf.Clamp(powerState,1,Mathf.Infinity));
                Vector3 scaleModifier = new Vector3(powerQuotient, powerQuotient, 1);
                hitBox.transform.localScale = Vector3.Scale(Vector3.Lerp(startScale, endScale, life), scaleModifier);
                //transform.position += transform.TransformDirection(new Vector3(0, 0, 1 * (power/speedModifier)));
                rb.velocity = transform.TransformDirection(new Vector3(0, 0, 1 * (moveSpeed * (power / Mathf.Clamp(powerState, 1, Mathf.Infinity)))));
                rb.angularVelocity = new Vector3(0, 0, power);
            }
        }
    }
}