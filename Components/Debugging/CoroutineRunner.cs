using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    public class CoroutineRunner : MonoBehaviour
    {
        private bool coroutineStarted;
        private bool coroutineRunning;

        private void Update()
        {
            if(!coroutineStarted)
            {
                return;
            }

            if(!coroutineRunning)
            {
                Destroy(gameObject);
            }
        }

        public Coroutine RunCoroutine(IEnumerator coroutine)
        {
            DontDestroyOnLoad(gameObject);
            return StartCoroutine(RunExternalCoroutine(coroutine));
        }

        private IEnumerator RunExternalCoroutine(IEnumerator coroutine)
        {
            coroutineStarted = true;
            coroutineRunning = true;
            yield return coroutine;
            coroutineRunning = false;
        }
    }

    public static class StaticCoroutine
    {
        public static Coroutine RunCoroutine(IEnumerator coroutine)
        {
            CoroutineRunner coroutineRunner = new GameObject("New Rebinder").AddComponent<CoroutineRunner>();
            return coroutineRunner.RunCoroutine(coroutine);
        }
    }
}
