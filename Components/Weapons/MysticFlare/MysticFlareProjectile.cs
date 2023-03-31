﻿using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    public class MysticFlareProjectile : MonoBehaviour
    {
        [UFGAsset("MysticFlareExplosion")] private static GameObject mysticFlareExplosion;
        private float moveSpeed = 1.0f;
        private bool dying = false;

        private void FixedUpdate()
        {
            transform.position += transform.forward * moveSpeed * Time.fixedDeltaTime;
            moveSpeed += (Time.fixedDeltaTime*2.0f);
        }

        public void Detonate()
        {
            if (dying)
                return;
            dying = true;
            Vector3 pos = transform.position;
            SonicReverberator.vineBoom_Loudest.PlayAudioClip(pos, -0.7f, 1.0f, 0.0f);
            StaticCoroutine.DelayedExecute(() => { SonicReverberator.vineBoom_Loudest.PlayAudioClip(pos, 0.8f, 1.0f, 0.0f); }, 0.24f);
            Instantiate(mysticFlareExplosion, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}