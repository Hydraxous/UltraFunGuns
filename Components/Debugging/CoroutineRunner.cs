using System;
using System.Collections;
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

        public static void DelayedExecute(Action action, float delayInSeconds)
        {
            RunCoroutine(DelayedExecution(action, delayInSeconds));
        }

        private static IEnumerator DelayedExecution(Action action, float timeInSeconds)
        {
            yield return new WaitForSecondsRealtime(timeInSeconds);
            action?.Invoke();
        }
    }
}
