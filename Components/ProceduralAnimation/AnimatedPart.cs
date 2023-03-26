using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    public class AnimatedPart : MonoBehaviour
    {
        [SerializeField] private float baseRotateSpeed;
        [SerializeField] private Vector3 rotateAxis;

        private float speedMultiplier = 1.0f;

        private Vector3 currentRotation;

        private void Awake()
        {
            currentRotation = transform.localRotation.eulerAngles;
        }

        public void SetRotationSpeed(float rotationSpeed)
        {
            speedMultiplier = rotationSpeed;
        }

        private void Update()
        {
            Rotate();
        }

        private void Rotate()
        {
            currentRotation += rotateAxis * baseRotateSpeed * speedMultiplier * Time.deltaTime;

            transform.localRotation = Quaternion.Euler(currentRotation);
        }
    }
}
