using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using UnityEngine;

namespace UltraFunGuns.Patches
{

    //This is 99% game code, only added check for ufg stuff. This probably will break if another mod patches this.
    [HarmonyPatch(typeof(RevolverBeam))]
    public static class RevolverBeamPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("PiercingShotCheck")]
        public static bool PiercePrefix(RevolverBeam __instance, ref int ___enemiesPierced, ref bool ___fadeOut, ref int ___currentHits, bool ___hasHitProjectile, LineRenderer ___lr)
        {
            if (___enemiesPierced >= __instance.hitList.Count)
            {
                ___enemiesPierced = 0;
                ___fadeOut = true;
                return false;
            }
            if (__instance.hitList[___enemiesPierced].transform == null)
            {
                ___enemiesPierced++;
                __instance.Invoke("PiercingShotCheck", 0.0f);
                return false;
            }
            if (__instance.hitList[___enemiesPierced].transform.gameObject.tag == "Armor" || (__instance.ricochetAmount > 0 && (__instance.hitList[___enemiesPierced].transform.gameObject.layer == 8 || __instance.hitList[___enemiesPierced].transform.gameObject.layer == 24 || __instance.hitList[___enemiesPierced].transform.gameObject.layer == 0)))
            {
                bool flag = __instance.hitList[___enemiesPierced].transform.gameObject.tag != "Armor";
                GameObject gameObject = GameObject.Instantiate<GameObject>(__instance.gameObject, __instance.hitList[___enemiesPierced].rrhit.point, __instance.transform.rotation);
                gameObject.transform.forward = Vector3.Reflect(__instance.transform.forward, __instance.hitList[___enemiesPierced].rrhit.normal);
                ___lr.SetPosition(1, __instance.hitList[___enemiesPierced].rrhit.point);
                RevolverBeam component = gameObject.GetComponent<RevolverBeam>();
                component.noMuzzleflash = true;
                component.alternateStartPoint = Vector3.zero;
                component.bodiesPierced = __instance.bodiesPierced;
                component.previouslyHitTransform = __instance.hitList[___enemiesPierced].transform;
                component.aimAssist = true;
                component.intentionalRicochet = flag;
                if (flag)
                {
                    __instance.ricochetAmount--;
                    component.maxHitsPerTarget++;
                    component.hitEids.Clear();
                }
                component.ricochetAmount = __instance.ricochetAmount;
                GameObject gameObject2 = GameObject.Instantiate<GameObject>(__instance.ricochetSound, __instance.hitList[___enemiesPierced].rrhit.point, Quaternion.identity);
                gameObject2.SetActive(false);
                gameObject.SetActive(false);
                MonoSingleton<DelayedActivationManager>.Instance.Add(gameObject, 0.1f);
                MonoSingleton<DelayedActivationManager>.Instance.Add(gameObject2, 0.1f);
                Glass glass;
                if (__instance.hitList[___enemiesPierced].transform.gameObject.TryGetComponent<Glass>(out glass) && !glass.broken)
                {
                    glass.Shatter();
                }
                Breakable breakable;
                if (__instance.hitList[___enemiesPierced].transform.gameObject.TryGetComponent<Breakable>(out breakable))
                {
                    breakable.Break();
                }
                ___fadeOut = true;
                ___enemiesPierced = __instance.hitList.Count;
                return false;
            }
            if (__instance.hitList[___enemiesPierced].transform.gameObject.tag == "Coin" && __instance.bodiesPierced < __instance.hitAmount)
            {
                Coin component2 = __instance.hitList[___enemiesPierced].transform.gameObject.GetComponent<Coin>();
                if (component2 == null)
                {
                    ___enemiesPierced++;
                    __instance.Invoke("PiercingShotCheck", 0.0f);
                    return false;
                }
                ___lr.SetPosition(1, __instance.hitList[___enemiesPierced].transform.position);
                GameObject gameObject3 = GameObject.Instantiate<GameObject>(__instance.gameObject, __instance.hitList[___enemiesPierced].rrhit.point, __instance.transform.rotation);
                gameObject3.SetActive(false);
                RevolverBeam component3 = gameObject3.GetComponent<RevolverBeam>();
                component3.bodiesPierced = 0;
                component3.noMuzzleflash = true;
                component3.alternateStartPoint = Vector3.zero;
                component3.hitEids.Clear();
                if (__instance.beamType == BeamType.Enemy)
                {
                    component2.ignoreBlessedEnemies = true;
                    component3.deflected = true;
                }
                component2.DelayedReflectRevolver(__instance.hitList[___enemiesPierced].rrhit.point, gameObject3);
                ___fadeOut = true;
                return false;
            }
            else if ((__instance.hitList[___enemiesPierced].transform.gameObject.layer == 10 || __instance.hitList[___enemiesPierced].transform.gameObject.layer == 11) && __instance.hitList[___enemiesPierced].transform.gameObject.tag != "Breakable" && __instance.bodiesPierced < __instance.hitAmount)
            {

                IUFGInteractionReceiver ufgObject = __instance.hitList[___enemiesPierced].transform.gameObject.GetComponentInParent<IUFGInteractionReceiver>();

                if (ufgObject != null)
                {
                    if (__instance.bodiesPierced < __instance.hitAmount)
                    {
                        __instance.ExecuteHits(__instance.hitList[___enemiesPierced].rrhit);
                        ___fadeOut = true; //TODO check this.
                                           //if(!ufgObject.CanPierce(__instance))
                        return false;
                    }
                }

                EnemyIdentifierIdentifier componentInParent = __instance.hitList[___enemiesPierced].transform.gameObject.GetComponentInParent<EnemyIdentifierIdentifier>();
                if (!componentInParent)
                {
                    if (__instance.attributes.Length != 0)
                    {
                        AttributeChecker component4 = __instance.hitList[___enemiesPierced].transform.GetComponent<AttributeChecker>();
                        if (component4)
                        {
                            HitterAttribute[] array = __instance.attributes;
                            for (int i = 0; i < array.Length; i++)
                            {
                                if (array[i] == component4.targetAttribute)
                                {
                                    component4.DelayedActivate(0.5f);
                                    break;
                                }
                            }
                        }
                    }
                    ___enemiesPierced++;
                    ___currentHits = 0;
                    __instance.Invoke("PiercingShotCheck", 0.0f);
                    return false;
                }
                EnemyIdentifier eid = componentInParent.eid;
                if (!(eid != null))
                {
                    __instance.ExecuteHits(__instance.hitList[___enemiesPierced].rrhit);
                    ___enemiesPierced++;
                    __instance.Invoke("PiercingShotCheck", 0.0f);
                    return false;
                }
                if ((__instance.hitEids.Contains(eid) && (!eid.dead || __instance.beamType != BeamType.Revolver || ___enemiesPierced != __instance.hitList.Count - 1)) || (__instance.beamType == BeamType.Enemy && !__instance.deflected && (eid.enemyType == __instance.ignoreEnemyType || EnemyIdentifier.CheckHurtException(eid.enemyType, __instance.ignoreEnemyType))))
                {
                    ___enemiesPierced++;
                    ___currentHits = 0;
                    __instance.Invoke("PiercingShotCheck", 0.0f);
                    return false;
                }
                bool flag2 = false;
                if (eid.dead)
                {
                    flag2 = true;
                }
                __instance.ExecuteHits(__instance.hitList[___enemiesPierced].rrhit);
                if (!flag2 || __instance.hitList[___enemiesPierced].transform.gameObject.layer == 11 || (__instance.beamType == BeamType.Revolver && ___enemiesPierced == __instance.hitList.Count - 1))
                {
                    ___currentHits++;
                    __instance.bodiesPierced++;
                    GameObject.Instantiate<GameObject>(__instance.hitParticle, __instance.hitList[___enemiesPierced].rrhit.point, __instance.transform.rotation);
                    MonoSingleton<TimeController>.Instance.HitStop(0.05f);
                }
                else
                {
                    if (__instance.beamType == BeamType.Revolver)
                    {
                        __instance.hitEids.Add(eid);
                    }
                    ___enemiesPierced++;
                    ___currentHits = 0;
                }
                if (___currentHits >= __instance.maxHitsPerTarget)
                {
                    __instance.hitEids.Add(eid);
                    ___currentHits = 0;
                    ___enemiesPierced++;
                }
                if (__instance.beamType == BeamType.Revolver && !flag2)
                {
                    __instance.Invoke("PiercingShotCheck", 0.05f);
                    return false;
                }
                if (__instance.beamType == BeamType.Revolver)
                {
                    __instance.Invoke("PiercingShotCheck", 0.0f);
                    return false;
                }
                if (!flag2)
                {
                    __instance.Invoke("PiercingShotCheck", 0.025f);
                    return false;
                }
                __instance.Invoke("PiercingShotCheck", 0.01f);
                return false;
            }
            else
            {
                if (__instance.canHitProjectiles && __instance.hitList[___enemiesPierced].transform.gameObject.layer == 14)
                {
                    if (!___hasHitProjectile)
                    {
                        __instance.Invoke("PiercingShotCheck", 0.01f);
                    }
                    else
                    {
                        MonoSingleton<TimeController>.Instance.HitStop(0.05f);
                        __instance.Invoke("PiercingShotCheck", 0.05f);
                    }
                    __instance.ExecuteHits(__instance.hitList[___enemiesPierced].rrhit);
                    ___enemiesPierced++;
                    return false;
                }
                if (__instance.hitList[___enemiesPierced].transform.gameObject.tag == "Glass" || __instance.hitList[___enemiesPierced].transform.gameObject.tag == "GlassFloor")
                {
                    Glass component5 = __instance.hitList[___enemiesPierced].transform.gameObject.GetComponent<Glass>();
                    if (!component5.broken)
                    {
                        component5.Shatter();
                    }
                    ___enemiesPierced++;
                    __instance.Invoke("PiercingShotCheck", 0.0f);
                    return false;
                }
                if (__instance.beamType == BeamType.Enemy && __instance.hitList[___enemiesPierced].transform.gameObject.CompareTag("Player") && __instance.bodiesPierced < __instance.hitAmount)
                {
                    __instance.ExecuteHits(__instance.hitList[___enemiesPierced].rrhit);
                    __instance.bodiesPierced++;
                    ___enemiesPierced++;
                    __instance.Invoke("PiercingShotCheck", 0.0f);
                    return false;
                }
                Breakable component6 = __instance.hitList[___enemiesPierced].transform.GetComponent<Breakable>();
                if (component6 != null && (__instance.beamType == BeamType.Railgun || component6.weak))
                {
                    if (component6.interrupt)
                    {
                        MonoSingleton<StyleHUD>.Instance.AddPoints(100, "ultrakill.interruption", __instance.sourceWeapon, null, -1, "", "");
                        MonoSingleton<TimeController>.Instance.ParryFlash();
                        if (__instance.canHitProjectiles)
                        {
                            component6.breakParticle = MonoSingleton<DefaultReferenceManager>.Instance.superExplosion;
                        }
                        if (component6.interruptEnemy && !component6.interruptEnemy.blessed)
                        {
                            component6.interruptEnemy.Explode();
                        }
                    }
                    component6.Break();
                }
                else if (__instance.bodiesPierced < __instance.hitAmount)
                {
                    __instance.ExecuteHits(__instance.hitList[___enemiesPierced].rrhit);
                }
                GameObject.Instantiate<GameObject>(__instance.hitParticle, __instance.hitList[___enemiesPierced].rrhit.point, Quaternion.LookRotation(__instance.hitList[___enemiesPierced].rrhit.normal));
                ___enemiesPierced++;
                __instance.Invoke("PiercingShotCheck", 0.0f);
                return false;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(RevolverBeam.ExecuteHits))]
        public static void ExecuteHitsPrefix(RevolverBeam __instance, RaycastHit currentHit)
        {
            try
            {
                if (currentHit.transform.TryGetComponent<IUFGInteractionReceiver>(out IUFGInteractionReceiver ufgObject))
                {
                    ufgObject.Shot(__instance.beamType);
                }
            }
            catch (System.Exception e)
            {
                HydraLogger.Log($"Revolver Hit Error: {e.Message}\n{e.StackTrace}", DebugChannel.Error);
            }

        }

        [HarmonyPrefix]
        [HarmonyPatch("RicochetAimAssist")]
        public static bool RicochetAimPrefix(RevolverBeam __instance, GameObject beam, bool aimAtHead = false)
        {
            RaycastHit[] array = Physics.SphereCastAll(beam.transform.position, 5f, beam.transform.forward, 1000f, LayerMaskDefaults.Get(LMD.Enemies));
            if (array == null || array.Length == 0)
            {
                return false;
            }
            Vector3 worldPosition = beam.transform.forward * 1000f;
            float num = 1000f;
            GameObject gameObject = null;
            bool skipCheckEnemy = false;
            for (int i = 0; i < array.Length; i++)
            {
                Coin coin;
                bool hitInteractable = MonoSingleton<CoinList>.Instance.revolverCoinsList.Count > 0 && array[i].transform.TryGetComponent<Coin>(out coin) && (!coin.shot || coin.shotByEnemy);

                if(!hitInteractable)
                {
                    if (array[i].transform.TryFindComponent<IUFGInteractionReceiver>(out IUFGInteractionReceiver ufgReciever))
                    {
                        TargetQuery targetQuery = new TargetQuery
                        {
                            queryOrigin = beam.transform.position,
                            queryDirection = beam.transform.forward,
                            checkLineOfSight = false,
                            maxRange = 1000.0f
                        };

                        if(ufgReciever.Targetable(targetQuery))
                            hitInteractable = true;
                    }
                }

                EnemyIdentifierIdentifier enemyIdentifierIdentifier;
                if ((!skipCheckEnemy || hitInteractable) && (array[i].distance <= num || (!skipCheckEnemy && hitInteractable)) && (array[i].distance >= 0.1f || hitInteractable) && !Physics.Raycast(beam.transform.position, array[i].point - beam.transform.position, array[i].distance, LayerMaskDefaults.Get(LMD.Environment)) && (hitInteractable || (array[i].transform.TryGetComponent<EnemyIdentifierIdentifier>(out enemyIdentifierIdentifier) && enemyIdentifierIdentifier.eid && !enemyIdentifierIdentifier.eid.dead)))
                {
                    if (hitInteractable)
                    {
                        skipCheckEnemy = true;
                    }
                    worldPosition = (hitInteractable ? array[i].transform.position : array[i].point);
                    num = array[i].distance;
                    gameObject = array[i].transform.gameObject;
                }
            }

            if (gameObject)
            {
                EnemyIdentifierIdentifier enemyIdentifierIdentifier2;
                if (aimAtHead && !skipCheckEnemy && (__instance.critDamageOverride != 0f || (__instance.beamType == BeamType.Revolver && !__instance.strongAlt)) && gameObject.TryGetComponent<EnemyIdentifierIdentifier>(out enemyIdentifierIdentifier2) && enemyIdentifierIdentifier2.eid && enemyIdentifierIdentifier2.eid.weakPoint && !Physics.Raycast(beam.transform.position, enemyIdentifierIdentifier2.eid.weakPoint.transform.position - beam.transform.position, Vector3.Distance(enemyIdentifierIdentifier2.eid.weakPoint.transform.position, beam.transform.position), LayerMaskDefaults.Get(LMD.Environment)))
                {
                    worldPosition = enemyIdentifierIdentifier2.eid.weakPoint.transform.position;
                }
                beam.transform.LookAt(worldPosition);
            }

            return false;
        }
    }
}
