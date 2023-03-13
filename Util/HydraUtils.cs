using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.Reflection;
using UnityEngine.InputSystem.HID;
using MonoMod.RuntimeDetour.HookGen;
using System.Runtime.CompilerServices;
using System.Linq;

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

        public static bool TryGetHomingTarget(Vector3 sampleLocation, out Transform homingTarget, out EnemyIdentifier enemyComponent)
        {
            homingTarget = null;
            enemyComponent = null;

            List<EnemyIdentifier> possibleTargets = new List<EnemyIdentifier>();
            List<Transform> targetPoints = new List<Transform>();
            GameObject[] enemyObjectsActive = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (GameObject enemyObject in enemyObjectsActive)
            {
                if (enemyObject.TryGetComponent<EnemyIdentifier>(out EnemyIdentifier enemyFound))
                {
                    if (!enemyFound.dead && !possibleTargets.Contains(enemyFound))
                    {
                        possibleTargets.Add(enemyFound);
                        Transform enemyTargetPoint;
                        if (enemyFound.weakPoint != null && enemyFound.weakPoint.activeInHierarchy)
                        {
                            enemyTargetPoint = enemyFound.weakPoint.transform;
                        }
                        else
                        {
                            EnemyIdentifierIdentifier enemyFoundIdentifier = enemyFound.GetComponentInChildren<EnemyIdentifierIdentifier>();
                            if (enemyFoundIdentifier)
                            {
                                enemyTargetPoint = enemyFoundIdentifier.transform;
                            }
                            else
                            {
                                enemyTargetPoint = enemyFound.transform;
                            }
                        }

                        Vector3 directionToEnemy = enemyTargetPoint.position - sampleLocation;
                        if (Physics.Raycast(sampleLocation, directionToEnemy, out RaycastHit rayHit, directionToEnemy.magnitude, LayerMask.GetMask("Limb", "BigCorpse", "Outdoors", "Environment", "Default")))
                        {
                            if (rayHit.collider.gameObject.TryGetComponent<EnemyIdentifier>(out EnemyIdentifier losEnemyID) || rayHit.collider.gameObject.TryGetComponent<EnemyIdentifierIdentifier>(out EnemyIdentifierIdentifier losEnemyIDID))
                            {
                                targetPoints.Add(enemyTargetPoint);
                            }
                        }

                    }
                }
            }

            int closestIndex = -1;
            float closestDistance = Mathf.Infinity;
            for (int i = 0; i < targetPoints.Count; i++)
            {
                Vector3 distance = targetPoints[i].position - sampleLocation;
                if (distance.sqrMagnitude < closestDistance)
                {
                    closestIndex = i;
                    closestDistance = distance.sqrMagnitude;
                }
            }

            if (closestIndex > -1)
            {
                homingTarget = targetPoints[closestIndex];
                enemyComponent = homingTarget.GetComponent<EnemyIdentifier>();
                return true;
            }
            else
            {
                return false;
            }

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

        /// <summary>
        /// Get a velocity vector for a projectile parabola with a specified flight time.
        /// </summary>
        /// <param name="start">Start position</param>
        /// <param name="end">End Position</param>
        /// <param name="airTime">Total flight time</param>
        /// <returns></returns>
        public static Vector3 GetVelocityTrajectory(Vector3 start, Vector3 end, float airTime)
        {
            Vector3 gravity = Physics.gravity;

            airTime = (airTime == 0) ? airTime + 0.0001f : airTime;

            Vector3 parabolaMiddleHeight = Vector3.LerpUnclamped(start, end, 0.5f / airTime);
            parabolaMiddleHeight -= gravity * airTime;

            Vector3 shootDirection = parabolaMiddleHeight - start;

            return shootDirection;
        }

        public static Vector3 CalculateProjectileArcPosition(Vector3 start, Vector3 end, float airTime, float normalizedTime)
        {
            Vector3 initialDirection = GetVelocityTrajectory(start, end, airTime);

            Vector3[] trajectoryPoints = GetTrajectoryPoints(start, Physics.gravity, initialDirection, 0.0f, airTime);

            int index = Mathf.FloorToInt(trajectoryPoints.Length-1 * normalizedTime);

            return trajectoryPoints[index];
        }

        //TODO optimize this. This does not do what it was ported for. Purpose is for getting a point along a trajectory at a given time. This function is for drawing said trajectory, not querying it.
        private static Vector3[] GetTrajectoryPoints(Vector3 start, Vector3 gravity, Vector3 direction, float drag, float flightTime)
        {

            Vector3 currentVelocity = direction;

            float positionStep = flightTime / 100;

            Vector3 lastPosition = start;

            List<Vector3> points = new List<Vector3>();

            for (int i = 0; i < 100 * 2; i++)
            {
                currentVelocity = currentVelocity * (1 - positionStep * drag);

                currentVelocity += gravity * positionStep;

                points.Add(lastPosition);

                lastPosition += currentVelocity * positionStep;
            }

            return points.ToArray();
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

        public static bool IsColliderEnemy(this Collider collider, out EnemyIdentifier enemy, bool filterDead = true)
        {
            enemy = null;
            if (collider.gameObject.TryGetComponent<EnemyIdentifierIdentifier>(out EnemyIdentifierIdentifier enemyIDID))
            {
                if(enemyIDID.eid != null)
                {
                    enemy = enemyIDID.eid;
                    if(!enemy.dead || !filterDead)
                    {
                        return true;
                    }
                }
            }
            else if (collider.gameObject.TryGetComponent<EnemyIdentifier>(out EnemyIdentifier enemyID))
            {
                enemy = enemyID;
                if (!enemy.dead || !filterDead)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsCollisionEnemy(this Collision collider, out EnemyIdentifier enemy, bool filterDead = true)
        {
            enemy = null;
            if (collider.gameObject.TryGetComponent<EnemyIdentifierIdentifier>(out EnemyIdentifierIdentifier enemyIDID))
            {
                if (enemyIDID.eid != null)
                {
                    enemy = enemyIDID.eid;
                    if (!enemy.dead || !filterDead)
                    {
                        return true;
                    }
                }
            }
            else if (collider.gameObject.TryGetComponent<EnemyIdentifier>(out EnemyIdentifier enemyID))
            {
                enemy = enemyID;
                if (!enemy.dead || !filterDead)
                {
                    return true;
                }
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

        public static bool ConeCheck(Vector3 direction1, Vector3 direction2, float maximumAngle = 90.0f)
        {
            return Vector3.Angle(direction1, direction2) <= maximumAngle;
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

        public static void PlayAudioClip(this AudioClip clip, float pitch = 1.0f, float volume = 1.0f, float spatialBlend = 0.0f)
        {
            PlayAudioClip(clip, Vector3.zero, pitch, volume, spatialBlend);
        }

        public static void PlayAudioClip(this AudioClip clip, Vector3 position, float pitch = 1.0f, float volume = 1.0f, float spatialBlend = 0.0f)
        {
            if (clip == null)
            {
                return;
            }

            GameObject newAudioObject = new GameObject($"AudioSource({clip.name})");
            newAudioObject.transform.position = position;
            AudioSource newAudioSource = newAudioObject.AddComponent<AudioSource>();
            DestroyAfterTime destroyOverTime = newAudioObject.AddComponent<DestroyAfterTime>();
            newAudioSource.playOnAwake = false;
            newAudioSource.spatialBlend = spatialBlend;
            newAudioSource.volume = volume;
            newAudioSource.pitch = pitch;
            newAudioSource.clip = clip;
            newAudioSource.Play();
        }


        public static Ray ToRay(this Transform transform)
        {
            return new Ray(transform.position, transform.forward);
        }

        public static TargetObject GetTargetFromGameObject(GameObject targetGameObject)
        {
            TargetObject newTargetObject = new TargetObject(targetGameObject);
            return newTargetObject;
        }

        public static List<TargetObject> GetTargetsFromGameObjects(GameObject[] targetGameObjects)
        {
            List<TargetObject> newTargets = new List<TargetObject>();
            for (int i = 0; i < targetGameObjects.Length; i++)
            {
                TargetObject newTarget = new TargetObject(targetGameObjects[i]);
                if (newTarget.validTarget)
                {
                    newTargets.Add(new TargetObject(targetGameObjects[i]));
                }
            }
            return newTargets;
        }

        public static bool CanBeDamaged (this EnemyIdentifier eid)
        {
            if(eid == null)
            {
                return false;
            }

            if(eid.dead)
            {
                return false;
            }

            if(eid.health <= 0.0f)
            {
                return false;
            }

            if(!eid.enabled)
            {
                return false;
            }

            return true;
        }


        //TODO optimize this. this camera only
        public static bool TryGetTarget(out Vector3 targetDirection) //Return true if enemy target found.
        {
            List<EnemyIdentifier> possibleTargets = new List<EnemyIdentifier>();
            List<Transform> targetPoints = new List<Transform>();
            GameObject[] enemyObjectsActive = EnemyTracker.Instance.GetCurrentEnemies().Select(e => e.gameObject).ToArray();
            Transform camera = CameraController.Instance.transform;

            foreach (GameObject enemyObject in enemyObjectsActive)
            {
                if (enemyObject.TryGetComponent<EnemyIdentifier>(out EnemyIdentifier enemyFound))
                {
                    if (!enemyFound.dead && !possibleTargets.Contains(enemyFound))
                    {
                        possibleTargets.Add(enemyFound);
                        Transform enemyTargetPoint;
                        if (enemyFound.weakPoint != null && enemyFound.weakPoint.activeInHierarchy)
                        {
                            enemyTargetPoint = enemyFound.weakPoint.transform;
                        }
                        else
                        {
                            EnemyIdentifierIdentifier enemyFoundIdentifier = enemyFound.GetComponentInChildren<EnemyIdentifierIdentifier>();
                            if (enemyFoundIdentifier)
                            {
                                enemyTargetPoint = enemyFoundIdentifier.transform;
                            }
                            else
                            {
                                enemyTargetPoint = enemyFound.transform;
                            }
                        }

                       

                        //Cone check
                        Vector3 directionToEnemy = (enemyTargetPoint.position - camera.position).normalized;
                        Vector3 lookDirection = camera.forward;
                        if (SphereCastMacro(camera.position,0.05f,directionToEnemy,Mathf.Infinity, out RaycastHit hit))
                        {
                            switch (hit.collider.gameObject.layer)
                            {
                                case 10:
                                case 11:
                                    targetPoints.Add(enemyTargetPoint);
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
            }

            //Get closest target
            int closestIndex = -1;
            float closestDistance = Mathf.Infinity;
            for (int i = 0; i < targetPoints.Count; i++)
            {
                Vector3 distance = targetPoints[i].position - camera.position;
                if (distance.sqrMagnitude < closestDistance)
                {
                    closestIndex = i;
                    closestDistance = distance.sqrMagnitude;
                }
            }

            if (closestIndex > -1)
            {
                targetDirection = targetPoints[closestIndex].position - camera.position;
                return true;
            }
            else //No targets found.
            {
                targetDirection = Vector3.zero;
                return false;
            }
        }

        /// <summary>
        /// Returns local quaternion rotation based off of world space quaternion
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="worldRotation"></param>
        /// <returns></returns>
        public static Quaternion WorldToLocalRotation(this Transform transform, Quaternion worldRotation)
        {
            return Quaternion.Inverse(transform.rotation) * worldRotation;
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

        /// <summary>
        /// Returns the component if it is found on the gameobject. If it isn't found, it will add the component.
        /// </summary>
        /// <returns>Always returns component</returns>
        public static T EnsureComponent<T>(this GameObject gameObject)
        {
            if(gameObject.TryGetComponent<T>(out T component))
            {
                return component;
            }else
            {
                return (T)(object)gameObject.AddComponent(typeof(T));
            }
        }

        /// <summary>
        /// Returns the component if it is found on the transform. If it isn't found, it will add the component.
        /// </summary>
        /// <returns>Always returns component</returns>
        public static T EnsureComponent<T>(this Transform transform)
        {
            if (transform.TryGetComponent<T>(out T component))
            {
                return component;
            }
            else
            {
                return (T)(object)transform.gameObject.AddComponent(typeof(T));
            }
        }
    }

    public class TargetObject
    {
        public enum TargetType { Zombie, Spider, Machine, Statue, Wicked, Drone, Idol, Dodgeball, GenericRigidbody, Egg, Pylon }
        public TargetType targetType;
        public GameObject gameObject;
        public Transform weakPoint;
        public Transform targetPoint;
        public Rigidbody rigidbody;
        public bool validTarget = true;

        public TargetObject(GameObject gameObject)
        {
            this.gameObject = gameObject;

            if (!this.gameObject.activeInHierarchy)
            {
                this.validTarget = false;
                return;
            }

            //Generic rigibody check
            if (this.gameObject.TryGetComponent<Rigidbody>(out Rigidbody genericRigidbody))
            {
                this.targetPoint = genericRigidbody.transform;
                this.targetType = TargetType.GenericRigidbody;
            }


            if (this.gameObject.TryGetComponent<EnemyIdentifier>(out EnemyIdentifier enemyFound))
            {
                if (!enemyFound.dead)
                {
                    if (enemyFound.zombie != null)
                    {
                        targetType = TargetType.Zombie;
                    }
                    else if (enemyFound.spider != null)
                    {
                        targetType = TargetType.Spider;
                    }
                    else if (enemyFound.machine != null)
                    {
                        targetType = TargetType.Machine;
                    }
                    else if (enemyFound.statue != null)
                    {
                        targetType = TargetType.Statue;
                    }
                    else if (enemyFound.wicked != null)
                    {
                        targetType = TargetType.Wicked;
                    }
                    else if (enemyFound.drone != null)
                    {
                        targetType = TargetType.Drone;
                    }
                    else if (enemyFound.idol != null)
                    {
                        targetType = TargetType.Idol;
                    }

                    if (enemyFound.weakPoint != null && enemyFound.weakPoint.activeInHierarchy)
                    {
                        this.weakPoint = enemyFound.weakPoint.transform;
                    }

                    EnemyIdentifierIdentifier enemyFoundIdentifier = enemyFound.GetComponentInChildren<EnemyIdentifierIdentifier>();
                    if (enemyFoundIdentifier)
                    {
                        this.targetPoint = enemyFoundIdentifier.transform;
                    }
                    else
                    {
                        this.targetPoint = enemyFound.transform;
                    }
                    return;
                }
            }




            //Ending validity check
            if (this.targetType != TargetType.GenericRigidbody)
            {
                this.validTarget = false;
                return;
            }
        }
    }

    //TODO Placeholder
    public class AltTarget
    {
        public enum TargetType { Zombie, Spider, Machine, Statue, Wicked, Drone, Idol, Dodgeball, GenericRigidbody, Egg, Pylon }
        public TargetType targetType;
        public GameObject gameObject;
        public Transform weakPoint;
        public Transform targetPoint;
        public Rigidbody rigidbody;
        public bool validTarget = true;
        private void NULL()
        {
            if (this.gameObject.TryGetComponent<ThrownEgg>(out ThrownEgg egg))
            {
                this.targetPoint = egg.transform;
                this.targetType = TargetType.Egg;
                return;
            }
            else if (this.gameObject.GetComponentInParent<ThrownEgg>() != null)
            {
                this.targetPoint = this.gameObject.GetComponentInParent<ThrownEgg>().transform;
                this.targetType = TargetType.Egg;
                return;
            }


            if (this.gameObject.TryGetComponent<ThrownDodgeball>(out ThrownDodgeball thrownDodgeball))
            {
                this.targetPoint = egg.transform;
                this.targetType = TargetType.Dodgeball;
                return;
            }
            else if (this.gameObject.GetComponentInParent<ThrownDodgeball>() != null)
            {
                this.targetPoint = this.gameObject.GetComponentInParent<ThrownDodgeball>().transform;
                this.targetType = TargetType.Dodgeball;
                return;
            }


            if (this.gameObject.TryGetComponent<FocalyzerPylon>(out FocalyzerPylon pylon))
            {
                this.targetPoint = pylon.transform;
                this.targetType = TargetType.Pylon;
                return;
            }
            else if (this.gameObject.GetComponentInParent<FocalyzerPylon>() != null)
            {
                this.targetPoint = this.gameObject.GetComponentInParent<FocalyzerPylon>().transform;
                this.targetType = TargetType.Pylon;
                return;
            }


            if (this.gameObject.TryGetComponent<FocalyzerPylonAlternate>(out FocalyzerPylonAlternate pylonAlt))
            {
                this.targetPoint = pylonAlt.transform;
                this.targetType = TargetType.Pylon;
                return;
            }
            else if (this.gameObject.GetComponentInParent<FocalyzerPylonAlternate>() != null)
            {
                this.targetPoint = this.gameObject.GetComponentInParent<FocalyzerPylonAlternate>().transform;
                this.targetType = TargetType.Pylon;
                return;
            }
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
}
