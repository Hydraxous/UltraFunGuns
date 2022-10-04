using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UltraFunGuns
{
    public class GyroRotator : MonoBehaviour
    {
        private SonicReverberator gun;
        private GyroRotatorData data;
        private OptionsManager om;

        void Start()
        {
            GetStuff();
        }

        void GetStuff()
        {
            gun = gameObject.GetComponentInParent<SonicReverberator>();
            om = MonoSingleton<OptionsManager>.Instance;
            HydraLoader.dataRegistry.TryGetValue(gameObject.name, out UnityEngine.Object dataGet);
            data = (GyroRotatorData)dataGet;
        }


        void Update()
        {
            if (!om.paused)
            {
                if (gun.charging)
                {
                    data.angularVelocity = Mathf.Clamp((data.angularVelocity + (gun.currentGyroRotationSpeed * data.rotationSpeedMultiplier)), 0, data.maxRotationSpeed);
                }
                else
                {
                    data.angularVelocity = Mathf.Clamp((data.angularVelocity - data.dragRate), 0, data.maxRotationSpeed);
                }
                data.currentRot += data.angularVelocity;
                transform.localRotation = Quaternion.AngleAxis(data.currentRot, data.rotateAxis); //bad bad gimbal lock go away stinky >:(
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
