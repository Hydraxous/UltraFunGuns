using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    public class FocalyzerPylon : MonoBehaviour
    {
        public Animator animator;
        public bool refracting = false;
        public Focalyzer focalyzer;
        public FocalyzerLaserController pylonManager;
        public FocalyzerPylon targetPylon;

        private float lifeTime = 8.0f;
        private float lifeTimeLeft = 0.0f;

        void Start()
        {
            lifeTimeLeft = lifeTime + Time.time;
            animator = GetComponent<Animator>();
            transform.Find("FocalyzerCrystalVisual/RefractorVisual").gameObject.AddComponent<AlwaysLookAtCamera>().speed = 0.0f;
            pylonManager.AddPylon(this);
        }

        void Update()
        {
            animator.SetBool("Refracting", refracting);
            if (lifeTimeLeft < Time.time)
            {
                Shatter();
            }
        }

        public void Refract(Vector3 hitPoint)
        {

        }

        void Shatter()
        {

        }

        void OnDisable()
        {
            pylonManager.RemovePylon(this);
        }

        void OnDestroy()
        {
            pylonManager.RemovePylon(this);
        }
    }
}
