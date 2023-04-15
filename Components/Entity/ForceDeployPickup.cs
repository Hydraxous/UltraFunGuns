using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraFunGuns.Components.Entity
{
    public class ForceDeployPickup : MonoBehaviour
    {
        private bool pickedUp;

        private void OnTriggerEnter(Collider col)
        {
            if(col.gameObject.tag == "Player" || col.gameObject.name.Contains("Player"))
            {
                Pickup();
            }
        }

        private void Pickup()
        {
            if (pickedUp)
                return;

            pickedUp = true;
            WeaponManager.ForceDeploy();
            Destroy(gameObject);
        }
    }
}
