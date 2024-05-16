using System;
using System.Collections.Generic;
using UnityEngine;

namespace UltraFunGuns
{
    public class VisualCounterAnimator : MonoBehaviour
    {
        private List<Animator> displays = new List<Animator>();

        private bool descending = false;

        public bool initalized = false;

        private int displayCount = 0;

        //Include placeholder into the relative path fe: PistonPusher{0}/Pusher iterates properly.
        public void Initialize(int numberOfDisplays, string relativePath, bool descending = false)
        {
            this.descending = descending;
            for (int i = 0; i < numberOfDisplays; i++)
            {
                displays.Add(transform.Find(String.Format(relativePath, i)).GetComponent<Animator>());
            }
            initalized = true;
            RefreshDisplays();
        }

        public int DisplayCount
        {
            get { return displayCount; }
            set
            {
                displayCount = value;
                RefreshDisplays();
            }
        }

        public void RefreshDisplays()
        {
            if (initalized)
            {
                if (!descending)
                {
                    for (int i = 0; i < displays.Count; i++)
                    {
                        if (displayCount >= i)
                        {
                            displays[i].SetBool("Active", false);
                        }
                        else
                        {
                            displays[i].SetBool("Active", true);
                        }
                    }

                }else
                {
                    for (int i = 0; i < displays.Count; i++)
                    {
                        if (displayCount - 1 >= i)
                        {
                            displays[i].SetBool("Active", true);
                        }
                        else
                        {
                            displays[i].SetBool("Active", false);
                        }
                    }
                }
            }
        }
    }
}
