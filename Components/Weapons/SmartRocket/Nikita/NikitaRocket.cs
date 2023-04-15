using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace UltraFunGuns
{
    public class NikitaRocket : MonoBehaviour, ICleanable
    {
        [SerializeField] private Image[] speedMeter;
        [SerializeField] private Camera camera;
        [NonSerialized] public float minSpeed = 5.0f, maxSpeed = 150.0f, startSpeed = 80.0f;
        [NonSerialized] public float speedChangeDelta = 50f, speedChangeAcceleration = 50f, steerSpeed = 120.0f, rollSpeed = 65.0f, steerSmoothing = 0.05f, rollSmoothing = 0.06f;
        [NonSerialized] public float currentSpeed;

        private float minFov = 77.0f, maxFov = 130.0f;

        public bool Controlled { get; private set; } = true;

        [NonSerialized] public Vector2 lookInput, moveInput;

        private void Start()
        {
            altCams = CameraController.Instance.GetComponentsInChildren<Camera>();
            currentSpeed = startSpeed;
            SetCameraActive(true);
        }

        private void Update()
        {
            if (InputManager.Instance.InputSource.Fire1.WasPerformedThisFrame && Controlled)
            {
                Detonate();
                return;
            }

            if(InputManager.Instance.InputSource.Fire2.WasPerformedThisFrame && Controlled)
            {
                SetControlState(false, false);
                return;
            }

            lookInput = Vector2.zero;
            moveInput = Vector2.zero;

            if (Controlled)
            {
                UpdateUI();
                lookInput = InputManager.Instance.InputSource.Look.ReadValue<Vector2>();
                moveInput = InputManager.Instance.InputSource.Move.ReadValue<Vector2>();
            }

            Move();

            if(camera != null)
            {
                camera.fieldOfView = Mathf.Lerp(minFov, maxFov, currentSpeed/maxSpeed);
            }
            
        }

        private Vector2 smoothedLook;
        private float smoothedRoll;
        private void Move()
        {
            float newSpeed = Mathf.Clamp(currentSpeed + (speedChangeAcceleration * moveInput.y), minSpeed, maxSpeed);
            currentSpeed = Mathf.MoveTowards(currentSpeed, newSpeed, speedChangeDelta*Time.deltaTime);

            float roll = -moveInput.x * rollSpeed * Time.deltaTime;
            smoothedRoll = Mathf.MoveTowards(smoothedRoll, roll, rollSmoothing) * Time.deltaTime;
            
            smoothedLook = Vector2.MoveTowards(smoothedLook, lookInput, steerSmoothing);
            Vector2 steerVector = smoothedLook * steerSpeed * Time.deltaTime;

            transform.Rotate(new Vector3(-steerVector.y, steerVector.x, smoothedRoll), Space.Self);
            transform.position += transform.forward * currentSpeed * Time.deltaTime;
        }

        public void Detonate()
        {
            Controlled = false;
            Destroy(gameObject);
        }

        public void SetControlState(bool control, bool cam)
        {
            Controlled = control;
            SetCameraActive(cam);
        }

        private Camera[] altCams;

        private void SetCameraActive(bool enabled)
        {
            if(camera != null)
                camera.enabled = enabled;
            
            CameraController.Instance.cam.enabled = !enabled;
            for(int i=0; i< altCams.Length; i++)
            {
                altCams[i].enabled = !enabled;
            }

        }

        void UpdateUI()
        {
            for(int i=0; i < speedMeter.Length; i++)
            {
                if (speedMeter[i] == null)
                    continue;

                speedMeter[i].fillAmount = currentSpeed / maxSpeed;
            }
        }


        private void OnDestroy()
        {
            SetCameraActive(false);
        }

        private void OnDisable()
        {
            SetCameraActive(false);
        }

        public void Cleanup()
        {
            Detonate();
        }
    }
}
