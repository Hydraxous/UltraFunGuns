using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using UnityEngine;

namespace UltraFunGuns
{
    [HarmonyPatch(typeof(RevolverBeam), "ExecuteHits")]
    public static class ExecuteHitsPatch
    {
        public static void Prefix(RevolverBeam __instance, RaycastHit currentHit)
        {
            try
            {
                if (!currentHit.transform.TryGetComponent<IUFGInteractionReceiver>(out IUFGInteractionReceiver ufgObject))
                {
                    ufgObject = currentHit.transform.GetComponentInParent<IUFGInteractionReceiver>();
                }

                if (ufgObject != null)
                {
                    ufgObject.Shot(__instance.beamType);

                }

                ThrownDodgeball hitDodgeball = currentHit.transform.GetComponentInParent<ThrownDodgeball>();
                if (hitDodgeball != null)
                {
                    switch (__instance.beamType)
                    {
                        case BeamType.Railgun:
                            hitDodgeball.ExciteBall(6);
                            break;

                        case BeamType.Revolver:
                            hitDodgeball.ExciteBall();
                            break;

                        case BeamType.MaliciousFace:
                            hitDodgeball.ExciteBall(2);
                            break;

                        case BeamType.Enemy:
                            break;
                    }

                }

                CanProjectile hitCan = currentHit.transform.GetComponentInParent<CanProjectile>();
                if (hitCan != null)
                {
                    HydraLogger.Log($"{hitCan.name} was shot!");
                    switch (__instance.beamType)
                    {
                        case BeamType.Railgun:
                            hitCan.Explode(Vector3.up, 3);
                            break;
                        case BeamType.Revolver:
                            hitCan.Bounce();
                            break;
                        case BeamType.MaliciousFace:
                            hitCan.Explode(Vector3.up, 3);
                            break;
                        case BeamType.Enemy:
                            hitCan.Bounce();
                            break;
                    }
                }

                RemoteBombExplosive remoteBombExplosive = currentHit.transform.GetComponentInParent<RemoteBombExplosive>();
                if (remoteBombExplosive != null)
                {

                    switch (__instance.beamType)
                    {
                        case BeamType.Railgun:
                            remoteBombExplosive.Detonate(true);
                            break;
                        case BeamType.Revolver:
                            remoteBombExplosive.Detonate(false);
                            break;
                        case BeamType.MaliciousFace:
                            remoteBombExplosive.Detonate(true);
                            break;
                        case BeamType.Enemy:
                            remoteBombExplosive.Detonate(true);
                            break;
                    }
                }
            }
            catch (System.Exception e)
            {
                HydraLogger.Log($"Revolver Hit Error: {e.Message}\n{e.StackTrace}", DebugChannel.Error);
            }

        }

    }

