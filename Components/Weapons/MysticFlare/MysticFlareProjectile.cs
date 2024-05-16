using UnityEngine;

namespace UltraFunGuns
{
    public class MysticFlareProjectile : MonoBehaviour
    {
        
        public float MoveSpeed { get; private set; } = 1.0f;
        private bool dying = false;

        private void FixedUpdate()
        {
            MoveSpeed += (Time.fixedDeltaTime);
            transform.position += transform.forward * MoveSpeed * Time.fixedDeltaTime;
            MoveSpeed += (Time.fixedDeltaTime);
        }

        public void Detonate()
        {
            if (dying)
                return;
            dying = true;
            Vector3 pos = transform.position;
            SonicReverberator.vineBoom_Loudest.PlayAudioClip(pos, -0.7f, 1.0f, 0.0f);
            StaticCoroutine.DelayedExecute(() => { SonicReverberator.vineBoom_Loudest.PlayAudioClip(pos, 0.8f, 1.0f, 0.0f); }, 0.24f);
            Instantiate(MysticFlare.MysticFlareExplosion, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}