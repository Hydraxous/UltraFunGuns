using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace UltraFunGuns
{
    [UFGWeapon("RemoteBomb","Radio Explosive", 3, true, WeaponIconColor.Red)]
    [WeaponAbility("Throw", "Press <color=orange>Fire 1</color> to throw an explosive.", 0, RichTextColors.aqua)]
    [WeaponAbility("Detonate", "Press <color=orange>Fire 2</color> detonate armed explosives.", 1, RichTextColors.lime)]
    [WeaponAbility("Mortar", "Parry a thrown bomb to boost your throw.", 2, RichTextColors.yellow)]
    [WeaponAbility("Demolition", "Clumps of bombs have increased yield.", 3, RichTextColors.yellow)]
    public class RemoteBomb : UltraFunGunBase
    {
        [UFGAsset("RemoteBomb_Explosive")] public static GameObject RemoteExplosivePrefab { get; private set; }


        [Configgy.Configgable("Weapons/Radio Explosive")]
        private static float bombDetonationDelay = 0.035f;

        [Configgy.Configgable("Weapons/Radio Explosive")]
        private static float throwForce = 50.0f;

        [Configgy.Configgable("Weapons/Radio Explosive")]
        private static float primaryFireCooldown = 0.45f;

        [Configgy.Configgable("Weapons/Radio Explosive")]
        private static float secondaryFireCooldown = 0.25f;

        private List<RemoteBombExplosive> thrownExplosives = new List<RemoteBombExplosive>();
        private bool throwingExplosive = false;

        private ActionCooldown throwBombCD = new ActionCooldown(primaryFireCooldown, true), detonateBombCD = new ActionCooldown(secondaryFireCooldown, true);

        public override void OnAwakeFinished()
        {
            AddSFX("Throw", "Detonate"); //yikes
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
            if(throwBombCD.CanFire())
            {
                if(!throwingExplosive)
                {
                    throwBombCD.AddCooldown();
                    StartCoroutine(ThrowExplosive());
                }
            }
        }

        //Detonate currently active and armed bombs
        public override void FireSecondary()
        {
            if(detonateBombCD.CanFire())
            {
                if(thrownExplosives.Count > 0)
                {
                    detonateBombCD.AddCooldown();
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
            Ray aimRay = (weaponIdentifier.duplicate) ? new Ray(firePoint.position, mainCam.forward) : HydraUtils.GetProjectileAimVector(mainCam, firePoint, 0.25f, 40.0f);
            GameObject newExplosive = Instantiate<GameObject>(RemoteExplosivePrefab, firePoint.position, Quaternion.identity);
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

        private void OnDisable()
        {
            throwingExplosive = false;
        }

        public override string GetDebuggingText()
        {
            string debugText = base.GetDebuggingText();

            debugText +=
                $"BOMBCOUNT: {thrownExplosives.Count}\n" +
                $"THROW_CD: {throwBombCD}\n" +
                $"DETONATE_CD: {detonateBombCD}";

            return debugText;
        }
    }
}



