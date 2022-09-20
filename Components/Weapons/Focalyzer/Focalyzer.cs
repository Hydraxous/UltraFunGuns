using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    public class Focalyzer : UltraFunGunBase
    {
        public FocalyzerLaserController laser;
        public GameObject pylonPrefab;
        private Animator animator;

        bool throwingPylon = false;
        bool laserActive = false;



        private void Start()
        {
            HydraLoader.prefabRegistry.TryGetValue("FocalyzerPylon", out pylonPrefab);
            HydraLoader.prefabRegistry.TryGetValue("FocalyzerLaser", out GameObject laserPrefab);
            laser = GameObject.Instantiate<GameObject>(laserPrefab, Vector3.zero, Quaternion.identity).GetComponent<FocalyzerLaserController>();
            laser.focalyzer = this;
        }

        public override void GetInput()
        {
            if (MonoSingleton<InputManager>.Instance.InputSource.Fire1.IsPressed && !throwingPylon && actionCooldowns["fireLaser"].CanFire() && !om.paused)
            {
                laserActive = true;
                FireLaser();
            }else if (laserActive)
            {
                laserActive = false;
                actionCooldowns["fireLaser"].AddCooldown();
            }

            if (MonoSingleton<InputManager>.Instance.InputSource.Fire2.WasPerformedThisFrame && actionCooldowns["throwPylon"].CanFire())
            {
                if (!om.paused && laser.PylonCount() < 3 && !throwingPylon)
                {
                    StartCoroutine(ThrowPylon());
                }
            }
        }

        public override void DoAnimations()
        {
            laser.laserActive = laserActive;
        }

        public void FireLaser()
        {
            Vector3 laserVector = mainCam.TransformDirection(0, 0, 1);
            RaycastHit[] hits = Physics.RaycastAll(mainCam.transform.position, laserVector, 200.0f, 117460224);
            if (hits.Length > 0)
            {
                int endingHit = 0;

                for (int i = 0; i < hits.Length; i++)
                {
                    endingHit = i;

                    if (hits[i].collider.gameObject.TryGetComponent<EnemyIdentifierIdentifier>(out EnemyIdentifierIdentifier Eii))
                    {
                        if (actionCooldowns["damageTick"].CanFire())
                        {
                            actionCooldowns["damageTick"].AddCooldown();
                            Eii.eid.DeliverDamage(Eii.eid.gameObject, laserVector, hits[i].point, 0.1f, false);
                        }
                    }

                    if (hits[i].collider.gameObject.TryGetComponent<ThrownEgg>(out ThrownEgg egg))
                    {
                        egg.Explode();
                        break;
                    }

                    if (hits[i].collider.gameObject.TryGetComponent<Grenade>(out Grenade grenade))
                    {
                        grenade.Explode();
                        break;
                    }

                    if (hits[i].collider.gameObject.TryGetComponent<FocalyzerPylon>(out FocalyzerPylon pylon))
                    {
                        pylon.Refract(hits[i].point, firePoint.position);
                        break;
                    }
                }
                DrawLaser(firePoint.position, hits[endingHit].point, hits[endingHit].normal);
            }
        }

        private void DrawLaser(Vector3 origin, Vector3 endPoint, Vector3 normal)
        {
            laser.AddLinePosition(origin);
            laser.AddLinePosition(endPoint);
            laser.BuildLine(normal);
        }

        IEnumerator ThrowPylon()
        {
            throwingPylon = true;
            actionCooldowns["throwPylon"].AddCooldown();
            animator.Play("ThrowPylon");
            yield return new WaitForSeconds(0.3f);
            GameObject newPylon = GameObject.Instantiate<GameObject>(pylonPrefab, mainCam.TransformPoint(0, 0, 1), Quaternion.identity);
            newPylon.GetComponent<Rigidbody>().AddForce(mainCam.TransformDirection(0, 0, 1)*100.0f);
            throwingPylon = false;
        }

        public override Dictionary<string, ActionCooldown> SetActionCooldowns()
        {
            Dictionary<string, ActionCooldown> cooldowns = new Dictionary<string, ActionCooldown>();
            cooldowns.Add("fireLaser", new ActionCooldown(0.16f));
            cooldowns.Add("damageTick", new ActionCooldown(0.25f));
            cooldowns.Add("throwPylon", new ActionCooldown(1.0f));
            return cooldowns;
        }

        private void OnDisable()
        {
            laserActive = false;
            laser.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            laser.gameObject.SetActive(true);
        }
    }
}
