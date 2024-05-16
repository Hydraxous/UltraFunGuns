using System;
using UnityEngine;

namespace UltraFunGuns
{
    //Used to control the display tubes on the focalyzer.
    public class FocalyzerTubeControllerAlternate : MonoBehaviour
    {
        private Animator[] crystals = new Animator[3];
        public int crystalsRemaining = 3;
        private int lastCrystalsLeft;
        bool initialized = false;

        void Awake()
        {
            for(int i=0;i<3;i++)
            {
                crystals[i] = transform.Find(String.Format("Crystal{0}", i)).GetComponent<Animator>();
            }
            initialized = true;
        }

        void Update()
        {
            if(crystalsRemaining != lastCrystalsLeft)
            {
                lastCrystalsLeft = crystalsRemaining;
                UpdateCrystals();
            }
        }

        //Updates the crystals to display how many pylons you have left to fire.
        //2 input
        void UpdateCrystals()
        {
            for (int i = 0; i < crystals.Length; i++)
            {
                if (crystalsRemaining-1 >= i)
                {
                    crystals[i].SetBool("Active", true);
                }
                else
                {
                    crystals[i].SetBool("Active", false);
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
