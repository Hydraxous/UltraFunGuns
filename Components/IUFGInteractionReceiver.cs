using BepInEx;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    public interface IUFGInteractionReceiver
    {
        bool Interact(UFGInteractionEventData interaction);
        bool Targetable(TargetQuery query);
        Vector3 GetPosition();
    }

    public struct UFGInteractionEventData
    {
        public Vector3 interactorPosition;
        public Vector3 direction;

        public float power;
        public string data;
        public Type invokeType;
        public string[] tags;

        public bool ContainsTag(string tag)
        {
            if (tags == null)
                return false;

            for(int i=0; i<tags.Length; i++)
            {
                if (tags[i].IsNullOrWhiteSpace())
                    continue;

                if (tags[i].Contains(tag))
                    return true;
            }

            return false;
        }

        public bool ContainsAnyTag(params string[] checkTags)
        {
            for (int i=0; i< checkTags.Length; i++)
            {
                if (ContainsTag(checkTags[i]))
                    return true;
            }

            return false;
        }

        public bool ContainsAllTags(params string[] checkTags)
        {
            for(int i=0; i< checkTags.Length; i++)
            {
                if(!ContainsTag(checkTags[i]))
                    return false;
            }

            return true;
        }

    }

    public struct TargetQuery
    {
        public Vector3 queryOrigin, queryDirection;
        public bool checkLineOfSight;
        public float maxRange = -1;
        public float lineOfSightErrorMargin = 1.5f;

        public Collider lineOfSightCollider;

        public TargetQuery() { }
            

        public bool HasLineOfSight(Vector3 targetablePosition)
        {
            if (!checkLineOfSight)
                return true;

            float raycastRange = (maxRange >= 0) ? maxRange : Mathf.Infinity;


            Vector3 expectedPoint = queryOrigin;

            Ray ray = new Ray(targetablePosition, queryOrigin - targetablePosition);

            if(lineOfSightCollider != null)
            {
                if(lineOfSightCollider.Raycast(ray, out RaycastHit hit, raycastRange))
                {
                    expectedPoint = hit.point;
                }
            }

            if(Physics.Raycast(ray, out RaycastHit hitInfo, raycastRange, LayerMaskDefaults.Get(LMD.Environment)))
            {
                if(Vector3.Distance(hitInfo.point, expectedPoint) < lineOfSightErrorMargin)
                    return true;
            }else
            {
                //We hit nothing and therefore nothing is in the way.
                return true;
            }

            //Hit something and it wasnt what we wanted.
            return false;
        }

        public bool InRange(Vector3 targetablePosition)
        {
            return Vector3.Distance(targetablePosition, queryOrigin) < ((maxRange >= 0) ? maxRange : Mathf.Infinity);
        }

        public bool CheckTargetable(Vector3 targetablePosition)
        {
            return (InRange(targetablePosition) && HasLineOfSight(targetablePosition));
        }
    }
}
