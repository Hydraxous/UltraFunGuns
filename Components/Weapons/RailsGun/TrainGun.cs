using System.Collections.Generic;
using UltraFunGuns.Components;
using UnityEngine;

namespace UltraFunGuns
{
    [UFGWeapon("TrainGun", "Conductor", 2, true, WeaponIconColor.Red, false)]
    public class TrainGun : UltraFunGunBase
    {
        [UFGAsset("TrainGun_Train")] private static GameObject TrainPrefab;
        [UFGAsset("TrainGunWarningLoop")] private static AudioClip trainWarningClip;

        [Configgy.Configgable("Weapons/Conductor")]
        private static float arrivalDelay = 2.5f;

        [Configgy.Configgable("Weapons/Conductor")] 
        private static float cooldown = 5f;

        private List<TrainGunTrain> instancedTrains = new List<TrainGunTrain>();

        private bool pickingDirection;
        private Vector3 targetPoint;
        private Vector3 endPoint;
        private Vector3 observedDirection;

        private float currentCooldown = 0f;
        private float timeDisabled;

        GameObject primitiveSphereTargeter;
        GameObject primitiveSphereTargeter2;

        private void Start()
        {
            primitiveSphereTargeter = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            if (primitiveSphereTargeter.TryGetComponent<Collider>(out Collider col))
                col.enabled = false;

            primitiveSphereTargeter.SetActive(false);

            primitiveSphereTargeter2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            if (primitiveSphereTargeter2.TryGetComponent<Collider>(out Collider col2))
                col2.enabled = false;

            primitiveSphereTargeter2.SetActive(false);
        }

        private void Update()
        {
            InputUpdate();
            currentCooldown -= Time.deltaTime;

            primitiveSphereTargeter?.SetActive(pickingDirection);
            if (pickingDirection && primitiveSphereTargeter != null)
            {
                primitiveSphereTargeter.transform.position = endPoint;
            }

            primitiveSphereTargeter2?.SetActive(pickingDirection);
            if (pickingDirection && primitiveSphereTargeter2 != null)
            {
                primitiveSphereTargeter2.transform.position = targetPoint;
            }
        }

        private void InputUpdate()
        {
            if (om.paused)
                return;
            
            if (!CanFire())
                return;

            if (InputManager.Instance.InputSource.Fire1.IsPressed)
            {
                if (!pickingDirection)
                {
                    targetPoint = GetLookPoint();
                    pickingDirection = true;
                }
                else
                {
                    Vector3 aimpoint = GetLookPoint();
                    aimpoint.y = targetPoint.y;
                    observedDirection = aimpoint;
                    endPoint = aimpoint;
                }
            }
            else
            {
                if(pickingDirection)
                {
                    pickingDirection = false;

                    Vector3 aimpoint = GetLookPoint();
                    aimpoint.y = targetPoint.y;
                    observedDirection = aimpoint;
                    endPoint = aimpoint;

                    FireTrain();
                    return;
                }
            }


            if (InputManager.Instance.InputSource.Fire2.WasPerformedThisFrame)
            {
                if (pickingDirection)
                {
                    pickingDirection = false;
                    return;
                }

                FireTrain();
            }
        }

        private Vector3 GetLookPoint()
        {
            if (!Physics.Raycast(mainCam.position, mainCam.forward, out RaycastHit hit, Mathf.Infinity, LayerMaskDefaults.Get(LMD.Environment)))
            {
                return player.transform.position+(UnityEngine.Random.onUnitSphere*0.02f);
            }

            //Check if we he a wall or something
            if (Vector3.Dot(Vector3.up, hit.normal) <= 0.25f)
            {
                if (!Physics.Raycast(hit.point, Vector3.down, out RaycastHit floorHit, Mathf.Infinity, LayerMaskDefaults.Get(LMD.Environment)))
                    return hit.point;

                return floorHit.point;
            }

            return hit.point;
        }

        private bool CanFire()
        {
            if (ULTRAKILL.Cheats.NoWeaponCooldown.NoCooldown)
                return true;

            return currentCooldown <= 0f;
        }

        private void FireTrain()
        {
            float moveSpeed = TrainGunTrain.moveSpeed;

            Vector3 direction = endPoint - targetPoint;
            Vector3 goalPosition = Vector3.Lerp(targetPoint, endPoint, 0.5f);
            Vector3 spawnPosition = goalPosition - (moveSpeed * arrivalDelay * direction.normalized);
            currentCooldown = cooldown;

            GameObject trainObject = GameObject.Instantiate(TrainPrefab, spawnPosition, Quaternion.LookRotation(direction.normalized));
            GameObject trainWarningSound = CreateTrainWarning(targetPoint+Vector3.up*4f, arrivalDelay);


            trainObject.AddComponent<BehaviourRelay>().OnOnDestroy += (g) =>
            {
                if (trainWarningSound != null)
                    GameObject.Destroy(trainWarningSound);
            };

            if(trainObject.TryGetComponent<TrainGunTrain>(out TrainGunTrain train))
            {
                train.SetOwner(gameObject);
                instancedTrains.Add(train);
            }
        }

        private GameObject CreateTrainWarning(Vector3 position, float delay)
        {
            GameObject trainWarning = new GameObject("TrainWarning");
            trainWarning.transform.position = position;

            trainWarning.AddComponent<Cleanable>();

            Light l = trainWarning.AddComponent<Light>();
            l.color = Color.red;
            l.range = 20f;

            AudioSource source = trainWarning.AddComponent<AudioSource>();
            source.clip = trainWarningClip; 

            source.loop = true;
            source.spatialBlend = 0.8f;

            source.Play();
            float timer = delay;

            trainWarning.AddComponent<BehaviourRelay>().OnUpdate = (g) =>
            {
                timer -= Time.deltaTime;
                float t = 1-(timer / delay);
                t = Mathf.Clamp01(t);
                l.intensity = Mathf.Lerp(0,5f,t);
                source.volume = 1-t;
                
            };

            return trainWarning;
        }

        private void OnEnable()
        {
            currentCooldown = Mathf.Min(currentCooldown, Time.time-timeDisabled);
        }

        private void OnDisable()
        {
            pickingDirection = false;
            timeDisabled = Time.time;
            primitiveSphereTargeter?.SetActive(false);
        }
    }
}
