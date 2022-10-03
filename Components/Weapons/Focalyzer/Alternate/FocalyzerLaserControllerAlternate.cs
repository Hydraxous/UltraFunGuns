using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    //Controls the pylons and the main laser for the focalyzer.
    public class FocalyzerLaserControllerAlternate : MonoBehaviour
    {
        private List<FocalyzerPylonAlternate> pylonList = new List<FocalyzerPylonAlternate>();

        private List<Vector3> laserPoints = new List<Vector3>();

        public FocalyzerAlternate focalyzer;
        public LineRenderer lineRenderer;
        public Animator animator;

        public bool laserActive = false;

        void Start()
        {
            //TODO Do the tube animations and pylon limits/recharging mechanic
            lineRenderer = GetComponent<LineRenderer>();
            animator = GetComponent<Animator>();
        }

        void Update()
        {
            animator.SetBool("Active", laserActive);
            if(focalyzer == null)
            {
                Destroy(gameObject);
            }
        }

        public void AddPylon(FocalyzerPylonAlternate pylon)
        {
            if (!pylonList.Contains(pylon))
            {
                pylonList.Add(pylon);
            }
        }

        public void RemovePylon(FocalyzerPylonAlternate pylon)
        {
            if (pylonList.Contains(pylon))
            {
                pylonList.Remove(pylon);
            }
        }

        public int GetPylonCount()
        {
            return pylonList.Count;
        }

        public void AddLinePosition(Vector3 position)
        {
            laserPoints.Add(position);
        }

        //Builds the laser visually, Mechanically does nothing.
        public void BuildLine(Vector3 normal)
        {
            lineRenderer.SetPositions(laserPoints.ToArray());
            transform.position = laserPoints[laserPoints.Count-1];
            transform.up = normal;
            laserPoints.Clear();
        }

        //Checks the line of sight from one pylon to another.
        private bool LineOfSightCheck(FocalyzerPylonAlternate pylon1, FocalyzerPylonAlternate pylon2)
        {
            Vector3 rayCastDirection = pylon2.transform.position - pylon1.transform.position;
            RaycastHit[] hits = Physics.RaycastAll(pylon1.transform.position, rayCastDirection, rayCastDirection.magnitude);
            hits = HydraUtils.SortRaycastHitsByDistance(hits);
            foreach(RaycastHit hit in hits)
            {
                if(hit.collider.gameObject.layer == 24 || hit.collider.gameObject.layer == 25 || hit.collider.gameObject.layer == 8 || hit.collider.gameObject.layer == 0)
                {
                    return false;
                }

                if(hit.collider.gameObject.TryGetComponent<FocalyzerPylonAlternate>(out FocalyzerPylonAlternate hitPylon))
                {
                    return true;
                }
            }
            return false;
        }  
    }
}
