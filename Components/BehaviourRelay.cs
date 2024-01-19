using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    public class BehaviourRelay : MonoBehaviour
    {
        public Action<GameObject> OnAwake;
        public Action<GameObject> OnStart;
        public Action<GameObject> OnUpdate;
        public Action<GameObject> OnLateUpdate;
        public Action<GameObject> OnFixedUpdate;
        public Action<GameObject> OnOnEnable;
        public Action<GameObject> OnOnDisable;
        public Action<GameObject> OnOnDestroy;

        private void Awake()
        {
            OnAwake?.Invoke(gameObject);
        }

        private void Start()
        {
            OnStart?.Invoke(gameObject);
        }

        private void Update()
        {
            OnUpdate?.Invoke(gameObject);
        }

        private void LateUpdate()
        {
            OnLateUpdate?.Invoke(gameObject);
        }

        private void FixedUpdate()
        {
            OnFixedUpdate?.Invoke(gameObject);
        }

        private void OnEnable()
        {
            OnOnEnable?.Invoke(gameObject);
        }

        private void OnDisable()
        {
            OnOnDisable?.Invoke(gameObject);
        }

        private void OnDestroy()
        {
            OnOnDestroy?.Invoke(gameObject);
        }

     

    }
}
