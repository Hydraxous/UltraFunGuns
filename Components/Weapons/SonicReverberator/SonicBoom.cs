using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UltraFunGuns
{
    public class SonicBoom : MonoBehaviour
    {
        [SerializeField] MeshRenderer[] quads;

        public float Size, Range, LifeTime;

        private Vector3 startPosition, startScale;
        private float fallOffRange;
        private float currentTime;

        private void Start()
        {
            startPosition = transform.position;
            startScale = transform.localScale;
            currentTime = LifeTime;
            fallOffRange = Range * 1.5f;
        }

        private void Update()
        {
            float lifeLeft = GetLifeRemaining();

            if(lifeLeft >= 1.0f)
            {
                Destroy(gameObject);
                return;
            }

            UpdateMeshRenderers();

            transform.position = Vector3.Lerp(startPosition, startPosition + (transform.forward * fallOffRange), lifeLeft);
            transform.localScale = Vector3.one * Size;
            currentTime -= Time.deltaTime;
        }

        public float GetLifeRemaining()
        {
            LifeTime = (LifeTime == 0) ? 1 : LifeTime; //Stop divide by zero
            return 1 - (currentTime / LifeTime);
        }

        private void UpdateMeshRenderers()
        {
            float t = GetLifeRemaining();
            foreach(var render in quads)
            {
                if (render == null)
                    continue;
                Color col = render.material.color;
                col.a = Mathf.Lerp(1.0f, 0.0f, t);
                render.material.color = col;
            }
        }

    }
}