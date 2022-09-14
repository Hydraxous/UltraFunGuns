using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UltraFunGuns
{
    public class GyroRotator : MonoBehaviour
    {
        private SonicReverberator gun;
        private GyroRotatorData data;

        void Start()
        {
            GetStuff();
        }

        void GetStuff()
        {
            try
            {
                gun = gameObject.GetComponentInParent<SonicReverberator>();
                DataFile dataGet;
                HydraLoader.dataRegistry.TryGetValue(gameObject.name, out dataGet);
                data = (GyroRotatorData)dataGet;
            }catch (System.Exception e)
            {
                return;
            }
        }


        void Update()
        {
            if (gun != null && data != null)
            {
                if (gun.charging)
                {
                    data.angularVelocity = Mathf.Clamp((data.angularVelocity + (gun.rotationSpeed * data.rotationSpeedMultiplier)), 0, data.maxRotationSpeed);
                }
                else
                {
                    data.angularVelocity = Mathf.Clamp((data.angularVelocity - data.dragRate), 0, data.maxRotationSpeed);
                }
                data.currentRot += data.angularVelocity;
                transform.localRotation = Quaternion.AngleAxis(data.currentRot, data.rotateAxis); //bad bad gimbal lock go away stinky >:(

            }
            else
            {
                GetStuff();
            }
        }


        public class GyroRotatorData : DataFile
        {
            public float rotationSpeedMultiplier = 1.0f;
            public Vector3 rotateAxis = new Vector3(0, 0, 1);
            public float angularVelocity = 0.0f;
            public float dragRate = 1.0f;
            public float maxRotationSpeed = 500.0f;
            public float currentRot = 0.0f;

            public GyroRotatorData(float rotationSpeedMultiplier, Vector3 rotateAxis, float dragRate, float maxRotationSpeed)
            {
                this.rotationSpeedMultiplier = rotationSpeedMultiplier;
                this.rotateAxis = rotateAxis;
                this.dragRate = dragRate;
                this.maxRotationSpeed = maxRotationSpeed;
            }

            public GyroRotatorData(float rotationSpeedMultiplier, Vector3 rotateAxis, float dragRate, float maxRotationSpeed, float currentRot)
            {
                this.rotationSpeedMultiplier = rotationSpeedMultiplier;
                this.rotateAxis = rotateAxis;
                this.dragRate = dragRate;
                this.maxRotationSpeed = maxRotationSpeed;
                this.currentRot = currentRot;
            }

        }
    }
}
