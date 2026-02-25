using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraFunGuns.Components.Weapons.TireGun
{
    public class TireProjectile : MonoBehaviour
    {
        private Rigidbody rb;
        private bool grounded;
        private RaycastHit groundHit;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        private void OnCollisionEnter(Collision collision)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.detectCollisions = false;
            Destroy(gameObject, 5);
        }


        private void FixedUpdate()
        {
            bool lastGrounded = grounded;
            grounded = CheckGrounding();

        }


        private bool CheckGrounding()
        {
            bool ground = Physics.BoxCast(transform.position, Vector3.one * 0.5f, Vector3.down, out RaycastHit hit, Quaternion.identity, 0.5f);
            if (ground)
            {
                groundHit = hit;
            }
            return ground;
        }
    }
}
