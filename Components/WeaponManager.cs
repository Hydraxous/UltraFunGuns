using System;
using System.Collections.Generic;
using System.Reflection;
using UMM;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UltraFunGuns
{
    public static class WeaponManager
    {
        public const int SLOTS = 4, SLOT_OFFSET = 7;
        // Assigned by UMM
        public static UKKeyBind[] UFGSlotKeys = {
            UKAPI.GetKeyBind("<color=orange>UFG</color> Slot 7", KeyCode.Alpha7),
            UKAPI.GetKeyBind("<color=orange>UFG</color> Slot 8", KeyCode.Alpha8),
            UKAPI.GetKeyBind("<color=orange>UFG</color> Slot 9", KeyCode.Alpha9),
            UKAPI.GetKeyBind("<color=orange>UFG</color> Slot 10", KeyCode.Alpha0)
        };
        public static UKKeyBind SecretButton = UKAPI.GetKeyBind("<color=orange>UFG</color> Secret", KeyCode.K);

        public static void Init()
        {
            RegisterWeapons();
            UKAPIP.OnLevelChanged += OnLevelChanged;
            UltraFunGuns.UFG.OnModUnloaded.AddListener(DeInit);
        }

        private static void DeInit()
        {
            UKAPIP.OnLevelChanged -= OnLevelChanged;
            weapons.Clear();
            WeaponsRegistered = false;
        }

        private static WeaponDeployer deployer;

        private static InventoryControllerDeployer inventoryDeployer;

        [Commands.UFGDebugMethod("Deploy Weapons", "Redeploy weapons")]
        public static void DeployWeapons(bool firstTime = false)
        {
            if(!UKAPI.InLevel())
            {
                return;
            }
  
            if (deployer == null)
            {
                GunControl gc = MonoSingleton<GunControl>.Instance;
                if (gc.TryGetComponent<WeaponDeployer>(out WeaponDeployer ultraFGPatch))
                {
                    deployer = ultraFGPatch;
                }
                else
                {
                    deployer = gc.gameObject.AddComponent<WeaponDeployer>();
                }
            }

            deployer.DeployWeapons(firstTime);
        }

        private static void OnLevelChanged(UKAPIP.UKLevelType levType)
        {
            if (!UKAPIP.InLevel())
            {
                return;
            }

            NewStyleItem("vaporized", "<color=cyan>VAPORIZED</color>");
            NewStyleItem("vibecheck", "VIBE-CHECKED");
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

            CanvasController canvas = MonoSingleton<CanvasController>.Instance;
            if (!canvas.TryGetComponent<InventoryControllerDeployer>(out InventoryControllerDeployer invControllerDeployer))
            {
                inventoryDeployer = canvas.gameObject.AddComponent<InventoryControllerDeployer>();
            }
            else
            {
                inventoryDeployer = invControllerDeployer;
            }
        }

        private static Dictionary<string, FunGun> weapons;

        public static Dictionary<string, FunGun> Weapons
        {
            get
            {
                if(!WeaponsRegistered || weapons == null)
                {
                    RegisterWeapons();
                }
                return weapons;
            }
        }

        public static FunGun[] GetWeapons()
        {
            List<FunGun> weaponInfos = new List<FunGun>();

            foreach(KeyValuePair<string,FunGun> info in Weapons)
            {
                weaponInfos.Add(info.Value);
            }

            return weaponInfos.ToArray();
        }

        public static int WeaponCount
        {
            get
            {
                return Weapons.Count;
            }
        }

        public static bool WeaponsRegistered { get; private set; } = false;
        public static bool UsingWeapons => (DeployedWeapons > 0);

        public static int DeployedWeapons
        {
            get
            {
                if (deployer == null)
                    return 0;

                return deployer.WeaponsDeployed;
            }
        }

        //Registers any weapons tagged with the WeaponInfo attribute
        public static void RegisterWeapons()
        {
            if(WeaponsRegistered)
            {
                return;
            }

            Assembly assembly = Assembly.GetExecutingAssembly();

            foreach (Type type in assembly.GetTypes())
            {
                var attribute = type.GetCustomAttribute<FunGun>();
                if (attribute != null)
                {
                    if(weapons == null)
                    {
                        weapons = new Dictionary<string, FunGun>();
                    }

                    if (!weapons.ContainsKey(attribute.WeaponKey))
                    {
                        List<WeaponAbility> weaponAbilities = new List<WeaponAbility>();
                        IEnumerable<WeaponAbility> abilities = type.GetCustomAttributes<WeaponAbility>();
                        foreach (WeaponAbility ability in abilities)
                        {
                            if(ability != null)
                            {
                                if (!weaponAbilities.Contains(ability))
                                {
                                    weaponAbilities.Add(ability);
                                }
                            }                   
                        }

                        attribute.SetAbilities(weaponAbilities.ToArray());
                        attribute.SetType(type);

                        int slot = ((attribute.Slot < 0) ? 0 : (attribute.Slot > SLOTS) ? SLOTS - 1 : attribute.Slot);

                        HydraLogger.Log($"Found weapon: {attribute.DisplayName}");
                        weapons.Add(attribute.WeaponKey, attribute);
                    }
                }
            }

            WeaponsRegistered = true;
        }

        public static InventorySlotData[] GetDefaultLoadout()
        {

            List<List<InventoryNodeData>> slotData = new List<List<InventoryNodeData>>();

            for (int i = 0; i < SLOTS; i++)
            {
                slotData.Add(new List<InventoryNodeData>());
            }

            FunGun[] infos = GetWeapons();
            
            for(int i=0;i<infos.Length;i++)
            {
                int slot = ((infos[i].Slot < 0) ? 0 : (infos[i].Slot > slotData.Count) ? slotData.Count - 1 : infos[i].Slot);

                InventoryNodeData newNodeData = new InventoryNodeData(infos[i].WeaponKey, infos[i].Equipped);
                if (!slotData[slot].Contains(newNodeData))
                {
                    slotData[slot].Add(newNodeData);
                    HydraLogger.Log($"Found weapon: {infos[i].DisplayName}");
                }
            }

            InventorySlotData[] slotInfo = new InventorySlotData[slotData.Count];

            for (int i = 0; i < slotData.Count; i++)
            {
                if (slotData[i].Count > 0)
                {
                    slotInfo[i] = new InventorySlotData(slotData[i].ToArray());
                }
                else
                {
                    slotInfo[i] = new InventorySlotData();
                }
            }

            return slotInfo;
        }

        //simplified way to add new style items for this mod.
        public static void NewStyleItem(string name, string text)
        {
            //This call will return true if the style item is NOT in the dictionary.
            if (MonoSingleton<StyleHUD>.Instance.GetLocalizedName("hydraxous.ultrafunguns."+ name) == "hydraxous.ultrafunguns." + name)
            {
                MonoSingleton<StyleHUD>.Instance.RegisterStyleItem("hydraxous.ultrafunguns." + name, text);
            }
        }

        public static Color GetColor(WeaponIconColor colorType)
        {
            Color color = MonoSingleton<ColorBlindSettings>.Instance.variationColors[(int)colorType];
            return color;
        }
    }

    public class WeaponDeployer : MonoBehaviour
    {

        public int WeaponsDeployed { get; private set; }

        GunControl gc;

        private void Awake()
        {
            gc = GetComponent<GunControl>();
        }

        private List<List<string>> weaponKeySlots = new List<List<string>>();


        //Empty slots for the weapons. Don't remove this.
        private List<List<GameObject>> customSlots = new List<List<GameObject>>()
        {
            new List<GameObject>(),
            new List<GameObject>(),
            new List<GameObject>(),
            new List<GameObject>()
        };

        public List<List<string>> CreateWeaponKeyset(Data.UFG_Loadout invControllerData)
        {
            List<List<string>> newWeaponKeys = new List<List<string>>();
            for (int x = 0; x < invControllerData.slots.Length; x++)
            {
                List<string> newWeaponKeyList = new List<string>();
                for (int y = 0; y < invControllerData.slots[x].slotNodes.Length; y++)
                {
                    if (invControllerData.slots[x].slotNodes[y].weaponEnabled)
                    {
                        newWeaponKeyList.Add(invControllerData.slots[x].slotNodes[y].weaponKey);
                    }
                }
                newWeaponKeys.Add(newWeaponKeyList);
            }
            return newWeaponKeys;
        }

        public void DeployWeapons(bool firstTime = false)
        {
            bool deploy = true;

            if (firstTime)
            {
                deploy = (UKAPIP.CurrentSceneName == "Level 0-1");
            }

            if (!deploy)
            {
                //return;
            }

            foreach (List<GameObject> customSlot in customSlots)
            {
                customSlot.Clear();
            }

            weaponKeySlots = CreateWeaponKeyset(Data.Loadout.Data);

            if (!(weaponKeySlots.Count > 0))
            {
                return;
            }

            WeaponsDeployed = 0;

            try
            {
                string weaponsGiven = "";
                for (int i = 0; i < weaponKeySlots.Count; i++)
                {
                    if (weaponKeySlots[i].Count > 0)
                    {
                        foreach (string weaponKey in weaponKeySlots[i])
                        {
                            if (WeaponManager.Weapons.TryGetValue(weaponKey, out FunGun weaponInfo))
                            {
                                if (HydraLoader.prefabRegistry.TryGetValue(weaponKey, out GameObject weaponPrefab))
                                {
                                    weaponPrefab.layer = 13;
                                    Transform[] childs = weaponPrefab.GetComponentsInChildren<Transform>();
                                    foreach (Transform child in childs)
                                    {
                                        child.gameObject.layer = 13;
                                    }
                                    weaponPrefab.SetActive(false);
                                    GameObject newWeapon = GameObject.Instantiate<GameObject>(weaponPrefab, this.transform);
                                    newWeapon.AddComponent(weaponInfo.Type);
                                    customSlots[i].Add(newWeapon);
                                    weaponsGiven += weaponKey + " ";
                                    WeaponsDeployed++;
                                }
                                else
                                {
                                    HydraLogger.Log($"Weapon Manager could not retrieve {weaponKey} from prefab registry.", DebugChannel.Error);
                                }
                            }
                            else
                            {
                                HydraLogger.Log($"Weaponkey {weaponKey} doesn't exist. Someone seriously screwed up (it was Hydra).", DebugChannel.Fatal);
                                this.enabled = false;
                                return;
                            }


                        }
                    }
                }

                if(weaponsGiven.Length > 0)
                {
                    weaponsGiven = "Weapons given: " + weaponsGiven;
                    AddWeapons();
                    HydraLogger.Log(weaponsGiven, DebugChannel.User);
                }
            }
            catch (System.Exception e)
            {
                HydraLogger.Log("WeaponManager component couldn't deploy weapons.\nERROR: " + e.Message, DebugChannel.Fatal);
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
        }
    }
}
