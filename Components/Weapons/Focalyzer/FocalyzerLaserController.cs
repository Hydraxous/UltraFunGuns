using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    public class FocalyzerLaserController : MonoBehaviour
    {
        private List<FocalyzerPylon> pylonList = new List<FocalyzerPylon>();

        private List<Vector3> laserPoints = new List<Vector3>();

        public Focalyzer focalyzer;
        public LineRenderer lineRenderer;
        public Animator animator;
        public bool laserActive = false;
        public int maxPylonRefractions = 4;
        public int maxPylons = 5;

        void Start()
        {
            lineRenderer = GetComponent<LineRenderer>();
            animator = GetComponent<Animator>();
        }

        void Update()
        {
            animator.SetBool("Active", laserActive);
        }

        public void AddPylon(FocalyzerPylon pylon)
        {
            if (!pylonList.Contains(pylon))
            {
                pylonList.Add(pylon);
            }
        }

        public void RemovePylon(FocalyzerPylon pylon)
        {
            if (pylonList.Contains(pylon))
            {
                pylonList.Remove(pylon);
            }
        }

        public int PylonCount()
        {
            return pylonList.Count;
        }

        public void AddLinePosition(Vector3 position)
        {
            laserPoints.Add(position);
        }

        public void BuildLine(Vector3 normal)
        {
            lineRenderer.SetPositions(laserPoints.ToArray());
            transform.position = laserPoints[laserPoints.Count-1];
            transform.up = normal;
            laserPoints.Clear();
        }

        public void StopAllRefractions()
        {
            foreach (FocalyzerPylon pylon in pylonList)
            {
                pylon.SetRefraction(false);
            }
        }

        //TODO figure out a way to return null to a pylon to stop refraction

        public FocalyzerPylon GetRefractorTarget(FocalyzerPylon originPylon)
        {
            originPylon.SetRefraction(true);
            FocalyzerPylon targetPylon = originPylon;
            if (pylonList.Count > 1)
            {
                int lowestRefractionIndex = -1;
                int lowestRefractionCount = maxPylonRefractions;
                for (int i = 0; i < pylonList.Count; i++)
                {
                    if(originPylon != pylonList[i])
                    {
                        if (!pylonList[i].refracting)
                        {
                            
                            targetPylon = pylonList[i];
                        }
                        else if (pylonList[i].refractionCount < lowestRefractionCount)
                        {
                            lowestRefractionCount = pylonList[i].refractionCount;
                            lowestRefractionIndex = i;
                        }
                    }
                    
                }

                if (lowestRefractionIndex > -1)
                {
                    targetPylon = pylonList[lowestRefractionIndex];
                }
            }
            return targetPylon;
        }
    }
}
