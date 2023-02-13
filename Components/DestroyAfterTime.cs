using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    public class DestroyAfterTime : MonoBehaviour
    {
        public float TimeLeft = 8.0f;
        private float timeAtStart;

        private void Update()
        {
            TimeLeft -= Time.deltaTime;

            if(TimeLeft <= 0.0f)
            {
                Destroy(gameObject);
            }
        }

        void OnDisable()
        {
            Destroy(gameObject);
        }
    }
}
