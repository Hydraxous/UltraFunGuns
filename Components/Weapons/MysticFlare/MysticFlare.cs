using UnityEngine;

namespace UltraFunGuns
{

    [UFGWeapon("MysticFlare", "Mystic Flare", 0, true, WeaponIconColor.Red, false)]
    [WeaponAbility("Flare", "Throw a flare using <color=orange>Fire 1</color>.", 6, RichTextColors.red)]
    public class MysticFlare : UltraFunGunBase
    {
        [UFGAsset("MysticFlareProjectile")] private static GameObject flareProjectilePrefab;
        [UFGAsset("MysticFlareExplosion")] public static GameObject MysticFlareExplosion { get; private set; }
        public float maxRange = 13.0f;

        private MysticFlareProjectile deployedFlare;

        public override void GetInput()
        {
            if (MonoSingleton<InputManager>.Instance.InputSource.Fire1.IsPressed)
            {
                if(deployedFlare ==null)
                    DeployFlare();

            }else if (deployedFlare != null)
            {
                ExplodeFlare();
            }
        }

        //Throw flare
        private void DeployFlare()
        {
            GameObject newFlare = Instantiate(flareProjectilePrefab, firePoint.position, Quaternion.identity);
            newFlare.transform.forward = mainCam.forward;
            deployedFlare = newFlare.GetComponent<MysticFlareProjectile>();

        }

        private void ExplodeFlare()
        {
            if(deployedFlare == null)
            {
                return;
            }

            deployedFlare.Detonate();
            deployedFlare = null;
        }

        public override string GetDebuggingText()
        {
            string debug = base.GetDebuggingText();
            debug += $"FLARE: {deployedFlare != null}\n";
            if (deployedFlare != null)
            {
                debug += $"FLARE_DIST: {Vector3.Distance(deployedFlare.transform.position,mainCam.transform.position)}\n";
                debug += $"FLARE_SPEED: {deployedFlare.MoveSpeed.ToString("0.000")}\n";
            }
            return debug;
        }
    }

}