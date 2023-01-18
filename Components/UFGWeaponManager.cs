using System.Collections;
using System.Collections.Generic;
using UMM;
using UnityEngine;
using HarmonyLib;
using UnityEngine.SceneManagement;

namespace UltraFunGuns
{
    //Single instance component which is applied directly to the GunControl object and inserts the custom weapons into the player's inventory.
    //TODO make this object non-destroyable and deploy on scene load and stuff
    public class UFGWeaponManager : MonoBehaviour
    {
        GunControl gc;

        // Assigned by UMM
        public static UKKeyBind[] UFGSlotKeys = {
            UKAPI.GetKeyBind("<color=orange>UFG</color> Slot 7", KeyCode.Alpha7),
            UKAPI.GetKeyBind("<color=orange>UFG</color> Slot 8", KeyCode.Alpha8),
            UKAPI.GetKeyBind("<color=orange>UFG</color> Slot 9", KeyCode.Alpha9),
            UKAPI.GetKeyBind("<color=orange>UFG</color> Slot 10", KeyCode.Alpha0)
        };
        public static UKKeyBind SecretButton = UKAPI.GetKeyBind("<color=orange>UFG</color> Secret", KeyCode.K);

        private List<List<string>> weaponKeySlots = new List<List<string>>();

        public static int SlotOffset = 7;

        //Empty slots for the weapons. Don't remove this.
        private List<List<GameObject>> customSlots = new List<List<GameObject>>()
        {
            new List<GameObject>(),
            new List<GameObject>(),
            new List<GameObject>(),
            new List<GameObject>()
        };

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
                        Debug.LogError("UFG: WeaponManager component couldn't deploy weapons.\nUFG: " + e.Message);
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

        //This handles input for the extra slots
        private void Update()
        {
            for(int i=0; i < UFGSlotKeys.Length; i++)
            {
                if(UFGSlotKeys[i].WasPerformedThisFrame && (customSlots[i].Count > 1 || gc.currentSlot != i+SlotOffset))
                {
                    if (customSlots[i].Count > 0 && customSlots[i][0] != null)
                    {
                        gc.SwitchWeapon(i+SlotOffset, customSlots[i], false, false);
                    }
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
