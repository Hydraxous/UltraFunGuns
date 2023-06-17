using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using UltraFunGuns;
using UnityEngine;

[UFGWeapon("RocketDagger","Rocket Dagger", 2, false, WeaponIconColor.Red, false, false)]
[WeaponAbility("Dagger", "Press Fire 1 to throw a dagger", 0, RichTextColors.aqua)]
[WeaponAbility("Daggers", "Press Fire 2 to throw multiple daggers", 0, RichTextColors.aqua)]
public class RocketDagger : UltraFunGunBase
{
    [UFGAsset("RocketDaggerProjectile")] public static GameObject RocketDaggerProjectilePrefab;

    private ActionCooldown primaryFireCooldown = new ActionCooldown(0.15f);
    private ActionCooldown secondaryFireCooldown = new ActionCooldown(0.6f);

    public int PelletsPerSecond = 12;
    public float maxShotgunTime = 5.0f;

    private float shotgunTime = 0;

    public override void GetInput()
    {
        if (MonoSingleton<InputManager>.Instance.InputSource.Fire1.IsPressed && primaryFireCooldown.CanFire() && !om.paused)
        {
            primaryFireCooldown.AddCooldown();
            Fire();
        }

        if (MonoSingleton<InputManager>.Instance.InputSource.Fire2.IsPressed)
        {
            if(secondaryFireCooldown.CanFire() && !om.paused)
            {
                shotgunTime += Time.deltaTime;
                shotgunTime = Mathf.Min(maxShotgunTime, shotgunTime);
                
                if (shotgunTime >= maxShotgunTime)
                    Shotgun();
            }
        }else if(shotgunTime > 0.0f)
        {
            Shotgun();
        }
    }


    private void Fire()
    {
        FirePellet(mainCam.position, mainCam.forward);
    }

    private void Shotgun()
    {
        int pelletCount = GetPelletCount();

        Vector3 camPos = mainCam.position;
        Quaternion camRot = mainCam.rotation;

        if(shotgunTime == maxShotgunTime)
        {
            //pelletCount = 1;
        }

        for(int i=0;i< pelletCount; i++)
        {
            Vector3 direction = Vector3.forward;

            if(i != 0)
            {
                direction = MathTools.GetProjectileSpreadVector(shotgunTime / 20.0f);
            }

            GameObject newPellet = FirePellet(camPos, camRot*direction);

            if (newPellet.TryFindComponent<RayBasedProjectile>(out RayBasedProjectile proj))
            {
                proj.sourceWeapon = gameObject;
                
                if(shotgunTime == maxShotgunTime)
                {
                    proj.damage *= 10f;
                    proj.ricochet = true;
                    proj.projectileSpeed *= 10.0f;
                }
            }

            newPellet.transform.Rotate(Vector3.forward, UnityEngine.Random.Range(0, 360), Space.Self);
        }

        secondaryFireCooldown.AddCooldown(shotgunTime/2.0f);
        shotgunTime = 0.0f;
    }

    private GameObject FirePellet(Vector3 position, Vector3 direction)
    {
        return GameObject.Instantiate(RocketDaggerProjectilePrefab, position, Quaternion.LookRotation(direction, Vector3.up));
        
    }

    private int GetPelletCount()
    {
        float pelletCount = shotgunTime * PelletsPerSecond;
        pelletCount = Mathf.CeilToInt(pelletCount);
        return (int) pelletCount;
    }
}