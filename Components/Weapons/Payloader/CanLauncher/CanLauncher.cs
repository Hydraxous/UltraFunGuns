using System.Collections.Generic;
using UnityEngine;

namespace UltraFunGuns
{
    [UFGWeapon("CanLauncher", "CANnon", 3, true, WeaponIconColor.Blue)]
    [WeaponAbility("Can Cannon", "Press <color=orange>Fire 1</color> to fire a can.", 0, RichTextColors.aqua)]
    [WeaponAbility("Boosted Output", "Parry a can after firing to accelerate it's velocity.", 0, RichTextColors.yellow)]
    [WeaponAbility("Kick It", "Shoot a can with a low power weapon to <color=orange>Kick it</color>.", 1, RichTextColors.yellow)]
    [WeaponAbility("Fragment", "After sufficient damage, a Can can be <color=orange>fragmented</color> with a swift punch.", 2, RichTextColors.yellow)]
    [WeaponAbility("Super Fragment", "Shoot a can with a high power weapon to <color=orange>fragment</color> it.", 3, RichTextColors.red)]
    public class CanLauncher : UltraFunGunBase
    {
        [UFGAsset("CanLauncher_CanProjectile")] public static GameObject CanProjectile { get; private set; }

        public float shootForce = 80.0f;
        public float targetBeamThickness = 0.5f;
        public float maxBeamDistance = 500.0f;
        public Vector3 shootVector;

        private Material[] canMaterials;
        private int canMaterialCount = 10;
        private MeshRenderer canPrefabMeshRenderer;

        private ActionCooldown fireCanCooldown = new ActionCooldown(0.75f, true);

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

            canPrefabMeshRenderer = CanProjectile.GetComponentInChildren<MeshRenderer>();
        }

        private void Start()
        {

        }

        public override void GetInput()
        {
            if (MonoSingleton<InputManager>.Instance.InputSource.Fire1.WasPerformedThisFrame && fireCanCooldown.CanFire() && !om.paused)
            {
                fireCanCooldown.AddCooldown();
                Shoot();
            }
        }

        private void Shoot(Ray direction)
        {
            Vector3 target = Vector3.zero;

            Ray aimRay = (weaponIdentifier.duplicate) ? new Ray(firePoint.position, mainCam.forward) : HydraUtils.GetProjectileAimVector(mainCam, firePoint, targetBeamThickness, maxBeamDistance);


            Vector3 targetVelocity = aimRay.direction * shootForce;
            CameraController.Instance.CameraShake(0.15f);
            GameObject latestCan = GameObject.Instantiate<GameObject>(CanProjectile, firePoint.position, Quaternion.identity);
            GameObject newMuzzleFX = GameObject.Instantiate<GameObject>(Prefabs.CanLauncher_MuzzleFX, firePoint.position, Quaternion.identity);
            newMuzzleFX.AddComponent<DestroyOnDisable>();
            newMuzzleFX.transform.forward = firePoint.forward;
            latestCan.transform.forward = targetVelocity.normalized;
            newMuzzleFX.transform.parent = firePoint.parent;
            latestCan.GetComponent<CanProjectile>().AlterVelocity(targetVelocity, false);
            animator.Play("CanLauncher_Fire",0,0);
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
