using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    //Throwable and droppable egg which deals damage. Can be shot in mid air for a funny explosion.
    [UFGWeapon("EggToss", "Egg", 1, true, WeaponIconColor.Yellow)]
    [WeaponAbility("Egg", "Press <color=orange>Fire 1</color> to throw an egg.", 0, RichTextColors.aqua)]
    [WeaponAbility("Egg Drop", "Press <color=orange>Fire 2</color> to drop an egg.", 1, RichTextColors.aqua)]
    [WeaponAbility("EGGSPLOSION", "Shoot an egg to release an <color=red>EGGSLPOSION</color>.", 2, RichTextColors.red)]

    public class EggToss : UltraFunGunBase
    {
        [UFGAsset("ThrownEgg")] public static GameObject ThrownEggPrefab { get; private set; }

        private ActionCooldown throwEgg = new ActionCooldown(0.6f, true), dropEgg = new ActionCooldown(0.3f, true);

        public float forceMultiplier = 59.0f;
        private bool throwingEgg = false;

        protected override void DoAnimations()
        {
            bool ableToShoot = (throwEgg.CanFire() || dropEgg.CanFire());
            animator.SetBool("CanShoot", ableToShoot);
        }

        //Drops egg directly below player with minimal no velocity inheritance.
        IEnumerator DropEgg()
        {
            throwingEgg = true;
            animator.Play("EggTossDrop");
            yield return new WaitForSeconds(0.15f);
            GameObject newThrownEgg = GameObject.Instantiate<GameObject>(ThrownEggPrefab, player.transform.TransformPoint(0,-1.5f,0), Quaternion.identity);
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
            GameObject newThrownEgg = GameObject.Instantiate<GameObject>(ThrownEggPrefab, firePoint.position, Quaternion.identity);
            newThrownEgg.transform.forward = mainCam.forward;
            Vector3 newVelocity = mainCam.TransformDirection(0, 0, 1 * forceMultiplier);
            newVelocity += player.rb.velocity;
            newThrownEgg.GetComponent<Rigidbody>().velocity = newVelocity;
            throwingEgg = false;
        }

        public override void GetInput()
        {
            if (MonoSingleton<InputManager>.Instance.InputSource.Fire1.WasPerformedThisFrame && throwEgg.CanFire() && !throwingEgg)
            {
                throwEgg.AddCooldown();
                StartCoroutine(ThrowEgg());
            }else if(MonoSingleton<InputManager>.Instance.InputSource.Fire2.WasPerformedThisFrame && dropEgg.CanFire() && !throwingEgg)
            {
                dropEgg.AddCooldown();
                StartCoroutine(DropEgg());
            }
        }

        private void OnDisable()
        {
            throwingEgg = false;
        }

    }
}
