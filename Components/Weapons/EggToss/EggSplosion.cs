using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    public class EggSplosion : MonoBehaviour
    {

        public float explosionSize = 3.5f;

        private void Start()
        {
            RaycastHit[] hits = Physics.SphereCastAll(transform.position, explosionSize, transform.position, Mathf.Infinity);
        }

        private void Update()
        {

        }
    }
}
