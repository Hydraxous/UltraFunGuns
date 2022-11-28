using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace UltraFunGuns
{
    public class RemoteBomb : UltraFunGunBase
    {
        private GameObject remoteExplosivePrefab;
        public float bombDetonationDelay = 0.16f;
        public float throwForce = 15.0f;

        private List<RemoteBombExplosive> thrownExplosives = new List<RemoteBombExplosive>();
        private bool throwingExplosive = false;

        public override void OnAwakeFinished()
        {
            HydraLoader.prefabRegistry.TryGetValue("RemoteBomb_Explosive", out remoteExplosivePrefab);
        }

        public override void FirePrimary()
        {
            base.FirePrimary();
        }

        public override void FireSecondary()
        {
            if(actionCooldowns["secondaryFire"].CanFire())
            {
                if(thrownExplosives.Count > 0)
                {
                    actionCooldowns["secondaryFire"].AddCooldown();
                    List<RemoteBombExplosive> bombsToDetonate = new List<RemoteBombExplosive>();
                    foreach (RemoteBombExplosive bomb in thrownExplosives)
                    {
                        if (bomb.CanDetonate())
                        {
                            bombsToDetonate.Add(bomb);
                        }
                    }

                    foreach (RemoteBombExplosive bomb in bombsToDetonate)
                    {
                        thrownExplosives.Remove(bomb);
                    }

                    StartCoroutine(DetonateExplosives(bombsToDetonate));
                }else
                {
                    Debug.Log("No bombs? ,':^)");
                }
                
            }
        }

        private IEnumerator ThrowExplosive()
        {
            throwingExplosive = true;
            //TODO do anim here and program alignment timing
            yield return new WaitForSeconds(0.08f);
            Ray aimRay = HydraUtils.GetProjectileAimVector(mainCam, firePoint, 0.25f, 40.0f);
            GameObject newExplosive = Instantiate<GameObject>(remoteExplosivePrefab, firePoint.position, Quaternion.identity);
            RemoteBombExplosive newBomb = newExplosive.GetComponent<RemoteBombExplosive>();

            newBomb.Initiate(this, player);
            newBomb.transform.forward = -aimRay.direction;
            newBomb.SetVelocity(aimRay.direction * throwForce);

            thrownExplosives.Add(newBomb);

            throwingExplosive = false;
        }

        private IEnumerator DetonateExplosives(List<RemoteBombExplosive> bombs)
        { 
            while(bombs.Count > 0)
            {
                RemoteBombExplosive bomb = bombs[0];
                bombs.Remove(bomb);
                bomb.Detonate();

                float timer = bombDetonationDelay;
                while(timer > 0.0f)
                {
                    timer -= Time.deltaTime;
                    yield return new WaitForEndOfFrame();
                }
            }
        }

        public override void DoAnimations()
        {
            
        }

        public override Dictionary<string, ActionCooldown> SetActionCooldowns()
        {
            Dictionary<string, ActionCooldown> cooldowns = new Dictionary<string, ActionCooldown>();
            cooldowns.Add("primaryFire", new ActionCooldown(0.85f));
            cooldowns.Add("secondaryFire", new ActionCooldown(0.25f));
            return cooldowns;
        }

        private void OnDisable()
        {
            throwingExplosive = false;
        }
    }
}



