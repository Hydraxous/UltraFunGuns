using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    public class DeleteAfterTime : MonoBehaviour
    {
        public float TimeLeft = 10f;

        private void Update()
        {
            if(TimeLeft <= 0f)
            {
                GameObject.Destroy(gameObject);
                return;
            }

            TimeLeft -= Time.deltaTime;
        }
    }
}
