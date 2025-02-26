using Configgy;
using System.Collections.Generic;
using UnityEngine;

namespace UltraFunGuns
{
    public class WeaponDeployer : MonoBehaviour
    {
        
        private static ConfigKeybind[] slotKeys = new ConfigKeybind[]
        {
            UFGInput.Slot7Key,
            UFGInput.Slot8Key,
            UFGInput.Slot9Key,
            UFGInput.Slot10Key
        };

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

        public void DisposeWeapons()
        {
            for (int i = 0; i < customSlots.Count; i++)
            {
                for (int j = 0; j < customSlots[i].Count; j++)
                {
                    GameObject toDestroy = customSlots[i][j];
                    customSlots[i][j] = null;
                    Destroy(toDestroy);
                }
                customSlots[i].Clear();
            }
        }

        //TODO optimization
        public void DeployWeapons()
        {
            DisposeWeapons();

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
                        UltraFunGuns.Log.LogError($"Weaponkey {weaponKey} doesn't exist. Someone seriously screwed up (it was Hydra).");
                        this.enabled = false;
                        return;
                    }

                    if (!HydraLoader.prefabRegistry.TryGetValue(weaponKey, out GameObject weaponPrefab))
                    {
                        UltraFunGuns.Log.LogError($"Weapon Manager could not retrieve {weaponKey} from prefab registry. Skipping...");
                        continue;
                    }

                    //Set layers correctly
                    weaponPrefab.layer = 13;
                    Transform[] childs = weaponPrefab.GetComponentsInChildren<Transform>();
                    foreach (Transform child in childs)
                    {
                        child.gameObject.layer = 13;
                    }

                    //Instantiate weapon and add it's component
                    GameObject newWeapon = GameObject.Instantiate<GameObject>(weaponPrefab, this.transform);
                    newWeapon.AddComponent(weaponInfo.Type);
                    newWeapon.SetActive(false);
                    customSlots[i].Add(newWeapon);
                    weaponsGiven += weaponKey + " ";
                    weaponsDeployed.Add(weaponInfo);
                    WeaponsDeployed++;
                }
            }

            for (int j = 0; j < customSlots.Count; j++)
            {
                if (gc.slots.Contains(customSlots[j]))
                {
                    gc.slots.Remove(customSlots[j]);
                }
            }

            if (weaponsGiven.Length > 0)
            {
                weaponsGiven = "Weapons given: " + weaponsGiven;
                for (int i = 0; i < customSlots.Count; i++)
                {
                    gc.slots.Add(customSlots[i]);
                }

                WeaponManager.OnWeaponsDeployed?.Invoke(weaponsDeployed.ToArray());
                UltraFunGuns.Log.Log(weaponsGiven);
            }
        }

        //This handles input for the extra slots
        private void Update()
        {
            for (int i = 0; i < slotKeys.Length; i++)
            {
                if (slotKeys[i].WasPeformed() && (customSlots[i].Count > 1 || gc.currentSlotIndex != i + WeaponManager.SLOT_OFFSET))
                {
                    if (customSlots[i].Count > 0 && customSlots[i][0] != null)
                    {
                        //int nextIndex = (gc.currentSlotIndex != i) ? 0 : (gc.currentVariationIndex + 1) % ((customSlots[i].Count > 0) ? customSlots[i].Count : 1); //Prevent division by zero.
                        gc.SwitchWeapon(i + WeaponManager.SLOT_OFFSET, null, false, false, false); //Hopefully this works.
                    }
                }
            }
        }
    }
}
