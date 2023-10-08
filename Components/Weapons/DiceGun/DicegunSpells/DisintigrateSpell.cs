using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static UnityEngine.UI.Image;

namespace UltraFunGuns
{
    public class DisintigrateSpell : DiceGunSpell
    {
        //[UFGAsset("DisintigrateRayFX")] private static GameObject p_disintigrateRay;
        //[UFGAsset("DisintigrateMuzzleFX")] private static GameObject p_disintigrateMuzzleFlash;
        //[UFGAsset("DisintigrateHitFX")] private static GameObject p_disintigrateHit;
        //[UFGAsset("DisintigrateVaporizeFX")] private static GameObject p_disintigrateVaporize;

        [Configgy.Configgable("UltraFunGuns/Weapons/Deterministic Observer/Outcomes/Disintigrate")]
        private static float maxRange = 1000f;

        [Configgy.Configgable("UltraFunGuns/Weapons/Deterministic Observer/Outcomes/Disintigrate")]
        private static float damagePerLevel = 0.25f;

        [Configgy.Configgable("UltraFunGuns/Weapons/Deterministic Observer/Outcomes/Disintigrate")]
        private static float powerDivisor = 38.5f;
        protected override void ExecuteSpellCore(DiceGun diceGun)
        {
            Ray logicRay = new Ray(diceGun.CameraTransform.position, diceGun.CameraTransform.forward);
            Ray visualRay = new Ray(diceGun.FirePoint.position, diceGun.FirePoint.forward);
            Vector3 endpoint = logicRay.GetPoint(maxRange);

            //Critical failure. miss everything
            if (spellPower <= 0)
            {
                if (Physics.Raycast(logicRay, out RaycastHit environmentHit, maxRange, LayerMaskDefaults.Get(LMD.Environment)))
                    endpoint = environmentHit.point;

                //DrawDirectionFX(p_disintigrateMuzzleFlash, rayHit.point, rayHit.normal);
                DrawLine(visualRay.origin, endpoint);
                return;
            }

            //If cast misses, dont bother
            if (!Physics.Raycast(logicRay, out RaycastHit hit, maxRange, LayerMaskDefaults.Get(LMD.EnemiesAndEnvironment)))
            {
                //DrawDirectionFX(p_disintigrateMuzzleFlash, rayHit.point, rayHit.normal);
                DrawLine(visualRay.origin, endpoint);
                return;
            }

            RaycastHit[] hits = Physics.RaycastAll(logicRay, maxRange, LayerMaskDefaults.Get(LMD.EnemiesAndEnvironment));

            foreach (RaycastHit rayHit in hits.OrderBy(x => x.distance))
            {
                if (rayHit.collider.IsColliderEnvironment())
                {
                    //DrawDirectionFX(p_disintigrateMuzzleFlash, rayHit.point, rayHit.normal);
                    DrawLine(visualRay.origin, rayHit.point);
                    //DrawDirectionFX(p_disintigrateHitFx, rayHit.point, rayHit.normal);
                    return;
                }

                if (!hit.collider.IsColliderEnemy(out EnemyIdentifier eid))
                    continue;

                if (eid.dead)
                    continue;

                float damage = CalculateDamage(spellPower);
                float health = eid.Override().GetHealth();

                if(health - damage <= 0f || spellPower >= 19)
                {
                    //Destory next frame bc we want death events to trigger.
                    int frameCount = 0;
                    eid.InstaKill();
                    DrawVaporizeEffect(hit.point);
                    eid.gameObject.AddComponent<BehaviourRelay>()
                        .OnUpdate += (g) =>
                        {
                            if (frameCount == 1)
                            {
                                GameObject.Destroy(g);
                            }
                            ++frameCount;
                        };

                    return;
                }

                eid.DeliverDamage(rayHit.collider.gameObject, -rayHit.normal, rayHit.point, damage, false, 0f, diceGun.gameObject);
                return;
            }
        }

        private float CalculateDamage(int power)
        {
            return (power * (power+power)) / 38.5f;
        }

        private void DrawVaporizeEffect(Vector3 origin)
        {
            //GameObject vaporizeEffect = GameObject.Instantiate(p_disintigrateVaporize, origin, Quaternion.identity);
        }

        

        private void DrawLine(Vector3 origin, Vector3 end)
        {
            /*
            GameObject lineObject = GameObject.Instantiate(p_disintigrateRay, origin, Quaternion.identity);
            LineRenderer lineRenderer = lineObject.GetComponent<LineRenderer>();
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, origin);
            lineRenderer.SetPosition(1, end);
            */
        }

        private void DrawDirectionFX(GameObject prefab, Vector3 position, Vector3 direction)
        {
            GameObject newDirectedFX = GameObject.Instantiate(prefab, position, Quaternion.LookRotation(direction));
        }
    }
}
