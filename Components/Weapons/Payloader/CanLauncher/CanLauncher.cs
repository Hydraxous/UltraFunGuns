using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace UltraFunGuns
{
    public class CanLauncher : UltraFunGunBase
    {
        public GameObject canPrefab;
        public GameObject muzzleFX;

        public float shootForce = 60.0f;
        public float targetBeamThickness = 0.5f;
        public float maxBeamDistance = 500.0f;
        public Vector3 shootVector;

        public Text debugText;
        private Material[] canMaterials;
        private int canMaterialCount = 6;
        private MeshRenderer canPrefabMeshRenderer;
        private MeshRenderer fakeCanMeshRenderer;

        public override void OnAwakeFinished()
        {
            Debug.Log("Starting material grab");
            List<Material> canTextureList = new List<Material>();
            for(int i =0;i<canMaterialCount;i++)
            {
                Material newCanMaterial;
                HydraLoader.dataRegistry.TryGetValue(string.Format("CanLauncher_CanProjectile_Material_{0}", i), out UnityEngine.Object obj);
                newCanMaterial = (Material) obj;
                canTextureList.Add(newCanMaterial);
            }
            canMaterials = canTextureList.ToArray();

            HydraLoader.prefabRegistry.TryGetValue("CanLauncher_CanProjectile", out canPrefab);

            canPrefabMeshRenderer = canPrefab.GetComponentInChildren<MeshRenderer>();

            //TODO FX HydraLoader.prefabRegistry.TryGetValue("TricksniperMuzzleFX", out muzzleFX);
            //debugText = transform.Find("DebugCanvas/DebugPanel/DebugText").GetComponent<Text>();
        }

        public override Dictionary<string, ActionCooldown> SetActionCooldowns()
        {
            Dictionary<string, ActionCooldown> cooldowns = new Dictionary<string, ActionCooldown>();
            cooldowns.Add("primaryFire", new ActionCooldown(0.75f));
            //cooldowns.Add("explodeDelay", new ActionCooldown(0.2f));
            return cooldowns;
        }

        private void Start()
        {

        }

        public override void DoAnimations()
        {

        }

        public override void GetInput()
        {
            if (MonoSingleton<InputManager>.Instance.InputSource.Fire1.WasPerformedThisFrame && actionCooldowns["primaryFire"].CanFire() && !om.paused)
            {
                actionCooldowns["primaryFire"].AddCooldown();
                Shoot();
            }

            //debugText.text = String.Format("{0} ROT\n{1} TURN", revolutions, turnsCompleted);
        }

        private void Shoot(Ray direction)
        {
            Vector3 target = Vector3.zero;

            RaycastHit[] hits = Physics.SphereCastAll(direction, targetBeamThickness, maxBeamDistance, LayerMask.GetMask("Limb", "BigCorpse", "Outdoors", "Environment", "Default", "Projectile"));
            hits = HydraUtils.SortRaycastHitsByDistance(hits);
            if (hits.Length > 0)
            {
                if (!(hits.Length == 1 && hits[0].collider.gameObject.name == "CameraCollisionChecker"))
                {
                    hits = HydraUtils.SortRaycastHitsByDistance(hits);
                    for (int i = 0; i < hits.Length; i++)
                    {
                        if (!(hits[i].collider.gameObject.name == "CameraCollisionChecker"))
                        {
                            target = hits[i].point;
                            Debug.Log("we hituhhhhh " + hits[i].collider.gameObject.name);
                            break;
                        }
                    }
                }
                else//todo clean this.
                {
                    Ray missray = new Ray();
                    missray.origin = mainCam.position;
                    missray.direction = mainCam.TransformDirection(0, 0, 1);

                    target = missray.GetPoint(maxBeamDistance);
                }
            }
            else
            {
                    Ray missray = new Ray();
                    missray.origin = mainCam.transform.position;
                    missray.direction = mainCam.transform.TransformDirection(0, 0, 1);

                    target = missray.GetPoint(maxBeamDistance);
            }


            Vector3 targetVelocity = (target - firePoint.position).normalized * shootForce;
            CameraController.Instance.CameraShake(0.15f);
            GameObject latestCan = GameObject.Instantiate<GameObject>(canPrefab, firePoint.position, Quaternion.identity);
            //TODO instantiate muzzle fx
            latestCan.transform.forward = targetVelocity.normalized;
            latestCan.GetComponent<CanProjectile>().AlterVelocity(targetVelocity, false);
                
        }

        private void Shoot()
        {
            Vector3 origin = mainCam.position;
            Vector3 direction = mainCam.TransformDirection(new Vector3(0, 0, 1));

            int randomCanMaterial = UnityEngine.Random.Range(0, canMaterials.Length);
            canPrefabMeshRenderer.material = canMaterials[randomCanMaterial];

            Shoot(new Ray(origin, direction));

        }

    }
}
