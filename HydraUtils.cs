using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;


namespace UltraFunGuns
{
    public static class HydraUtils
    {
        public static RaycastHit[] SortRaycastHitsByDistance(RaycastHit[] hits)
        {
            List<RaycastHit> sortedHits = new List<RaycastHit>(hits.Length);

            for (int i = 0; i < hits.Length; i++)
            {
                RaycastHit currentHit = hits[i];
                int currentIndex = i;

                while (currentIndex > 0 && sortedHits[currentIndex - 1].distance > currentHit.distance)
                {
                    currentIndex--;
                }

                sortedHits.Insert(currentIndex, currentHit);

            }

            return sortedHits.ToArray();
        }

        public static bool LineOfSightCheck(Vector3 source, Vector3 target)
        {
            Vector3 rayCastDirection = target - source;
            RaycastHit[] hits = Physics.RaycastAll(source, rayCastDirection, rayCastDirection.magnitude);
            hits = HydraUtils.SortRaycastHitsByDistance(hits);
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.gameObject.layer == 24 || hit.collider.gameObject.layer == 25 || hit.collider.gameObject.layer == 8 || hit.collider.gameObject.layer == 0)
                {
                    if(hit.collider.gameObject.name != "CameraCollisionChecker")
                    {
                        return false;
                    }
                    
                }
            }
            return true;
        }

    }
}
