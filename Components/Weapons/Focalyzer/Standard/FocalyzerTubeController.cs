using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    //Used to control the display tubes on the focalyzer.
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

        void Update()
        {
            if(crystalsUsed != lastCrystalsLeft)
            {
                lastCrystalsLeft = crystalsUsed;
                UpdateCrystals();
            }
        }

        //Updates the crystals to display how many pylons you have left to fire.
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
