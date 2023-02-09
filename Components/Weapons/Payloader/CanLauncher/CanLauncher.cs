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

        public float shootForce = 80.0f;
        public float targetBeamThickness = 0.5f;
        public float maxBeamDistance = 500.0f;
        public Vector3 shootVector;

        public Text debugText;
        private Material[] canMaterials;
        private int canMaterialCount = 10;
        private MeshRenderer canPrefabMeshRenderer;
        private MeshRenderer fakeCanMeshRenderer;

        public override void OnAwakeFinished()
        {
            List<Material> canTextureList = new List<Material>();
            for(int i=0;i<canMaterialCount;i++)
            {
                Material newCanMaterial;
                HydraLoader.dataRegistry.TryGetValue(string.Format("CanLauncher_CanProjectile_Material_{0}", i), out UnityEngine.Object obj);
                newCanMaterial = (Material) obj;
                canTextureList.Add(newCanMaterial);
            }
            canMaterials = canTextureList.ToArray();

            HydraLoader.prefabRegistry.TryGetValue("CanLauncher_CanProjectile", out canPrefab);
            HydraLoader.prefabRegistry.TryGetValue("CanLauncher_MuzzleFX", out muzzleFX);

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

            Ray aimRay = HydraUtils.GetProjectileAimVector(mainCam, firePoint, targetBeamThickness, maxBeamDistance);

            Vector3 targetVelocity = aimRay.direction * shootForce;
            CameraController.Instance.CameraShake(0.15f);
            GameObject latestCan = GameObject.Instantiate<GameObject>(canPrefab, firePoint.position, Quaternion.identity);
            GameObject newMuzzleFX = GameObject.Instantiate<GameObject>(muzzleFX, firePoint.position, Quaternion.identity);
            newMuzzleFX.transform.forward = firePoint.forward;
            latestCan.transform.forward = targetVelocity.normalized;
            newMuzzleFX.transform.parent = firePoint.parent;
            latestCan.GetComponent<CanProjectile>().AlterVelocity(targetVelocity, false);
            animator.Play("CanLauncher_Fire");
        }

        private void Shoot()
        {
            Vector3 origin = mainCam.position;
            Vector3 direction = mainCam.TransformDirection(new Vector3(0, 0, 1));

            int randomCanMaterial = UnityEngine.Random.Range(0, canMaterials.Length);
            canPrefabMeshRenderer.material = canMaterials[randomCanMaterial];

            Shoot(new Ray(origin, direction));

        }

        private void OnEnable()
        {
            animator.Play("CanLauncher_Equip");
        }

    }
}
