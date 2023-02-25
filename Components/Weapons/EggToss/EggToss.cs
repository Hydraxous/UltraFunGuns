using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    //Throwable and droppable egg which deals damage. Can be shot in mid air for a funny explosion.
    [FunGun("EggToss", "Egg", 1, true, WeaponIconColor.Yellow)]
    public class EggToss : UltraFunGunBase
    {
        private GameObject thrownEggPrefab;

        public float forceMultiplier = 59.0f;
        private bool throwingEgg = false;

        private void Start()
        {
            HydraLoader.prefabRegistry.TryGetValue("ThrownEgg", out thrownEggPrefab);
            HydraLoader.prefabRegistry.TryGetValue("EggImpactFX", out thrownEggPrefab.GetComponent<ThrownEgg>().impactFX);
            HydraLoader.prefabRegistry.TryGetValue("EggSplosion", out thrownEggPrefab.GetComponent<ThrownEgg>().eggsplosionPrefab);
        }

        public override Dictionary<string, ActionCooldown> SetActionCooldowns()
        {
            Dictionary<string, ActionCooldown> cooldowns = new Dictionary<string, ActionCooldown>();
            cooldowns.Add("primaryFire", new ActionCooldown(0.6f, true));
            cooldowns.Add("secondaryFire", new ActionCooldown(0.3f, true));
            return cooldowns;
        }

        protected override void DoAnimations()
        {
            bool ableToShoot = (actionCooldowns["primaryFire"].CanFire() || actionCooldowns["secondaryFire"].CanFire());
            animator.SetBool("CanShoot", ableToShoot);
        }

        //Drops egg directly below player with minimal no velocity inheritance.
        IEnumerator DropEgg()
        {
            throwingEgg = true;
            animator.Play("EggTossDrop");
            yield return new WaitForSeconds(0.15f);
            GameObject newThrownEgg = GameObject.Instantiate<GameObject>(thrownEggPrefab, player.transform.TransformPoint(0,-1.5f,0), Quaternion.identity);
            newThrownEgg.transform.forward = mainCam.forward;
            newThrownEgg.GetComponent<ThrownEgg>().dropped = true;
            newThrownEgg.GetComponent<Rigidbody>().velocity = Vector3.down;
            throwingEgg = false;
        }

        //Throws egg inheriting the velocity of the player.
        IEnumerator ThrowEgg()
        {
            throwingEgg = true;
            animator.Play("EggTossThrow");
            yield return new WaitForSeconds(0.16f);
            GameObject newThrownEgg = GameObject.Instantiate<GameObject>(thrownEggPrefab, firePoint.position, Quaternion.identity);
            newThrownEgg.transform.forward = mainCam.forward;
            Vector3 newVelocity = mainCam.TransformDirection(0, 0, 1 * forceMultiplier);
            newVelocity += player.rb.velocity;
            newThrownEgg.GetComponent<Rigidbody>().velocity = newVelocity;
            throwingEgg = false;
        }

        public override void GetInput()
        {
            if (MonoSingleton<InputManager>.Instance.InputSource.Fire1.WasPerformedThisFrame && actionCooldowns["primaryFire"].CanFire() && !throwingEgg)
            {
                actionCooldowns["primaryFire"].AddCooldown();
                StartCoroutine(ThrowEgg());
            }else if(MonoSingleton<InputManager>.Instance.InputSource.Fire2.WasPerformedThisFrame && actionCooldowns["secondaryFire"].CanFire() && !throwingEgg)
            {
                actionCooldowns["secondaryFire"].AddCooldown();
                StartCoroutine(DropEgg());
            }
        }

        private void OnDisable()
        {
            throwingEgg = false;
        }

    }
}
