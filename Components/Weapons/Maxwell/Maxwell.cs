using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    [WeaponAbility("Cat Power", "Explodes everything in existence it's kind of funny if you think about it.", 0, RichTextColors.fuchsia)]
    [UFGWeapon("Maxwell","Maxwell", 3, false, WeaponIconColor.Yellow, true)]
    public class Maxwell : UltraFunGunBase
    {
        private ActionCooldown killCooldown = new ActionCooldown(3.0f, true);

        private float maxPower = 2.0f;
        private float power;

        private float maxRange = 25.0f;

        public override void GetInput()
        {
            if(InputManager.Instance.InputSource.Fire1.IsPressed)
            {
                power += Time.deltaTime;
                if(power >= maxPower)
                {
                    if (killCooldown.CanFire())
                        Kill();
                }

            }else
            {
                power -= Time.deltaTime;
            }

            power = Mathf.Clamp(power, 0.0f, maxPower);
        }

        private void Kill()
        {

            Collider[] hitColliders = Physics.OverlapSphere(mainCam.position, maxRange).Where(x=>x.IsColliderEnemy(out EnemyIdentifier eid)).ToArray();

            if(hitColliders.Length > 0)
            {
                if(hitColliders[UnityEngine.Random.Range(0,hitColliders.Length)].IsColliderEnemy(out EnemyIdentifier enemy))
                {
                    enemy.Explode();
                    killCooldown.AddCooldown(UnityEngine.Random.Range(killCooldown.FireDelay-(killCooldown.FireDelay*0.5f), killCooldown.FireDelay + killCooldown.FireDelay));
                }
            }
        }
    }
}
