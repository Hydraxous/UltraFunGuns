using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    public class DebugLine : MonoBehaviour
    {
        private LineRenderer lr;
        private bool initialized;

        private void Awake()
        {
            lr = GetComponent<LineRenderer>();
        }

        public void SetLine(Vector3[] points)
        {
            if(!initialized)
            {
                initialized = true;
                lr.positionCount = points.Length;
                lr.SetPositions(points);
            }
        }
        
    }
}
