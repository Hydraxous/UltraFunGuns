using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    public class DestroyAfterTime : MonoBehaviour
    {
        void Start()
        {
            Destroy(gameObject, 8.0f);
        }
    }
}
