using UnityEngine;

namespace UltraFunGuns
{
    [UFGWeapon("Bulletstorm", "Bulletstorm", 3, true, WeaponIconColor.Green, false)]
    public class Bulletstorm : UltraFunGunBase
    {


        public float spreadTightness = 1.5f;
        public float fireRateSpeed = 0.02f;
        public float maxRange = 200.0f;

        public int fireAmount = 5;

        private ActionCooldown fireCooldown = new ActionCooldown(0.75f, true);
        private ActionCooldown fireRate = new ActionCooldown(0.02f, true);

        public override void GetInput()
        {
            if(MonoSingleton<InputManager>.Instance.InputSource.Fire1.IsPressed && fireCooldown.CanFire() && !om.paused)
            {
                Shoot();
            }
        }

        private void Shoot()
        {
            if (!fireRate.CanFire())
                return;

            Ray shot = new Ray();

            Vector2 spread = UnityEngine.Random.insideUnitCircle;

            shot.origin = mainCam.transform.position;
            shot.direction = mainCam.TransformDirection(spread.x, spread.y, spreadTightness);

            for (int i = 0; i < fireAmount; i++)
            {
                Vector3[] bulletTrailInfo = HydraUtils.DoRayHit(shot, maxRange, false, 0.05f, false, 0.0f, this.gameObject, true, true);

                if (bulletTrailInfo.Length != 2)
                {
                    bulletTrailInfo = new Vector3[] { shot.GetPoint(maxRange), shot.direction * -1 };
                }

                CreateBulletTrail(firePoint.position, bulletTrailInfo[0], bulletTrailInfo[1]);
            }
        }

        private void CreateBulletTrail(Vector3 startPosition, Vector3 endPosition, Vector3 normal)
        {
            if(Prefabs.BulletTrail == null)
                return;

            GameObject newBulletTrail = Instantiate<GameObject>(Prefabs.BulletTrail, endPosition, Quaternion.identity);
            newBulletTrail.transform.up = normal;
            LineRenderer line = newBulletTrail.GetComponent<LineRenderer>();
            Vector3[] linePoints = new Vector3[2] { startPosition, endPosition };
            line.SetPositions(linePoints);
        }


    }
}
