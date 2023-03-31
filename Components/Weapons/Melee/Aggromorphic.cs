﻿using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraFunGuns.Components.Weapons.Melee
{
    [WeaponAbility("Assault", "Use the Aggromorph to it's fullest ability.", 0, RichTextColors.aqua)]
    [WeaponAbility("Morph", "Morph the weapon.", 1, RichTextColors.lime)]
    [UFGWeapon("Aggromorphic", "Aggromorphic", 2, true, WeaponIconColor.Red, true)]
    public class Aggromorphic : UltraFunGunBase
    {
        public float damage = 1.0f;
        public float maxRange = 3.75f;
        private ActionCooldown fireCooldown = new ActionCooldown(0.3f, true);

        private List<AggromorphicWeapon> subWeapons = new List<AggromorphicWeapon>();

        private int selectedWeapon;

        private AggromorphicWeapon currentWeapon
        {
            get
            {
                return subWeapons[selectedWeapon];
            }
        }

        public override void OnAwakeFinished()
        {
            subWeapons = new List<AggromorphicWeapon>(GetComponentsInChildren<AggromorphicWeapon>(true));
            foreach(AggromorphicWeapon weapon in subWeapons)
            {
                weapon.gameObject.SetActive(false);
            }
        }

        public override void GetInput()
        {
            if (InputManager.Instance.InputSource.Fire1.IsPressed && !om.paused && fireCooldown.CanFire())
            {
                fireCooldown.AddCooldown(currentWeapon.hitCooldown);
                Fire();
            }

            if(InputManager.Instance.InputSource.Fire2.WasPerformedThisFrame && !om.paused)
            {
                EquipNext();
            }

            if (WeaponManager.SecretButton.WasPerformedThisFrame && !om.paused)
            {
                currentWeapon?.SecretAnimation();
            }
        }


        private void EquipNext()
        {
            currentWeapon?.gameObject.SetActive(false);
            selectedWeapon = (selectedWeapon + 1 >= subWeapons.Count) ? 0 : selectedWeapon+1;
            currentWeapon?.gameObject.SetActive(true);
        }

        private bool hitEnvironmentAudio = false;

        private void Fire()
        {
            hitEnemies.Clear();
            hitEnvironmentAudio = false;
            currentWeapon?.Hit();
            if (HydraUtils.SphereCastAllMacro(mainCam.position, 0.25f, mainCam.forward, maxRange, out RaycastHit[] hits))
            {
                foreach (RaycastHit hit in hits)
                {
                    ProcessHit(hit);
                }
            }
        }

        List<EnemyIdentifier> hitEnemies = new List<EnemyIdentifier>();
        private void ProcessHit(RaycastHit hit)
        {
            if (hit.collider.IsColliderEnvironment() && !hitEnvironmentAudio)
            {
                currentWeapon?.PlayHitAudio(hit.point, 0.6f);
                GameObject bulletDecal = Instantiate(Prefabs.BulletImpactFX, hit.point+(hit.normal*0.01f), Quaternion.identity);
                bulletDecal.transform.up = hit.normal;
                hitEnvironmentAudio = true;
                return;
            }

            if (!hit.collider.IsColliderEnemy(out EnemyIdentifier eid, false))
                return;

            if (hitEnemies.Contains(eid))
                return;

            currentWeapon?.PlayHitAudio(hit.point, 0.2f);
            eid.DeliverDamage(eid.gameObject, mainCam.forward * 5000.0f, hit.point, currentWeapon.damage, true, 1.0f, gameObject);
            hitEnemies.Add(eid);
        }

        private void OnEnable()
        {
            currentWeapon?.gameObject.SetActive(true);
        }
    }
}