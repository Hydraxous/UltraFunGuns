using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UltraFunGuns
{
    public class GyroRotator : MonoBehaviour
    {

        [SerializeField] private float baseSpeedMultiplier = 1.0f;
        [SerializeField] private Vector3 rotateAxis;
        [SerializeField] private float dragRate = 1.0f;
        [SerializeField] private float maxRotationSpeed = 500.0f;
        [SerializeField] private float startRotation = 0.0f;

        private float angularVelocity = 0.0f;
        private float currentRot = 0.0f;

        public bool Spin;
        public float Speed = 0.0f;

        private void Start ()
        {
            currentRot = startRotation;
        }

        void Update()
        {
            if (Spin)
            {
                angularVelocity = Mathf.Clamp((angularVelocity + (Speed * baseSpeedMultiplier)), 0, maxRotationSpeed);
            }
            else
            {
                angularVelocity = Mathf.Clamp((angularVelocity - dragRate), 0, maxRotationSpeed);
            }
            currentRot += angularVelocity * Time.timeScale;
            transform.localRotation = Quaternion.AngleAxis(currentRot, rotateAxis); //bad bad gimbal lock go away stinky >:(
        }
    }
}
