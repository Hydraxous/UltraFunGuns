using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;
using UnityEngine.SceneManagement;

namespace UltraFunGuns
{
    //Single instance patch which is applied directly to the GunControl object and inserts the custom weapons into the player's inventory.
    public class UFGWeaponManager : MonoBehaviour
    {
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

        //Assigned by plugin main class
        public KeyCode Slot7Key;
        public KeyCode Slot8Key;
        public KeyCode Slot9Key;
        public KeyCode Slot10Key;

        //Use for intializing style items
        private void Awake()
        {
            gc = GetComponent<GunControl>();
            NewStyleItem("vaporized", "<color=cyan>VAPORIZED</color>");
            NewStyleItem("vibecheck","VIBE-CHECKED");
            NewStyleItem("v2kill", "<color=#ff33001>OXIDIZED</color>");
            NewStyleItem("gabrielkill", "<color=#ff0051>L-DISTRIBUTED</color>");
            NewStyleItem("wickedkill", "<color=#919191>NOT WICKED ENOUGH</color>");
            NewStyleItem("minoskill", "<color=#03ffa7>JUDGED</color>");
            NewStyleItem("orbited", "ORBITAL LAUNCH");
            NewStyleItem("egged", "EGGED");
            NewStyleItem("eggstrike", "TACTICAL EGG STRIKE");
            NewStyleItem("eggsplosion", "<color=yellow>EGGSPLOSION</color>");
            NewStyleItem("refractionhit", "BUG ZAPPER");
            NewStyleItem("discohit", "DISCO INFERNO");
            NewStyleItem("dodgeballhit", "OUTPLAYED");
            NewStyleItem("dodgeballparry", "BOOST BALL");
            NewStyleItem("dodgeballparryhit", "<color=orange>SLAM DUNK</color>");
            NewStyleItem("dodgeballreversehit", "REBOUND");
            NewStyleItem("fingergunhit", "BANG'D");
            NewStyleItem("fingergunfullpenetrate", "<color=cyan>KABOOMA!</color>");
            NewStyleItem("fingergunprojhit", "DENIAL");



            DeployWeapons();
        }

        public List<List<string>> CreateWeaponKeyset(InventoryControllerData invControllerData)
        {
            List<List<string>> newWeaponKeys = new List<List<string>>();
            for (int x = 0; x < invControllerData.slots.Length; x++)
            {
                List<string> newWeaponKeyList = new List<string>();
                for (int y = 0; y < invControllerData.slots[x].slotNodes.Length; y++)
                {
                    if(invControllerData.slots[x].slotNodes[y].weaponEnabled)
                    {
                        newWeaponKeyList.Add(invControllerData.slots[x].slotNodes[y].weaponKey);
                    }
                }
                newWeaponKeys.Add(newWeaponKeyList);
            }
            return newWeaponKeys;
        }

        //Gets weapon prefabs from the Data loader and instantiates them into the world and adds them to the gun controllers lists.
        public void DeployWeapons(bool firstTime = false)
        {
            string sceneName = SceneManager.GetActiveScene().name;
            bool deploy = true;

            if(sceneName == "Level 0-1" && firstTime)
            {
                deploy = true;
            }
            else if(firstTime)
            {
                deploy = false;
            }

            if(deploy)
            {
                foreach (List<GameObject> customSlot in customSlots)
                {
                    customSlot.Clear();
                }

                weaponKeySlots = CreateWeaponKeyset(InventoryDataManager.GetInventoryData());
                if(weaponKeySlots.Count > 0)
                {
                    try
                    {
                        string weaponsGiven = "UFG: Weapons given: ";
                        for (int i = 0; i < weaponKeySlots.Count; i++)
                        {
                            if (weaponKeySlots[i].Count > 0)
                            {
                                foreach (string weaponKey in weaponKeySlots[i])
                                {
                                    HydraLoader.prefabRegistry.TryGetValue(weaponKey, out GameObject weaponPrefab);
                                    weaponPrefab.layer = 13;
                                    Transform[] childs = weaponPrefab.GetComponentsInChildren<Transform>();
                                    foreach (Transform child in childs)
                                    {
                                        child.gameObject.layer = 13;
                                    }
                                    weaponPrefab.SetActive(false);
                                    customSlots[i].Add(GameObject.Instantiate<GameObject>(weaponPrefab, this.transform));
                                    weaponsGiven += weaponKey + " ";
                                }
                            }
                        }
                        Debug.Log(weaponsGiven);
                        AddWeapons();
                    }
                    catch (System.Exception e)
                    {
                        Debug.Log("UFG: WeaponManager component couldn't deploy weapons.\nUFG: " + e.Message);
                    }
                }
                
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

        //TODO fix this, for some reason the input on switching to weapons doesn't work past slot 7. No its not because of the keycodes, that was an attempt to fix it. Inspect the GunControl class closer.
        private void Update()
        {
            if (Input.GetKeyDown(Slot7Key) && (customSlots[0].Count > 1 || gc.currentSlot != 7))
            {
                if (customSlots[0].Count > 0 && customSlots[0][0] != null)
                {
                    gc.SwitchWeapon(7, customSlots[0], false, false);
                }
            }else if (Input.GetKeyDown(Slot8Key) && (customSlots[1].Count > 1 || gc.currentSlot != 8))
            {
                if (customSlots[1].Count > 0 && customSlots[1][0] != null)
                {
                    gc.SwitchWeapon(8, customSlots[1], false, false);
                }
            }
            else if(Input.GetKeyDown(Slot9Key) && (customSlots[2].Count > 1 || gc.currentSlot != 9))
            {
                if (customSlots[2].Count > 0 && customSlots[2][0] != null)
                {
                    gc.SwitchWeapon(9, customSlots[2], false, false);
                }
            }
            else if(Input.GetKeyDown(Slot10Key) && (customSlots[3].Count > 1 || gc.currentSlot != 10))
            {
                if (customSlots[3].Count > 0 && customSlots[3][0] != null)
                {
                    gc.SwitchWeapon(10, customSlots[3], false, false);
                }
            }
        }

        //simplified way to add new style items for this mod.
        private void NewStyleItem(string name, string text)
        {
            if (MonoSingleton<StyleHUD>.Instance.GetLocalizedName("hydraxous.ultrafunguns."+ name) == "hydraxous.ultrafunguns." + name)
            {
                MonoSingleton<StyleHUD>.Instance.RegisterStyleItem("hydraxous.ultrafunguns." + name, text);
            }
        }
    }
}