    //this physically hurts me
    [HarmonyPatch(typeof(RevolverBeam), "PiercingShotCheck")]
    public static class PierceCheckPatch
    {
        public static bool Prefix(RevolverBeam __instance, ref int ___enemiesPierced, ref int ___currentHits, ref List<RevolverBeam.RaycastResult> ___hitList, ref LineRenderer ___lr, ref bool ___fadeOut, ref List<EnemyIdentifier> ___hitEids)
        {
            if (___enemiesPierced < ___hitList.Count)
            {
                if (___hitList[___enemiesPierced].transform == null)
                {
                    ___enemiesPierced++;
                    __instance.Invoke("PiercingShotCheck", 0.0f);
                }
                else
                {
                    if (___hitList[___enemiesPierced].transform.gameObject.tag == "Armor")
                    {
                        ___lr.SetPosition(1, ___hitList[___enemiesPierced].rrhit.point);
                        GameObject gameObject = GameObject.Instantiate<GameObject>(__instance.gameObject, ___hitList[___enemiesPierced].rrhit.point, __instance.transform.rotation);
                        gameObject.transform.forward = Vector3.Reflect(__instance.transform.forward, ___hitList[___enemiesPierced].rrhit.normal);
                        RevolverBeam component = gameObject.GetComponent<RevolverBeam>();
                        component.noMuzzleflash = true;
                        component.alternateStartPoint = Vector3.zero;
                        component.bodiesPierced = __instance.bodiesPierced;
                        GameObject.Instantiate<GameObject>(__instance.ricochetSound, ___hitList[___enemiesPierced].rrhit.point, Quaternion.identity);
                        ___fadeOut = true;
                        ___enemiesPierced = ___hitList.Count;
                    }
                    else
                    {
                        if (___hitList[___enemiesPierced].transform.gameObject.tag == "Coin" && __instance.bodiesPierced < __instance.hitAmount)
                        {
                            Coin component2 = ___hitList[___enemiesPierced].transform.gameObject.GetComponent<Coin>();
                            bool flag5 = component2 == null;
                            if (flag5)
                            {
                                ___enemiesPierced++;
                                __instance.Invoke("PiercingShotCheck", 0.0f);
                            }
                            else
                            {
                                ___lr.SetPosition(1, ___hitList[___enemiesPierced].transform.position);
                                GameObject gameObject2 = GameObject.Instantiate<GameObject>(__instance.gameObject, ___hitList[___enemiesPierced].rrhit.point, __instance.transform.rotation);
                                gameObject2.SetActive(false);
                                RevolverBeam component3 = gameObject2.GetComponent<RevolverBeam>();
                                component3.bodiesPierced = 0;
                                component3.noMuzzleflash = true;
                                component3.alternateStartPoint = Vector3.zero;
                                component3.hitEids.Clear();
                                Debug.Log("Beam hit coin: " + ___hitList[___enemiesPierced].transform.gameObject.name, ___hitList[___enemiesPierced].transform.gameObject);
                                component2.DelayedReflectRevolver(___hitList[___enemiesPierced].rrhit.point, gameObject2);
                                if (__instance.beamType == BeamType.Enemy)
                                {
                                    component3.deflected = true;
                                }
                                ___fadeOut = true;
                            }
                        }
                        else
                        {
                            bool flag7 = (___hitList[___enemiesPierced].transform.gameObject.layer == 10 || ___hitList[___enemiesPierced].transform.gameObject.layer == 11) && ___hitList[___enemiesPierced].transform.gameObject.tag != "Breakable" && __instance.bodiesPierced < __instance.hitAmount;
                            if (flag7)
                            {

                                IUFGInteractionReceiver ufgObject = ___hitList[___enemiesPierced].transform.gameObject.GetComponentInParent<IUFGInteractionReceiver>();
                                EnemyIdentifierIdentifier componentInParent = ___hitList[___enemiesPierced].transform.gameObject.GetComponentInParent<EnemyIdentifierIdentifier>();

                                if (ufgObject != null)
                                {
                                    if (__instance.bodiesPierced < __instance.hitAmount)
                                    {
                                        __instance.ExecuteHits(___hitList[___enemiesPierced].rrhit);
                                        ___fadeOut = true; //TODO check this.
                                    }
                                }
                                else if (!componentInParent)
                                {
                                    bool flag9 = __instance.attributes.Length != 0;
                                    if (flag9)
                                    {
                                        AttributeChecker component4 = ___hitList[___enemiesPierced].transform.GetComponent<AttributeChecker>();
                                        bool flag10 = component4;
                                        if (flag10)
                                        {
                                            foreach (HitterAttribute hitterAttribute in __instance.attributes)
                                            {
                                                bool flag11 = hitterAttribute == component4.targetAttribute;
                                                if (flag11)
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
                                }
                                else
                                {
                                    EnemyIdentifier eid = componentInParent.eid;
                                    bool flag12 = eid != null;
                                    if (flag12)
                                    {
                                        bool flag13 = (!___hitEids.Contains(eid) || (eid.dead && __instance.beamType == BeamType.Revolver && ___enemiesPierced == ___hitList.Count - 1)) && (__instance.beamType != BeamType.Enemy || __instance.deflected || (eid.enemyType != __instance.ignoreEnemyType && !EnemyIdentifier.CheckHurtException(eid.enemyType, __instance.ignoreEnemyType)));
                                        if (flag13)
                                        {
                                            bool flag14 = false;
                                            bool dead = eid.dead;
                                            if (dead)
                                            {
                                                flag14 = true;
                                            }
                                            __instance.ExecuteHits(___hitList[___enemiesPierced].rrhit);
                                            bool flag15 = !flag14 || ___hitList[___enemiesPierced].transform.gameObject.layer == 11 || (__instance.beamType == BeamType.Revolver && ___enemiesPierced == ___hitList.Count - 1);
                                            if (flag15)
                                            {
                                                ___currentHits++;
                                                __instance.bodiesPierced++;
                                                GameObject.Instantiate<GameObject>(__instance.hitParticle, ___hitList[___enemiesPierced].rrhit.point, __instance.transform.rotation);
                                                MonoSingleton<TimeController>.Instance.HitStop(0.05f);
                                            }
                                            else
                                            {
                                                bool flag16 = __instance.beamType == BeamType.Revolver;
                                                if (flag16)
                                                {
                                                    ___hitEids.Add(eid);
                                                }
                                                ___enemiesPierced++;
                                                ___currentHits = 0;
                                            }
                                            bool flag17 = ___currentHits >= __instance.maxHitsPerTarget;
                                            if (flag17)
                                            {
                                                ___hitEids.Add(eid);
                                                ___currentHits = 0;
                                                ___enemiesPierced++;
                                            }
                                            bool flag18 = __instance.beamType == BeamType.Revolver && !flag14;
                                            if (flag18)
                                            {
                                                __instance.Invoke("PiercingShotCheck", 0.05f);
                                            }
                                            else
                                            {
                                                bool flag19 = __instance.beamType == BeamType.Revolver;
                                                if (flag19)
                                                {
                                                    __instance.Invoke("PiercingShotCheck", 0.0f);
                                                }
                                                else
                                                {
                                                    bool flag20 = !flag14;
                                                    if (flag20)
                                                    {
                                                        __instance.Invoke("PiercingShotCheck", 0.025f);
                                                    }
                                                    else
                                                    {
                                                        __instance.Invoke("PiercingShotCheck", 0.01f);
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            ___enemiesPierced++;
                                            ___currentHits = 0;
                                            __instance.Invoke("PiercingShotCheck", 0.0f);

                                        }
                                    }
                                    else
                                    {
                                        __instance.ExecuteHits(___hitList[___enemiesPierced].rrhit);
                                        ___enemiesPierced++;
                                        __instance.Invoke("PiercingShotCheck", 0.0f);

                                    }
                                }
                            }
                            else
                            {
                                bool flag21 = ___hitList[___enemiesPierced].transform.gameObject.tag == "Glass" || ___hitList[___enemiesPierced].transform.gameObject.tag == "GlassFloor";
                                if (flag21)
                                {
                                    Glass component5 = ___hitList[___enemiesPierced].transform.gameObject.GetComponent<Glass>();
                                    bool flag22 = !component5.broken;
                                    if (flag22)
                                    {
                                        component5.Shatter();
                                    }
                                    ___enemiesPierced++;
                                    __instance.Invoke("PiercingShotCheck", 0.0f);

                                }
                                else
                                {
                                    bool flag23 = __instance.beamType == BeamType.Enemy && ___hitList[___enemiesPierced].transform.gameObject.CompareTag("Player") && __instance.bodiesPierced < __instance.hitAmount;
                                    if (flag23)
                                    {
                                        __instance.ExecuteHits(___hitList[___enemiesPierced].rrhit);
                                        __instance.bodiesPierced++;
                                        ___enemiesPierced++;
                                        __instance.Invoke("PiercingShotCheck", 0.0f);

                                    }
                                    else
                                    {
                                        Breakable component6 = ___hitList[___enemiesPierced].transform.GetComponent<Breakable>();
                                        bool flag24 = component6 != null && (__instance.beamType == BeamType.Railgun || component6.weak);
                                        if (flag24)
                                        {
                                            bool interrupt = component6.interrupt;
                                            if (interrupt)
                                            {
                                                MonoSingleton<StyleHUD>.Instance.AddPoints(100, "ultrakill.interruption", __instance.sourceWeapon, null, -1, "", "");
                                                MonoSingleton<TimeController>.Instance.ParryFlash();
                                                bool flag25 = component6.interruptEnemy && !component6.interruptEnemy.blessed;
                                                if (flag25)
                                                {
                                                    component6.interruptEnemy.Explode();
                                                }
                                            }
                                            component6.Break();
                                        }
                                        else
                                        {
                                            if (__instance.bodiesPierced < __instance.hitAmount)
                                            {
                                                __instance.ExecuteHits(___hitList[___enemiesPierced].rrhit);
                                            }
                                        }
                                        GameObject.Instantiate<GameObject>(__instance.hitParticle, ___hitList[___enemiesPierced].rrhit.point, Quaternion.LookRotation(___hitList[___enemiesPierced].rrhit.normal));
                                        ___enemiesPierced++;
                                        __instance.Invoke("PiercingShotCheck", 0.0f);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                ___enemiesPierced = 0;
                ___fadeOut = true;
            }

            return false;
        }
    }
}
