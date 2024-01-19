using Configgy;
using CustomRay;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    [UFGWeapon("Gigahammer", "Giga Hammer", 0, true, WeaponIconColor.Red)]
    [WeaponAbility("Smash", "Press <color=orange>Fire 1</color> to smack things with a giant hammer.", 0, RichTextColors.aqua)]
    public class ColossalHammer : UltraFunGunBase
    {
        private Animator anim;

        private float charge;
        [Configgable("Weapons/GigaHammer")]
        private static float chargeRate = 0.1f;

        [Configgable("Weapons/GigaHammer")]
        private static float hitstop = 0.05f;

        [Configgable("Weapons/GigaHammer")]
        private static float camShake = 0.5f;

        [Configgable("Weapons/GigaHammer")]
        private static float maxCharge = 1f;

        [Configgable("Weapons/GigaHammer")]
        private static float maxRange = 4f;

        [Configgable("Weapons/GigaHammer")]
        private static float minCharge = 0.3f;

        [Configgable("Weapons/GigaHammer")]
        private static float fireDelay = 0.15f;

        [Configgable("Weapons/GigaHammer")]
        private static float damage = 4f;

        [Configgable("Weapons/GigaHammer")]
        private static float scalar = 1f;

        [UFGAsset("CrowbarHitEnemy")]
        private static AudioClip crowbarSound;

        public override void OnAwakeFinished()
        {
            base.OnAwakeFinished();
            anim = GetComponentInChildren<Animator>();
            source = gameObject.AddComponent<AudioSource>();
            source.clip = crowbarSound;
            source.pitch = 0.5f;
            source.spatialBlend = 0f;
            source.playOnAwake = false;
        }

        private AudioSource source;

        private bool startedCharge;
        private bool swinging;


        private void Update()
        {
            if (swinging)
            {
                swinging = !anim.GetCurrentAnimatorStateInfo(0).IsName("Idle");
                if (swinging)
                    return;
            }

            bool firePressed = InputManager.Instance.InputSource.Fire1.IsPressed;

            if (firePressed)
            {
                startedCharge = true;
                anim.SetBool("Raising", true);
            }

            if (startedCharge)
            {
                charge = Mathf.Clamp(charge+Time.deltaTime*chargeRate, 0f, maxCharge);

                if(charge > minCharge)
                {
                    if (!firePressed)
                    {
                        chargeNormalized = charge/maxCharge;
                        DelayFired();
                    }
                }else if (!firePressed)
                {
                    charge = 0f;
                    anim.SetBool("Raising", false);
                    anim.Play("Idle", 0, 0f);
                    startedCharge = false;
                }

            }

        }


        private IEnumerator FireCoroutine()
        {
            yield return new WaitForSeconds(fireDelay);
            Fire();
        }

        private void DelayFired()
        {
            anim.SetBool("Raising", false);
            anim.Play("Slam", 0, 0f);
            swinging = true;
            startedCharge = false;
            StartCoroutine(FireCoroutine());
        }

        private void OnDisable()
        {
            charge = 0f;
            startedCharge = false;
            swinging = false;
            anim.SetBool("Raising", false);
        }

        private void OnEnable()
        {
            anim.Play("Recover", 0, 0.5f);
            startedCharge = false;
            charge = 0f;
        }

        private void Fire()
        {
            hitEnemies.Clear();
            source.Play();
            CameraController.Instance.CameraShake(camShake);
            if (HydraUtils.SphereCastAllMacro(mainCam.position, 0.75f, mainCam.forward, maxRange, out RaycastHit[] hits))
            {
                foreach (RaycastHit hit in hits)
                {
                    ProcessHit(hit);
                }
            }
        }

        float chargeNormalized;

        List<EnemyIdentifier> hitEnemies = new List<EnemyIdentifier>();
        private void ProcessHit(RaycastHit hit)
        {
            if (hit.collider.TryGetComponent<Breakable>(out Breakable breakable))
            {
                breakable.Break();
            }

            if (hit.collider.TryGetComponent<Glass>(out Glass glass))
            {
                glass.Shatter();
            }

            if (hit.collider.IsColliderEnvironment())
            {
                GameObject bulletDecal = Instantiate(Prefabs.BulletImpactFX, hit.point + (hit.normal * 0.01f), Quaternion.identity);
                bulletDecal.transform.up = hit.normal;
                return;
            }

            if (!hit.collider.IsColliderEnemy(out EnemyIdentifier eid, false))
                return;

            if (hitEnemies.Contains(eid))
                return;

            eid.transform.localScale = Vector3.Scale(new Vector3(1f, scalar*(1-chargeNormalized), 1f), eid.transform.localScale);
            GameObject gore = BloodsplatterManager.Instance.GetGore(GoreType.Body, eid);
            gore.transform.position = hit.point;
            gore.transform.localScale *= 2f;
            gore.transform.SetParent(GoreZone.ResolveGoreZone(eid.transform).goreZone, true);
            eid.DeliverDamage(eid.gameObject, mainCam.forward * 5000.0f, hit.point, damage, true, 1.0f, gameObject);
            TimeController.Instance.HitStop(hitstop);
            hitEnemies.Add(eid);
        }
      
    }
}
