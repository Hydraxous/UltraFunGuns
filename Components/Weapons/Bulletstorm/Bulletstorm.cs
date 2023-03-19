﻿using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    [UFGWeapon("Bulletstorm", "Bulletstorm", 3, true, WeaponIconColor.Green)]
    public class Bulletstorm : UltraFunGunBase
    {

        private GameObject[] projectiles = new GameObject[] 
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
            Prefabs.ScottPlush.Asset
        };

        public float spreadTightness = 1.5f;
        public float fireRateSpeed = 0.02f;
        public float maxRange = 200.0f;

        public override void OnAwakeFinished()
        {
            //HydraLoader.prefabRegistry.TryGetValue("BulletTrail", out bulletTrailPrefab);

        }

        private void Start()
        {

        }

        public override void GetInput()
        {
            if(MonoSingleton<InputManager>.Instance.InputSource.Fire1.IsPressed && actionCooldowns["primaryCooldown"].CanFire() && !om.paused)
            {
                Shoot();
            }
        }

        private void Shoot()
        {
            if(actionCooldowns["fireRate"].CanFire())
            {
                Ray shot = new Ray();

                Vector2 spread = UnityEngine.Random.insideUnitCircle;

                shot.origin = mainCam.transform.position;
                shot.direction = mainCam.TransformDirection(spread.x, spread.y, spreadTightness);

                GameObject randProjectile = null;
                int count = 0;

                while (randProjectile == null && (count < projectiles.Length))
                {
                    int randIndex = UnityEngine.Random.Range(0, projectiles.Length);         
                    randProjectile = projectiles[randIndex];
                    ++count;
                }    

                if(randProjectile == null)
                {
                    return;
                }

                GameObject newProjectile = GameObject.Instantiate<GameObject>(randProjectile, firePoint.position + (shot.direction * 2.2f), Quaternion.identity);
                
                if(newProjectile.TryGetComponent<Rigidbody>(out Rigidbody rb))
                {
                    rb.velocity = shot.direction * 70.0f;
                }

                if(newProjectile.TryGetComponent<IUFGInteractionReceiver>(out IUFGInteractionReceiver ufgRec))
                {
                    ufgRec.Parried(shot.direction);
                }

                return;

                Vector3[] bulletTrailInfo = HydraUtils.DoRayHit(shot, maxRange, false, 0.05f, false, 0.0f, this.gameObject, true, true);
                if(bulletTrailInfo.Length != 2)
                {
                    bulletTrailInfo = new Vector3[] { shot.GetPoint(maxRange), shot.direction * -1 };
                }
                CreateBulletTrail(firePoint.position, bulletTrailInfo[0], bulletTrailInfo[1]);

            }
        }

        private void CreateBulletTrail(Vector3 startPosition, Vector3 endPosition, Vector3 normal)
        {
            if(Prefabs.BulletTrail == null)
            {
                return;
            }

            GameObject newBulletTrail = Instantiate<GameObject>(Prefabs.BulletTrail, endPosition, Quaternion.identity);
            newBulletTrail.transform.up = normal;
            LineRenderer line = newBulletTrail.GetComponent<LineRenderer>();
            Vector3[] linePoints = new Vector3[2] { startPosition, endPosition };
            line.SetPositions(linePoints);
        }

        public override Dictionary<string, ActionCooldown> SetActionCooldowns()
        {
            Dictionary<string, ActionCooldown> cooldowns = new Dictionary<string, ActionCooldown>();
            cooldowns.Add("primaryCooldown", new ActionCooldown(0.75f, true));
            cooldowns.Add("fireRate", new ActionCooldown(0.02f));
            return cooldowns;
        }

    }
}
