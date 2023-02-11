using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Threading.Tasks;

namespace UltraFunGuns
{
    [FunGun("RemoteBomb","Radio Explosive", 3, true, WeaponIconColor.Red)]
    public class RemoteBomb : UltraFunGunBase
    {
        private GameObject remoteExplosivePrefab;
        public float bombDetonationDelay = 0.035f;
        public float throwForce = 50.0f;

        private List<RemoteBombExplosive> thrownExplosives = new List<RemoteBombExplosive>();
        private bool throwingExplosive = false;

        public override void OnAwakeFinished()
        {
            AddSFX("Throw", "Detonate");
            HydraLoader.prefabRegistry.TryGetValue("RemoteBomb_Explosive", out remoteExplosivePrefab);
        }

        public override void GetInput()
        {
            if (MonoSingleton<InputManager>.Instance.InputSource.Fire1.WasPerformedThisFrame && !om.paused)
            {
                FirePrimary();
            }

            if (MonoSingleton<InputManager>.Instance.InputSource.Fire2.WasPerformedThisFrame && !om.paused)
            {
                FireSecondary();
            }
        }

        //Throwbomb
        public override void FirePrimary()
        {
            HydraLogger.Log($"Remote bomb fired primary");
            if(actionCooldowns["primaryFire"].CanFire())
            {
                if(!throwingExplosive)
                {
                    actionCooldowns["primaryFire"].AddCooldown();
                    StartCoroutine(ThrowExplosive());
                }
            }
        }

        //Detonate currently active and armed bombs
        public override void FireSecondary()
        {
            if(actionCooldowns["secondaryFire"].CanFire())
            {
                if(thrownExplosives.Count > 0)
                {
                    actionCooldowns["secondaryFire"].AddCooldown();
                    animator.Play("RemoteBomb_Anim_Detonate", 0, 0.0f);
                    PlaySFX("Detonate", 0.85f, 1.0f);
                    List<RemoteBombExplosive> bombsToDetonate = new List<RemoteBombExplosive>();
                    List<RemoteBombExplosive> currentBombs = new List<RemoteBombExplosive>(thrownExplosives);

                    foreach (RemoteBombExplosive bomb in currentBombs)
                    {
                        if(bomb != null)
                        {
                            if (bomb.CanDetonate())
                            {
                                bombsToDetonate.Add(bomb);
                            }
                        }else
                        {
                            thrownExplosives.Remove(bomb);
                        }
                    }

                    foreach (RemoteBombExplosive bomb in bombsToDetonate)
                    {
                        thrownExplosives.Remove(bomb);
                    }

                    DetonateExplosives(bombsToDetonate);
                }else
                {
                    HydraLogger.Log($"({gameObject.name}) No bombs to detonate");
                }
                
            }
        }

        private IEnumerator ThrowExplosive()
        {
            HydraLogger.Log($"({gameObject.name}) Throwing explosive");
            animator.Play("RemoteBomb_Anim_Throw", 0, 0.0f);
            throwingExplosive = true;
            yield return new WaitForSeconds(0.1666f);
            PlaySFX("Throw", 0.95f, 1.15f);
            Ray aimRay = HydraUtils.GetProjectileAimVector(mainCam, firePoint, 0.25f, 40.0f);
            GameObject newExplosive = Instantiate<GameObject>(remoteExplosivePrefab, firePoint.position, Quaternion.identity);
            RemoteBombExplosive newBomb = newExplosive.GetComponent<RemoteBombExplosive>();

            newBomb.Initiate(this, player);
            newBomb.transform.forward = -aimRay.direction;
            newBomb.SetVelocity(aimRay.direction * throwForce);

            thrownExplosives.Add(newBomb);

            throwingExplosive = false;
        }

        private async void DetonateExplosives(List<RemoteBombExplosive> bombs)
        {
            while (bombs.Count > 0)
            {
                RemoteBombExplosive bomb = bombs[0];
                bombs.Remove(bomb);

                if (bomb != null)
                {
                    bomb.Detonate();

                    float timer = bombDetonationDelay;
                    while (timer > 0.0f)
                    {
                        timer -= Time.deltaTime;
                        await Task.Yield();
                    }
                }
            }
        }

        /*
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

        */
        protected override void DoAnimations()
        {
            
        }

        public void BombDetonated(RemoteBombExplosive bomb)
        {
            if(thrownExplosives.Contains(bomb))
            {
                thrownExplosives.Remove(bomb);
            }
        }

        public override Dictionary<string, ActionCooldown> SetActionCooldowns()
        {
            Dictionary<string, ActionCooldown> cooldowns = new Dictionary<string, ActionCooldown>();
            cooldowns.Add("primaryFire", new ActionCooldown(0.45f));
            cooldowns.Add("secondaryFire", new ActionCooldown(0.25f));
            return cooldowns;
        }

        private void OnDisable()
        {
            throwingExplosive = false;
        }
    }
}



