using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    public class Vibrator : MonoBehaviour
    {
        private Vector3 startPositon;

        private Vector3 vibrationAxes = Vector3.one.normalized;
        public float CurrentVibration { get; private set; }
        public float MaxDistance { get; private set; }

        private float vibrationTime = 0;


        private void Start()
        {
            startPositon = transform.localPosition;
        }

        private void Update()
        {
            if(CurrentVibration > 0)
            {
                transform.localPosition = GetNewPosition();
            }

            CheckVibrationTime();
        }

        public void AddTime(float t)
        {
            vibrationTime += t;
        }

        private void CheckVibrationTime()
        {
            vibrationTime -= Time.deltaTime;
            vibrationTime = Mathf.Max(vibrationTime, 0);
            CurrentVibration = (vibrationTime * 0.016666f);
        }

        public void SetVibrationAxes(Vector3 localAxes)
        {
            vibrationAxes = localAxes.Abs();
        }

        public void SetVibrationValue(float t)
        {
            CurrentVibration = Mathf.Clamp01(t);
        }

        public void SetMaxDistance(float distance)
        {
            MaxDistance = Mathf.Abs(distance);
        }

        public Vector3 GetNewPosition()
        {
            float distance = CurrentVibration * MaxDistance;
            Vector3 newPosition = Vector3.Scale(UnityEngine.Random.insideUnitSphere,vibrationAxes);
            newPosition *= distance;
            return newPosition + startPositon;
        }

        private float automaticVibrationTimer;

        public void VibrateFor(float seconds, float transitionPaddingNormalized = 0.0f)
        {
            if(automaticVibrationTimer >= 0.0f)
            {
                automaticVibrationTimer += seconds;
            }else
            {
                automaticVibrationTimer = seconds;
                transitionPaddingNormalized = (transitionPaddingNormalized > 0.5f) ? 0.5f : (transitionPaddingNormalized < 0.0f) ? 0.0f : transitionPaddingNormalized;
                StartCoroutine(AutomaticVibration(transitionPaddingNormalized));
            }
        }

        public void ForceStop()
        {
            automaticVibrationTimer = 0.0f;
            SetVibrationValue(0.0f);
        }

        private IEnumerator AutomaticVibration(float transitionPaddingNormalized)
        {
            float cachedAutoTime = automaticVibrationTimer;
            float paddingTime = automaticVibrationTimer * transitionPaddingNormalized;
            float vibrateTime = automaticVibrationTimer - (paddingTime * 2);

            while(automaticVibrationTimer > GetCachedTime()-paddingTime)
            {
                yield return new WaitForEndOfFrame();
                automaticVibrationTimer -= Time.deltaTime;
                SetVibrationValue(Mathf.InverseLerp(GetCachedTime(),GetCachedTime()-paddingTime,automaticVibrationTimer));
            }

            SetVibrationValue(1);

            while (automaticVibrationTimer > GetCachedTime()-vibrateTime)
            {
                yield return new WaitForEndOfFrame();
                automaticVibrationTimer -= Time.deltaTime;
                automaticVibrationTimer = Mathf.Max(automaticVibrationTimer, 0);
            }

            while (automaticVibrationTimer > 0.0f)
            {
                yield return new WaitForEndOfFrame();
                automaticVibrationTimer -= Time.deltaTime;
                SetVibrationValue(Mathf.InverseLerp(GetCachedTime(), 0.0f, automaticVibrationTimer));
            }

            SetVibrationValue(0);

            float GetCachedTime()
            {
                if(cachedAutoTime <= automaticVibrationTimer)
                {
                    cachedAutoTime = automaticVibrationTimer;
                }

                return cachedAutoTime;
            }
        }

        private void OnDisable()
        {
            ForceStop();
        }

    }
}
