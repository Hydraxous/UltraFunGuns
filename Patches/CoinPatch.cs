using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using UnityEngine;

namespace UltraFunGuns
{
    [HarmonyPatch(typeof(Coin), "ReflectRevolver")]
    public static class CoinPatch
    {
        public static void Postfix(Coin __instance)
        {
            ThrownDodgeball[] foundDodgeballs = GameObject.FindObjectsOfType<ThrownDodgeball>();
            if(foundDodgeballs.Length > 0)
            {
                int closestIndex = -1;
                float closestDistance = Mathf.Infinity;
                for(int i=0;i< foundDodgeballs.Length; i++)
                {
                    Vector3 directionToTarget = foundDodgeballs[i].transform.position - __instance.transform.position;
                    if (Physics.Raycast(__instance.transform.position, directionToTarget, out RaycastHit rayHit, directionToTarget.magnitude, LayerMask.GetMask("Limb", "BigCorpse", "Outdoors", "Environment", "Default")))
                    {
                        if (rayHit.collider.GetComponent<ThrownDodgeball>() == null && rayHit.collider.GetComponentInParent<ThrownDodgeball>() == null)
                        {
                            foundDodgeballs[i] = null;
                        }else if(directionToTarget.sqrMagnitude < closestDistance)
                        {
                            closestIndex = i;
                        }
                    }
                }
                
                if(closestIndex != -1)
                {
                    LineRenderer beam = GameObject.Instantiate<GameObject>(__instance.refBeam, __instance.transform.position, Quaternion.identity).GetComponent<LineRenderer>();
                    beam.gameObject.GetComponent<RevolverBeam>().sourceWeapon = __instance.sourceWeapon;
                    

                    if (__instance.hitPoint == Vector3.zero)
                    {
                        beam.SetPosition(0, __instance.transform.position);
                    }
                    else
                    {
                        beam.SetPosition(0, __instance.hitPoint);
                    }
                    beam.SetPosition(1, foundDodgeballs[closestIndex].transform.position);
                    foundDodgeballs[closestIndex].ExciteBall();
                    return;
                }

            }

            ThrownEgg[] foundEggs = GameObject.FindObjectsOfType<ThrownEgg>();
            if (foundEggs.Length > 0)
            {
                int closestIndex = -1;
                float closestDistance = Mathf.Infinity;
                for (int i = 0; i < foundEggs.Length; i++)
                {
                    Vector3 directionToTarget = foundEggs[i].transform.position - __instance.transform.position;
                    if (Physics.Raycast(__instance.transform.position, directionToTarget, out RaycastHit rayHit, directionToTarget.magnitude, LayerMask.GetMask("Limb", "BigCorpse", "Outdoors", "Environment", "Default")))
                    {
                        if (rayHit.collider.GetComponent<ThrownEgg>() == null && rayHit.collider.GetComponentInParent<ThrownEgg>() == null)
                        {
                            foundEggs[i] = null;
                        }
                        else if (directionToTarget.sqrMagnitude < closestDistance)
                        {
                            closestIndex = i;
                        }
                    }
                }

                if (closestIndex != -1)
                {
                    LineRenderer beam = GameObject.Instantiate<GameObject>(__instance.refBeam, __instance.transform.position, Quaternion.identity).GetComponent<LineRenderer>();
                    beam.gameObject.GetComponent<RevolverBeam>().sourceWeapon = __instance.sourceWeapon;


                    if (__instance.hitPoint == Vector3.zero)
                    {
                        beam.SetPosition(0, __instance.transform.position);
                    }
                    else
                    {
                        beam.SetPosition(0, __instance.hitPoint);
                    }
                    beam.SetPosition(1, foundEggs[closestIndex].transform.position);
                    foundEggs[closestIndex].Explode();
                    return;
                }

            }
        }

    }
}
