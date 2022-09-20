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

        void Start()
        {
            lineRenderer = GetComponent<LineRenderer>();
            animator = GetComponent<Animator>();
        }

        void Update()
        {
            animator.SetBool("Active", laserActive);
            if (laserActive)
            {
                BuildLine();
            }
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

        public void SetLinePos(int index, Vector3 worldPosition)
        {

        }

        public void AddLinePosition(Vector3 position)
        {
            laserPoints.Add(position);
        }

        public void BuildLine()
        {

        }

        public FocalyzerPylon GetRefractorTarget(FocalyzerPylon originPylon) //TODO
        {
            return null;
        }
    }
}
