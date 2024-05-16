using System.Linq;
using UnityEngine;

namespace UltraFunGuns.Components.Audio
{
    //Adapted from SandboxProp and PhysicsSounds which I assume were written by PITR
    public class PhysicAudio : MonoBehaviour
    {
        private Rigidbody rb;

        [SerializeField] private AudioClip[] impactAudio;

        private float timeSinceLastImpact;

        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            if (rb == null)
                Destroy(this);

            impactAudio = impactAudio.Where(x => x != null).ToArray();
        }

        private void Update()
        {
            timeSinceLastImpact += Time.deltaTime;
        }
        
        public void ImpactAt(Vector3 point, float magnitude)
        {
            if (magnitude < 3.5f)
            {
                return;
            }

            AudioSource audioSource = new GameObject($"physAudio ({gameObject.name})").AddComponent<AudioSource>();
            audioSource.transform.position = point;
            audioSource.clip = impactAudio[UnityEngine.Random.Range(0, impactAudio.Length)];
            audioSource.volume = Mathf.Lerp(0.2f, 1f, magnitude / 60f);
            audioSource.pitch = Mathf.Lerp(0.65f, 2.2f, (60f - magnitude) / 60f);
            audioSource.spatialBlend = 1.0f;
            audioSource.gameObject.SetActive(true);
            audioSource.Play();

            audioSource.gameObject.AddComponent<DestroyAfterTime>();
        }


        private void OnCollisionEnter(Collision other)
        {
            if (rb.isKinematic)
            {
                return;
            }
            if (other.impulse.magnitude < 3f)
            {
                return;
            }
            if (timeSinceLastImpact < 0.1f)
            {
                return;
            }
            timeSinceLastImpact = 0f;
            ImpactAt(other.GetContact(0).point, other.impulse.magnitude);
        }
    }
}
