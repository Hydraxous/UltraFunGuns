using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using UnityEngine;

namespace UltraFunGuns.Patches
{

    ////This is 99% game code, only added check for ufg stuff. This probably will break if another mod patches this.
    //[HarmonyPatch(typeof(RevolverBeam))]
    //public static class RevolverBeamPatch
    //{
    //    //TODO this is busted still
    //    [HarmonyPrefix]
    //    [HarmonyPatch("RicochetAimAssist")]
    //    public static bool RicochetAimPrefix(RevolverBeam __instance, GameObject beam, bool aimAtHead = false)
    //    {
    //        RaycastHit[] array = Physics.SphereCastAll(beam.transform.position, 5f, beam.transform.forward, 1000f, LayerMaskDefaults.Get(LMD.Enemies));
    //        if (array == null || array.Length == 0)
    //        {
    //            return false;
    //        }
    //        Vector3 worldPosition = beam.transform.forward * 1000f;
    //        float num = 1000f;
    //        GameObject gameObject = null;
    //        bool skipCheckEnemy = false;
    //        for (int i = 0; i < array.Length; i++)
    //        {
    //            Coin coin;
    //            bool hitInteractable = MonoSingleton<CoinList>.Instance.revolverCoinsList.Count > 0 && array[i].transform.TryGetComponent<Coin>(out coin) && (!coin.shot || coin.shotByEnemy);

    //            if(!hitInteractable)
    //            {
    //                if (array[i].transform.TryFindComponent<IUFGInteractionReceiver>(out IUFGInteractionReceiver ufgReciever))
    //                {
    //                    TargetQuery targetQuery = new TargetQuery
    //                    {
    //                        queryOrigin = beam.transform.position,
    //                        queryDirection = beam.transform.forward,
    //                        checkLineOfSight = false,
    //                        maxRange = 1000.0f
    //                    };

    //                    if(ufgReciever.Targetable(targetQuery))
    //                        hitInteractable = true;
    //                }
    //            }

    //            EnemyIdentifierIdentifier enemyIdentifierIdentifier;
    //            if ((!skipCheckEnemy || hitInteractable) && (array[i].distance <= num || (!skipCheckEnemy && hitInteractable)) && (array[i].distance >= 0.1f || hitInteractable) && !Physics.Raycast(beam.transform.position, array[i].point - beam.transform.position, array[i].distance, LayerMaskDefaults.Get(LMD.Environment)) && (hitInteractable || (array[i].transform.TryGetComponent<EnemyIdentifierIdentifier>(out enemyIdentifierIdentifier) && enemyIdentifierIdentifier.eid && !enemyIdentifierIdentifier.eid.dead)))
    //            {
    //                if (hitInteractable)
    //                {
    //                    skipCheckEnemy = true;
    //                }
    //                worldPosition = (hitInteractable ? array[i].transform.position : array[i].point);
    //                num = array[i].distance;
    //                gameObject = array[i].transform.gameObject;
    //            }
    //        }

    //        if (gameObject)
    //        {
    //            EnemyIdentifierIdentifier enemyIdentifierIdentifier2;
    //            if (aimAtHead && !skipCheckEnemy && (__instance.critDamageOverride != 0f || (__instance.beamType == BeamType.Revolver && !__instance.strongAlt)) && gameObject.TryGetComponent<EnemyIdentifierIdentifier>(out enemyIdentifierIdentifier2) && enemyIdentifierIdentifier2.eid && enemyIdentifierIdentifier2.eid.weakPoint && !Physics.Raycast(beam.transform.position, enemyIdentifierIdentifier2.eid.weakPoint.transform.position - beam.transform.position, Vector3.Distance(enemyIdentifierIdentifier2.eid.weakPoint.transform.position, beam.transform.position), LayerMaskDefaults.Get(LMD.Environment)))
    //            {
    //                worldPosition = enemyIdentifierIdentifier2.eid.weakPoint.transform.position;
    //            }
    //            beam.transform.LookAt(worldPosition);
    //        }

    //        return false;
    //    }
    //}
}
