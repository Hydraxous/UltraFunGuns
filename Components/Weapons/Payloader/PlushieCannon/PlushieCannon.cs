using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    [UFGWeapon("PlushieCannon", "dev_cannon", 3, true, WeaponIconColor.Yellow, false)]
    [WeaponAbility("DeliverDamage<color=white>();</color>", "Fire a plushie with <color=orange>Fire 1</color>", 0, RichTextColors.aqua)]
    [WeaponAbility("Fray<color=white>();</color>", "Firing upon a plushie will result in a frayed explosion.", 1, RichTextColors.red)]
    public class PlushieCannon : UltraFunGunBase
    {
        private GameObject[] plushies = new GameObject[]
        {
            Prefabs.HakitaPlush.Asset,
            Prefabs.MakoPlush.Asset,
            Prefabs.GianniPlush.Asset,
            Prefabs.DaliaPlush.Asset,
            Prefabs.PITRPlush.Asset,
            Prefabs.BigRockPlush.Asset,
            Prefabs.CameronPlush.Asset,
            Prefabs.VvizardPlush.Asset,
            Prefabs.DawgPlush.Asset,
            Prefabs.FrancisPlush.Asset,
            Prefabs.SaladPlush.Asset,
            Prefabs.HeckPlush.Asset,
            Prefabs.KGCPlush.Asset,
            Prefabs.MegaNekoPlush.Asset,
            Prefabs.LucasPlush.Asset,
            Prefabs.JoyPlush.Asset,
            Prefabs.JerichoPlush.Asset,
            Prefabs.MandyPlush.Asset,
            Prefabs.V1Plush.Asset,
            Prefabs.ScottPlush.Asset,
            Prefabs.CabalCrowPlush.Asset,
            Prefabs.QuetzalPlush.Asset,
            Prefabs.HealthBJPlush.Asset,
            Prefabs.HealthJakePlush.Asset,
            Prefabs.HealthJohnPlush.Asset,
            Prefabs.LenvalPlush.Asset,
            Prefabs.WeytePlush.Asset,
            Prefabs.HydraPlushie
        };

        private float targetBeamThickness = 0.5f, maxBeamDistance = 50.0f;

        [Configgable("UltraFunGuns/Weapons/dev_cannon")]
        private static float shootForce = 130.0f;

        [Configgable("UltraFunGuns/Weapons/dev_cannon")]
        private static float plushieSpinSpeed = 90.0f;

        [Configgable("UltraFunGuns/Weapons/dev_cannon")]
        private static float damage = 1.0f;

        //[UFGAsset("HydraPlushie")] public static GameObject HydraPlushie { get; private set; }

        private ActionCooldown fireCooldown = new ActionCooldown(0.6f, true);

        public override void GetInput()
        {
            if(InputManager.Instance.InputSource.Fire1.IsPressed && !om.paused && fireCooldown.CanFire())
            {
                fireCooldown.AddCooldown();
                Fire();
            }
        }

        private GameObject lastFired;

        private void Fire()
        {

            Ray aimRay = (weaponIdentifier.duplicate) ? new Ray(firePoint.position, mainCam.forward) : HydraUtils.GetProjectileAimVector(mainCam, firePoint, targetBeamThickness, maxBeamDistance);
            Vector3 targetVelocity = aimRay.direction * shootForce;

            lastFired = PickPlushie();

            GameObject firedPlushie = Instantiate(lastFired, firePoint.position+firePoint.forward*0.35f, Quaternion.identity);
            firedPlushie.transform.forward = targetVelocity.normalized;
            DeadlyPlushie plushie = firedPlushie.AddComponent<DeadlyPlushie>();

            plushie.sourceWeapon = gameObject;
            plushie.damage = damage;

            if(plushie.TryGetComponent<Rigidbody>(out Rigidbody rb))
            {
                rb.velocity = targetVelocity;
                float torqueDir = (UnityEngine.Random.value - 0.5f) * 2;
                rb.AddRelativeTorque(Vector3.forward * plushieSpinSpeed * torqueDir);
            }

            GameObject newMuzzleFX = GameObject.Instantiate<GameObject>(Prefabs.CanLauncher_MuzzleFX, firePoint.position, Quaternion.identity);
            newMuzzleFX.AddComponent<DestroyOnDisable>();
            newMuzzleFX.transform.forward = firePoint.forward;
            newMuzzleFX.transform.parent = firePoint.parent;

            CameraController.Instance.CameraShake(0.30f);
            animator.Play("CanLauncher_Fire", 0, 0);

        }

        //Pick a random plushie that isnt the last one fired.
        private GameObject PickPlushie()
        {
            GameObject[] sorted = plushies;
            if (lastFired != null && plushies.Length > 1)
            {
                sorted = plushies.Where(x => x != lastFired && x != null).ToArray();
            }
            return sorted[UnityEngine.Random.Range(0, sorted.Length)];
        }


        private void OnEnable()
        {
            animator.Play("CanLauncher_Equip");
        }

        public override string GetDebuggingText()
        {
            string debug = base.GetDebuggingText();
            if (lastFired != null)
                debug += $"LAST_FIRED: {lastFired.name}\n";
            return debug;
        }
    }
}
