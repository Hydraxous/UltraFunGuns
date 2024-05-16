using System;
using UnityEngine;

namespace UltraFunGuns
{
    //[UFGWeapon("PlutonRifle", "Pluton Rifle", 2, true, WeaponIconColor.Green, true, false)]
    public class PlutonRifle : UltraFunGunBase
    {
        /* Pluton Rifle
         * 
         * Firing once allows you to fire off two more shots in quick succession if you fire within a certain time frame
         * if you miss the timeframe the weapon will overheat and you need to wait for the cooldown to restore before firing again
         * 
         * When you fire the weapon it is causing critical mass of elements and if the energy is not vented it will auto-vent causing a cooldown essentially.
         * 
         * upon hitting an enemy with all three shots it will cause the enemy to detonate
         * 
         * Passive ->
         * Shots will slightly drift towards nearby targets
         * 
         * Skill Check
         * Firing 3 shots rapidly and aiming all of them well.
         * 
         * Reward -> enemy takes heavy damage from detonation
         * ^
         * Feedback -> three glowing indicators of charge, also should blend colors to display shot timing window more accurately
         * v
         * Bad Usage -> Gun will lock up on cooldown for a moment and will be unusable for that duration.
         * 
         * 
         * Additional Tech ->
         * 
         * Weapon swapping -> instant cooldown reset
         * 
         * Synergy -> 
         * Alchemical Enrichment -> Hit 3 shots on a coin to instantly fire upon three enemies
         * Purification -> Firing on a focalyzer crystal increases its dps
         * Enrichment -> When shooting enemy projectiles, they become supercharged, if they are parried, they will release a core nuke explosion on impact.
         * 
         */

        #region Configurable
        [Configgy.Configgable("Weapons/Pluton Rifle")]
        private static int shotBurstCount = 3;

        [Configgy.Configgable("Weapons/Pluton Rifle")]
        private static float maxShotWindowTime = 0.75f;

        [Configgy.Configgable("Weapons/Pluton Rifle")]
        private static float minShotWindowTime = 0.25f;

        //Max cooldown is 3 seconds, minimum is 0.5

        [Configgy.Configgable("Weapons/Pluton Rifle")]
        private static float ventCooldownPerShot = 1.25f;
        
        [Configgy.Configgable("Weapons/Pluton Rifle")]
        private static float minimumBurstCooldown = 0.5f;

        [Configgy.Configgable("Weapons/Pluton Rifle")]
        private static float maxRange = 2000f;

        [Configgy.Configgable("Weapons/Pluton Rifle")]
        private static float projectileSpeed = 14f;
        #endregion

        private float lastFireTime;
        private float timeSinceLastFire => Time.time - lastFireTime;

        private float ventTimeLeft;

        private int burstShotsRemaining = 0;

        private LayerMask hitMask => LayerMaskDefaults.Get(LMD.EnemiesAndEnvironment);

        private static GameObject plutonProjectilePrefab; //Logic projectile
        [UFGAsset("PlutonShot")] private static GameObject plutonProjectileVisualPrefab; //fake visual
        //[UFGAsset("PlutonShot_ImpactFX")] private static GameObject plutonShotImpactFX; 

        private bool firePressedThisFrame => InputManager.Instance.InputSource.Fire1.WasPerformedThisFrame && !om.paused;


        private void Update()
        {
            HandlePrimaryAction();
            HandleVenting();
        }

        private void HandlePrimaryAction()
        {
            if (ventTimeLeft > 0) //Gun is venting. Cant fire or go on cooldown
                return;

            if (firePressedThisFrame)
                FireUser();
            else if (burstShotsRemaining > 0 && burstShotsRemaining < shotBurstCount) //User started firing, but shots are still remaining
                CheckOverheat();
        }

        private void HandleVenting()
        {
            if (ventTimeLeft <= 0)
                return;

            ventTimeLeft = Mathf.Max(0, ventTimeLeft - Time.deltaTime);

            if (ventTimeLeft > 0)
                return;

            //Cooldown over.
            burstShotsRemaining = shotBurstCount;
        }

        private void CheckOverheat()
        {
            if (timeSinceLastFire < maxShotWindowTime)
                return;

            //User failed to fire the rest of the shots.
            float ventCooldown = minimumBurstCooldown + (burstShotsRemaining * ventCooldownPerShot);
            AddVentCooldown(ventCooldown);
        }

        private void AddVentCooldown(float ventCooldownTime)
        {
            ventTimeLeft = Mathf.Max(0, ventTimeLeft + ventCooldownTime);
        }

        private void FireUser()
        {
            if (timeSinceLastFire < minShotWindowTime)
                return;

            FireProjectile((p,v) =>
            {
                RayProjectile projectile = p.AddComponent<RayProjectile>();
                projectile.StepDistance = projectileSpeed;
                projectile.Hitmask = hitMask;
                projectile.OnHit += LogicHit;

                RayProjectile fakeProj = v.AddComponent<RayProjectile>();
                fakeProj.StepDistance = projectileSpeed;
                fakeProj.Hitmask = hitMask;
                fakeProj.OnHit += VisualHit;

                Vector3 fakeProjVector = firePoint.forward;

                if (Physics.Raycast(mainCam.position, mainCam.forward, out RaycastHit hit, maxRange, hitMask))
                    fakeProjVector = (hit.point - firePoint.position).normalized;

                v.transform.position = firePoint.position;
                v.transform.forward = fakeProjVector;
            });

            HydraLogger.Log("Pluton: Bang!");

            lastFireTime = Time.time;
            --burstShotsRemaining;

            //Check if burst finished.
            if (burstShotsRemaining > 0)
                return;

            AddVentCooldown(minimumBurstCooldown);
        }

        private void FireProjectile(Action<GameObject, GameObject> onIntance)
        {
            GameObject logicProjectile = GameObject.Instantiate(plutonProjectilePrefab, mainCam.position, Quaternion.LookRotation(mainCam.forward));
            GameObject visualProjectile = GameObject.Instantiate(plutonProjectileVisualPrefab, firePoint.position, Quaternion.LookRotation(mainCam.forward));
            onIntance?.Invoke(logicProjectile, visualProjectile);
        }

        private void LogicHit(RaycastHit hit, RayProjectile projectile)
        {
            if(hit.collider.IsColliderEnemy(out EnemyIdentifier eid))
            {
                
            }
        }

        //Destroy visual projectile. We no longer need it.
        private void VisualHit(RaycastHit hit, RayProjectile projectile)
        {
            projectile.transform.position = hit.point;
            //GameObject.Instantiate(plutonShotImpactFX, hit.point, Quaternion.LookRotation(hit.normal));
            Destroy(projectile.gameObject);
        }


        private void OnEnable()
        {
            burstShotsRemaining = shotBurstCount;
            ventTimeLeft = Mathf.Max(0, ventTimeLeft - (Time.time - lastFireTime)); //Subtract last fire cooldown, this is probably weird. Check it later.
        }

        public override string GetDebuggingText()
        {
            string debug = base.GetDebuggingText();
            debug += $"BSR: {burstShotsRemaining}\n";
            debug += $"VTL: {ventTimeLeft}\n";
            debug += $"LFT: {lastFireTime}\n";
            return debug;
        }
    }
}
