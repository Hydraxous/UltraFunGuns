using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    //Controls the pylons and the main laser for the focalyzer.
    public class FocalyzerLaserController : MonoBehaviour
    {
        private List<FocalyzerPylon> pylonList = new List<FocalyzerPylon>();

        private List<Vector3> laserPoints = new List<Vector3>();

        public Focalyzer focalyzer;
        public LineRenderer lineRenderer;
        public Animator animator;

        public bool laserActive = false;
        public int maxPylonRefractions = 4;
        public int maxPylons = 6;
        public float maxPylonRange = 500.0f;

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
        private bool LineOfSightCheck(FocalyzerPylon pylon1, FocalyzerPylon pylon2)
        {
            Vector3 rayCastDirection = pylon2.transform.position - pylon1.transform.position;
            RaycastHit[] hits = Physics.RaycastAll(pylon1.transform.position, rayCastDirection, rayCastDirection.magnitude);
            hits = focalyzer.SortHitsByDistance(hits);
            foreach(RaycastHit hit in hits)
            {
                if(hit.collider.gameObject.layer == 24 || hit.collider.gameObject.layer == 25 || hit.collider.gameObject.layer == 8 || hit.collider.gameObject.layer == 0)
                {
                    return false;
                }

                if(hit.collider.gameObject.TryGetComponent<FocalyzerPylon>(out FocalyzerPylon hitPylon))
                {
                    return true;
                }
            }
            return false;
        }
        
        //Returns closest pylon within line of sight which isnt already refracting, if it cannot find one it will return itself.
        public FocalyzerPylon GetRefractorTarget(FocalyzerPylon originPylon)
        {
            FocalyzerPylon targetPylon = originPylon;
            if (pylonList.Count > 1)
            {
                int closestPylonIndex = -1;
                float closestDistance = maxPylonRange;
                for (int i = 0; i < pylonList.Count; i++)
                {
                    //Pylon should not target itself.
                    if(originPylon != pylonList[i])
                    {
                        Vector3 directionToPylon = pylonList[i].transform.position - originPylon.transform.position;
                        //Pylon should not target a pylon that is already firing or out of line of sight
                        if (LineOfSightCheck(originPylon, pylonList[i]))
                        {
                            bool eligible = false;

                            bool condition1 = (!pylonList[i].refracting); //First target pylons that aren't refracting.
                            bool condition2 = (pylonList[i].targetPylon != originPylon && pylonList.Count > 2); //Second target pylons that aren't trgeting og pylon if theres more than 2.
                            bool condition3 = (pylonList.Count < 3 && pylonList[i].refracting && pylonList[i].targetPylon == originPylon); //Third target pylons which are already targeting og if theres less than 3 pylons and is

                            if(condition1 || condition2 || condition3)
                            {
                                eligible = true;
                            }
                            //TODO problem, you can trick a pylon into disco mode when it is being targeted.
                            if(eligible)
                            {
                                targetPylon = pylonList[i];
                                if (directionToPylon.sqrMagnitude < (closestDistance * closestDistance))
                                {
                                    closestDistance = directionToPylon.magnitude;
                                    closestPylonIndex = i;
                                }
                            }
                        }
                    }
                }

                if (closestPylonIndex > -1)
                {
                    targetPylon = pylonList[closestPylonIndex];
                }
            }
            return targetPylon;
        }
    }
}
