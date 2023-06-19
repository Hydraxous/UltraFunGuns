using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UltraFunGuns;
using UnityEngine;

namespace UltraFunGuns
{
    public class WeaponDeployer : MonoBehaviour
    {
        public int WeaponsDeployed { get; private set; }

        GunControl gc;

        private List<List<string>> weaponKeySlots = new List<List<string>>();

        //Empty slots for the weapons. Don't remove this.
        private List<List<GameObject>> customSlots = new List<List<GameObject>>()
        {
            new List<GameObject>(),
            new List<GameObject>(),
            new List<GameObject>(),
            new List<GameObject>()
        };

        private void Awake()
        {
            gc = GetComponent<GunControl>();
        }

        public List<List<string>> CreateWeaponKeyset(Loadout invControllerData)
        {
            List<List<string>> newWeaponKeys = new List<List<string>>();
            for (int x = 0; x < invControllerData.slots.Length; x++)
            {
                List<string> newWeaponKeyList = new List<string>();
                for (int y = 0; y < invControllerData.slots[x].slotNodes.Length; y++)
                {
                    if (invControllerData.slots[x].slotNodes[y].weaponEnabled && (invControllerData.slots[x].slotNodes[y].weaponUnlocked || UltraFunGuns.DebugMode))
                    {
                        newWeaponKeyList.Add(invControllerData.slots[x].slotNodes[y].weaponKey);
                    }
                }
                newWeaponKeys.Add(newWeaponKeyList);
            }
            return newWeaponKeys;
        }

        public void RemoveWeapons()
        {
            for (int i = 0; i < customSlots.Count; i++)
            {
                for (int x = 0; x < customSlots[i].Count; x++)
                {
                    GameObject toDestroy = customSlots[i][x];
                    customSlots[i][x] = null;
                    Destroy(toDestroy);
                }
                customSlots[i].Clear();
            }
        }

        //TODO optimization
        public void DeployWeapons(bool firstTime = false)
        {
            RemoveWeapons();

            weaponKeySlots = CreateWeaponKeyset(Data.Loadout.Data);

            if (!(weaponKeySlots.Count > 0))
            {
                return;
            }

            WeaponsDeployed = 0;


            List<UFGWeapon> weaponsDeployed = new List<UFGWeapon>();

            string weaponsGiven = "";
            for (int i = 0; i < weaponKeySlots.Count; i++)
            {
                if (weaponKeySlots[i].Count <= 0)
                {
                    continue;
                }

                foreach (string weaponKey in weaponKeySlots[i])
                {
                    if (!WeaponManager.Weapons.TryGetValue(weaponKey, out UFGWeapon weaponInfo))
                    {
                        HydraLogger.Log($"Weaponkey {weaponKey} doesn't exist. Someone seriously screwed up (it was Hydra).", DebugChannel.Fatal);
                        this.enabled = false;
                        return;
                    }

                    if (!HydraLoader.prefabRegistry.TryGetValue(weaponKey, out GameObject weaponPrefab))
                    {
                        HydraLogger.Log($"Weapon Manager could not retrieve {weaponKey} from prefab registry. Skipping...", DebugChannel.Error);
                        continue;
                    }

                    //Set layers correctly
                    weaponPrefab.layer = 13;
                    Transform[] childs = weaponPrefab.GetComponentsInChildren<Transform>();
                    foreach (Transform child in childs)
                    {
                        child.gameObject.layer = 13;
                    }

                    //weaponPrefab.SetActive(false);

                    //Instantiate weapon and add it's component
                    GameObject newWeapon = GameObject.Instantiate<GameObject>(weaponPrefab, this.transform);
                    newWeapon.AddComponent(weaponInfo.Type);
                    newWeapon.SetActive(false);
                    customSlots[i].Add(newWeapon);
                    weaponsGiven += weaponKey + " ";
                    WeaponManager.AddWeaponToFreshnessDict(newWeapon);
                    weaponsDeployed.Add(weaponInfo);
                    WeaponsDeployed++;
                }
            }

            if (weaponsGiven.Length > 0)
            {
                weaponsGiven = "Weapons given: " + weaponsGiven;
                AddWeapons();
                WeaponManager.OnWeaponsDeployed?.Invoke(weaponsDeployed.ToArray());
                HydraLogger.Log(weaponsGiven, DebugChannel.User);
            }
        }

        //adds weapons to the gun controller
        private void AddWeapons()
        {
            for (int j = 0; j < customSlots.Count; j++)
            {
                if (gc.slots.Contains(customSlots[j]))
                {
                    gc.slots.Remove(customSlots[j]);
                }
            }

            for (int i = 0; i < customSlots.Count; i++)
            {
                gc.slots.Add(customSlots[i]);
                foreach (GameObject wep in customSlots[i])
                {
                    if (!gc.allWeapons.Contains(wep))
                    {
                        gc.allWeapons.Add(wep);
                    }
                }
            }
        }

        //This handles input for the extra slots
        private void Update()
        {
            for (int i = 0; i < WeaponManager.UFGSlotKeys.Length; i++)
            {
                if (WeaponManager.UFGSlotKeys[i].WasPerformedThisFrame && (customSlots[i].Count > 1 || gc.currentSlot != i + WeaponManager.SLOT_OFFSET))
                {
                    if (customSlots[i].Count > 0 && customSlots[i][0] != null)
                    {
                        gc.SwitchWeapon(i + WeaponManager.SLOT_OFFSET, customSlots[i], false, false);
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.Keypad4))
            {
                if (spawnedRod != null)
                {
                    return;
                }

                spawnedRod = GameObject.Instantiate<GameObject>(Prefabs.FishingRod.Asset, this.transform);
                spawnedRod.SetActive(false);
                customSlots[0].Add(spawnedRod);
                WeaponManager.AddWeaponToFreshnessDict(spawnedRod);
                gc.allWeapons.Add(spawnedRod);
            }
        }

        private GameObject spawnedRod;
    }
}
