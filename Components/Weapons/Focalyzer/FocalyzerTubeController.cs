using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    public class FocalyzerTubeController : MonoBehaviour
    {
        private Animator[] crystals = new Animator[6];
        public int crystalsUsed = 6;
        private int lastCrystalsLeft;
        bool initialized = false;

        void Awake()
        {
            for(int i=0;i<6;i++)
            {
                crystals[i] = transform.Find(String.Format("Crystal{0}", i)).GetComponent<Animator>();
            }
            initialized = true;
        }

        //6 crystal will be crystals used
        void Update()
        {
            if(crystalsUsed != lastCrystalsLeft)
            {
                lastCrystalsLeft = crystalsUsed;
                UpdateCrystals();
            }
        }


        void UpdateCrystals()
        {
            for (int i = 0; i < crystals.Length; i++)
            {
                if (crystalsUsed >= i)
                {
                    crystals[i].SetBool("Active", false);
                }
                else
                {
                    crystals[i].SetBool("Active", true);
                }
            }
        }

        void OnDisable()
        {
            if(initialized)
            {
                UpdateCrystals();
            }
        }

        void OnEnable()
        {
            if(initialized)
            {
                UpdateCrystals();
            }
        }
    }
}
