using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;

namespace UltraFunGuns
{
    [HarmonyPatch]
    public static class RevolverBeamPatches
    {
        [HarmonyPatch(typeof(RevolverBeam), nameof(RevolverBeam.ExecuteHits)), HarmonyPrefix]
        public static bool OnExecuteHits(RevolverBeam __instance, ref RaycastHit currentHit)
        {
            if (currentHit.transform == null)
                return true;

            //Debug.Log($"EXEC HIT: {currentHit.transform.name}");

            if (!currentHit.transform.TryFindComponent<IRevolverBeamShootable>(out IRevolverBeamShootable beamInteractable))
                    return true;

            if (!beamInteractable.CanRevolverBeamHit(__instance, ref currentHit))
                return true;

            beamInteractable.OnRevolverBeamHit(__instance, ref currentHit);
            return false;
        }

        public static void SetShotHitPoint(this RevolverBeam instance, Vector3 point)
        {
            FieldInfo shotHitField = typeof(RevolverBeam).GetField("shotHitPoint", BindingFlags.NonPublic | BindingFlags.Instance);
            
            if (shotHitField == null)
            {
                throw new Exception("Could not find shotHitPoint field in RevolverBeam. Possible outdated UFG or Game Version.");
            }

            shotHitField.SetValue(instance, point);
        }


        //the original method but tweaked with our values. This has some performance overhead but it's not too bad.
        [HarmonyPatch(typeof(RevolverBeam), "RicochetAimAssist"), HarmonyPrefix]
        public static bool OnRicochetAimAssist(RevolverBeam __instance, GameObject beam)
        {
            bool usedInterface = false;
            //Debug.Log($"RicochetAimAssist patch exec");

            RaycastHit[] hits = Physics.SphereCastAll(beam.transform.position, 5f, beam.transform.forward, float.PositiveInfinity, LayerMaskDefaults.Get(LMD.Enemies));
            if (hits == null || hits.Length == 0)
                return true;

            //Debug.Log($"HITS: {hits.Length}");


            Vector3 worldPosition = beam.transform.forward * 1000f;
            float closestDistance = float.PositiveInfinity;
            GameObject targetedObject = null;
            bool coinFound = false;
            RaycastHit selectedRay = new RaycastHit();

            for (int i = 0; i < hits.Length; i++)
            {
                bool currentHitIsCoin = MonoSingleton<CoinList>.Instance.revolverCoinsList.Count > 0 && hits[i].transform.TryGetComponent<Coin>(out Coin coin) && (!coin.shot || coin.shotByEnemy);
                EnemyIdentifierIdentifier enemyIdentifierIdentifier;
                bool newTargetValid = (!coinFound || currentHitIsCoin);
                bool environmentBlocked = Physics.Raycast(beam.transform.position, hits[i].point - beam.transform.position, hits[i].distance, LayerMaskDefaults.Get(LMD.Environment));
                bool hitIsValidEnemy = (hits[i].transform.TryGetComponent<EnemyIdentifierIdentifier>(out enemyIdentifierIdentifier) && enemyIdentifierIdentifier.eid && !enemyIdentifierIdentifier.eid.dead);

                bool hitIsValidRicochetInterface = hits[i].transform.TryFindComponent<ISharpshooterTarget>(out ISharpshooterTarget sharpshooterTarget) && sharpshooterTarget.CanBeSharpshot(__instance, hits[i]);

                if (newTargetValid && (hits[i].distance <= closestDistance || newTargetValid) && (hits[i].distance >= 0.1f || currentHitIsCoin) && !environmentBlocked && (currentHitIsCoin || hitIsValidEnemy || hitIsValidRicochetInterface))
                {
                    if (currentHitIsCoin)
                    {
                        coinFound = true;
                    }

                    if (currentHitIsCoin)
                        worldPosition = hits[i].transform.position;
                    else if (hitIsValidRicochetInterface)
                        worldPosition = sharpshooterTarget.GetSharpshooterTargetPoint();
                    else
                        worldPosition = hits[i].point;

                    closestDistance = hits[i].distance;
                    targetedObject = hits[i].transform.gameObject;
                    selectedRay = hits[i];
                }
            }

            if (targetedObject)
            {
                bool isValidInterface = targetedObject.TryFindComponent<ISharpshooterTarget>(out ISharpshooterTarget sharpshooterTarget);
                if (!isValidInterface)
                    return true;
                
                usedInterface = true;
                beam.transform.LookAt(worldPosition);
                sharpshooterTarget.OnSharpshooterTargeted(__instance, selectedRay);
            }

            return !usedInterface;
        }
    }
}
