using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace UltraFunGuns
{
    public static class HydraUtils
    {
        public static Ray GetProjectileAimVector(Transform view, Transform projectileOrigin, float thickness = 0.01f, float maxRange = 1000.0f)
        {
            Ray aimRay = new Ray();

            aimRay.origin = view.position;
            aimRay.direction = view.forward;

            Vector3 endPoint = aimRay.GetPoint(maxRange);

            aimRay.origin = projectileOrigin.position;
            aimRay.direction = endPoint - projectileOrigin.position;

            if(SphereCastMacro(view.position, thickness, view.forward, maxRange, out RaycastHit hit))
            {
                aimRay.origin = projectileOrigin.position;
                aimRay.direction = hit.point - projectileOrigin.position;
                HydraLogger.Log($"Raycast hit [{hit.collider.gameObject.name}]");
            }

            return aimRay;
        }

        //I hate you CameraCollisionChecker.
        public static bool SphereCastMacro(Vector3 position, float thickness, Vector3 direction, float maxRange, out RaycastHit hit, bool hitTriggers = false)
        {
            hit = new RaycastHit();

            if (SphereCastAllMacro(position, thickness, direction, maxRange, out RaycastHit[] hits, hitTriggers))
            {
                hit = hits[0];
                return true;
            }

            return false;
        }

        public static bool SphereCastAllMacro(Vector3 position, float thickness, Vector3 direction, float maxRange,  out RaycastHit[] hits, bool hitTriggers = false)
        {
            hits = Physics.SphereCastAll(position, thickness, direction, maxRange, LayerMask.GetMask("Projectile", "Limb", "BigCorpse", "Environment", "Outdoors", "Armor", "Default"));

            if (hits.Length <= 0)
                return false;

            List<RaycastHit> newHits = new List<RaycastHit>(SortRaycastHitsByDistance(hits));

            int counter = 0;
            while(counter < newHits.Count)
            {
                if (newHits[counter].collider.gameObject.name == "CameraCollisionChecker" || (newHits[counter].collider.isTrigger && !hitTriggers))
                {
                    newHits.RemoveAt(counter);
                }else
                {
                    counter++;
                }
            }

            hits = newHits.ToArray();

            return newHits.Count > 0;
        }

        public static string CollisionInfo(Collision col)
        {
            string str = "Name: {0}\n" + "Layer: {1}\n" + "Tag: {2}\n" + "ContactCount: {3}";
            str = string.Format(str, col.collider.name, col.gameObject.layer, col.collider.tag, col.contactCount);
            return str;
        }

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

        //LOS check only counts the level, environment, etc. as obstruction of view.
        public static bool LineOfSightCheck(Vector3 source, Vector3 target)
        {
            Vector3 rayCastDirection = target - source;
            RaycastHit[] hits = Physics.RaycastAll(source, rayCastDirection, rayCastDirection.magnitude);
            hits = HydraUtils.SortRaycastHitsByDistance(hits);
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.gameObject.layer == 24 || hit.collider.gameObject.layer == 25 || hit.collider.gameObject.layer == 8 || hit.collider.gameObject.layer == 0)
                {
                    if (hit.collider.gameObject.name != "CameraCollisionChecker")
                    {
                        return false;
                    }

                }
            }
            return true;
        }

        public static bool IsColliderEnvironment(this Collider col)
        {
            switch(col.gameObject.layer)
            {
                case 24: case 25: case 8:
                    return true;
            }

            return false;
        }

        public static Vector3[] DoRayHit(Ray hitRay, float range, bool penetration = false, float enemyDamage = 1.0f, bool explodeEnemyLimb = false, float critDamageMultiplier = 0.0f, GameObject sourceObject = null, bool explodeGrenade = false, bool explodeEgg = false, bool breakGlass = false, bool breakBreakable = false, bool exciteDodgeball = false)
        {
            RaycastHit[] hits = Physics.RaycastAll(hitRay, range, LayerMask.GetMask("Limb", "BigCorpse", "Outdoors", "Environment", "Default"));
            if (hits.Length > 0)
            {
                if (!(hits.Length == 1 && hits[0].collider.gameObject.name == "CameraCollisionChecker"))
                {
                    int endingHit = 0;
                    hits = HydraUtils.SortRaycastHitsByDistance(hits);
                    for (int i = 0; i < hits.Length; i++)
                    {
                        endingHit = i;

                        if ((hits[i].collider.gameObject.layer == 24 || hits[i].collider.gameObject.layer == 25 || hits[i].collider.gameObject.layer == 8))
                        {
                            break;
                        }

                        if (hits[i].collider.gameObject.TryGetComponent<EnemyIdentifierIdentifier>(out EnemyIdentifierIdentifier enemyIDID))
                        {
                            enemyIDID.eid.DeliverDamage(hits[i].collider.gameObject, hitRay.direction, hits[i].point, enemyDamage, explodeEnemyLimb, critDamageMultiplier, sourceObject);

                            if (!penetration)
                            {
                                break;
                            }
                        }
                        else if (hits[i].collider.gameObject.TryGetComponent<EnemyIdentifier>(out EnemyIdentifier enemyID))
                        {
                            enemyID.DeliverDamage(hits[i].collider.gameObject, hitRay.direction, hits[i].point, enemyDamage, explodeEnemyLimb, critDamageMultiplier, sourceObject);

                            if (!penetration)
                            {
                                break;
                            }
                        }

                        if (hits[i].collider.gameObject.TryGetComponent<Breakable>(out Breakable breakable) && breakBreakable)
                        {
                            breakable.Break();
                            if (!penetration)
                            {
                                break;
                            }
                        }

                        if (hits[i].collider.gameObject.TryGetComponent<ThrownEgg>(out ThrownEgg egg) && explodeEgg)
                        {
                            egg.Explode();
                            if (!penetration)
                            {
                                break;
                            }
                        }

                        if (hits[i].collider.gameObject.TryGetComponent<ThrownDodgeball>(out ThrownDodgeball dodgeBall) && exciteDodgeball)
                        {
                            dodgeBall.ExciteBall(6);
                            if (!penetration)
                            {
                                break;
                            }
                        }

                        if (hits[i].collider.gameObject.TryGetComponent<Grenade>(out Grenade grenade) && explodeGrenade)
                        {
                            MonoSingleton<TimeController>.Instance.ParryFlash();
                            grenade.Explode();
                            if (!penetration)
                            {
                                break;
                            }
                        }
                    }
                    return new Vector3[] { hits[endingHit].point, hits[endingHit].normal };
                }
            }

            return new Vector3[] { Vector3.zero };

        }

        public static T[] GetAllAttributesOfType<T>() where T : Attribute
        {
            List<T> attributeList = new List<T>();

            Assembly assembly = Assembly.GetExecutingAssembly();

            foreach (Type type in assembly.GetTypes())
            {
                var attributes = type.GetCustomAttributes<T>();
                if (attributes != null)
                {
                    foreach (T attribute in attributes)
                    {
                        if (attribute != null)
                        {
                            if(!attributeList.Contains(attribute))
                            {
                                attributeList.Add(attribute);
                            }
                        }
                    }
                }
            }

            return attributeList.ToArray();
        }


        public static Transform[] GetChildren(this Transform tf)
        {
            List<Transform> children = new List<Transform>();
            for (int i = 0; i < tf.childCount; i++)
            {
                children.Add(tf.GetChild(i));
            }
            return children.ToArray();
        }

        public static Ray ToRay(this Transform transform)
        {
            return new Ray(transform.position, transform.forward);
        }

        /// <summary>
        /// Sets the player rotation properly using a quaternion. Note: euler Z axis is ignored.
        /// Converts world space quaternion into player space rotations
        /// </summary>
        /// <param name="newRotation"></param>
        public static void SetPlayerRotation(Quaternion newRotation)
        {
            //Cnv
            Quaternion oldRot = CameraController.Instance.transform.rotation;
            CameraController.Instance.transform.rotation = newRotation;
            float sampleX = CameraController.Instance.transform.localEulerAngles.x;
            float newX = sampleX;

            //AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
            if (sampleX <= 90.0f && sampleX >=0)
            {
                newX = -sampleX;
            }else if(sampleX >= 270.0f && sampleX <= 360.0f)
            {
                newX = Mathf.Lerp(0.0f, 90.0f, Mathf.InverseLerp(360.0f,270.0f, sampleX));
            }

            float newY = CameraController.Instance.transform.rotation.eulerAngles.y;

            CameraController.Instance.rotationX = newX;
            CameraController.Instance.rotationY = newY;
        }

        public static void CreateBulletTrail(Vector3 startPosition, Vector3 endPosition, Vector3 normal)
        {
            if (Prefabs.BulletTrail == null)
            {
                return;
            }

            GameObject newBulletTrail = GameObject.Instantiate<GameObject>(Prefabs.BulletTrail, endPosition, Quaternion.identity);
            newBulletTrail.transform.up = normal;
            LineRenderer line = newBulletTrail.GetComponent<LineRenderer>();
            Vector3[] linePoints = new Vector3[2] { startPosition, endPosition };
            line.SetPositions(linePoints);
        }
    }

    public class Trajectory
    {
        public Vector3 Origin;
        public Vector3 End;
        public float AirTime;
        public Vector3 Gravity;

        public Trajectory(Vector3 origin, Vector3 end, float airTime, Vector3 gravity)
        {
            Origin = origin;
            End = end;
            AirTime = airTime;
            Gravity = gravity;
        }

        public Trajectory(Vector3 origin, Vector3 end, float airTime)
        {
            Origin = origin;
            End = end;
            AirTime = airTime;
            Gravity = Physics.gravity;
        }

        public Vector3 GetRequiredVelocity()
        {
            float flightTime = (AirTime == 0) ? AirTime + 0.0001f : AirTime;

            Vector3 parabolaMiddleHeight = Vector3.LerpUnclamped(Origin, End, 0.5f / flightTime);
            parabolaMiddleHeight -= Gravity * flightTime;

            Vector3 shootDirection = parabolaMiddleHeight - Origin;

            return shootDirection;
        }

        public Vector3[] GetPoints(int quality = 100, float drag = 0.0f)
        {
            if (quality == 0)
            {
                return new Vector3[] { Origin, End };
            }

            Vector3 currentVelocity = GetRequiredVelocity();

            float positionStep = AirTime / (quality/2);

            Vector3 lastPosition = Origin;

            List<Vector3> points = new List<Vector3>();

            //TODO I think this causes the speed up
            for (int i = 0; i < quality; i++)
            {
                currentVelocity = currentVelocity * (1 - positionStep * drag);

                currentVelocity += Gravity * positionStep;

                points.Add(lastPosition);

                lastPosition += currentVelocity * positionStep;

            }

            return points.ToArray();
        }

        public Vector3 GetPoint(float time, int quality = 100, float drag = 0.0f)
        {
            Vector3[] points = GetPoints(quality, drag);
            time = Mathf.Clamp01(time);
            return points[Mathf.FloorToInt((points.Length - 1) * time)];
        }

        

    }

    public class Ring
    {
        private int objectCount;

        private float radius;
        public float Radius
        {
            get
            {
                return CalcRadius();
            }

            set
            {
                this.radius = value;
                circumference = CalcCircumference();
            }
        }

        public float CalcRadius()
        {
            return circumference / (2 * Mathf.PI);
        }

        private float circumference;

        public float Circumference
        {
            get
            {
                return CalcCircumference();
            }

            set
            {
                circumference = value;
                radius = CalcRadius();
            }
        }

        public float Diameter
        {
            get
            {
                return radius * 2;
            }

            set
            {
                radius = (value / 2);
            }
        }

        public float CalcCircumference()
        {
            return 2 * Mathf.PI * radius;
        }


        public Ring(int objectCount, float radius)
        {
            this.objectCount = objectCount;
            this.radius = radius;
            Circumference = CalcCircumference();
        }
        
        public void SetCircumferenceFromObjectRadius(float objectRadius)
        {
            Circumference = objectRadius * objectCount;
        }

        public void SetCircumference(float circumference)
        {
            radius = circumference / (2 * Mathf.PI);
        }

        public float GetSliceSize()
        {
            return 2 * Mathf.PI / objectCount;
        }

        public Vector3[] GetPositions()
        {
            if(objectCount == 1)
            {
                return new Vector3[] { Vector3.zero };
            }

            List<Vector3> positions = new List<Vector3>();
            float slice = GetSliceSize();
            for (int i = 0; i < objectCount; i++)
            {
                float angle = slice * i;
                float newX = (Radius * Mathf.Cos(angle));
                float newZ = (Radius * Mathf.Sin(angle));

                positions.Add(new Vector3(newX,newZ,0.0f));
            }

            return positions.ToArray();
        }
    }

    
}
