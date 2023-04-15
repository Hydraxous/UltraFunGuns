using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace UltraFunGuns
{
    public class AbilityMeter : MonoBehaviour
    {
        [SerializeField] private Image fillImage;
        [SerializeField] private Gradient fillGradient;
        public float AnimateSpeed = 0.34f;
        private float displayedAmount;

        public float Amount { get; private set; }


        public void SetAmount(float newAmount)
        {
            Amount = Mathf.Clamp01(newAmount);
        }

        private void Update()
        {
            if (fillImage == null)
                return;

            if(displayedAmount != Amount || fillImage.fillAmount != displayedAmount)
            {
                displayedAmount = Mathf.MoveTowards(displayedAmount, Amount, AnimateSpeed*Time.deltaTime);
                fillImage.fillAmount = displayedAmount;
                fillImage.color = fillGradient.Evaluate(displayedAmount);
            }
        }

        private void OnEnable()
        {
            displayedAmount = Amount;
        }
        private void OnDsiable()
        {
            displayedAmount = Amount;
        }
    }
}
