using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using HydraDynamics.Keybinds;
using UltraFunGuns.Datas;
using HydraDynamics;
using HydraDynamics.Events;
using System.Data;
using System.Transactions;

namespace UltraFunGuns
{
    public static class WeaponManager
    {
        public const int SLOTS = 4, SLOT_OFFSET = 7;

        public static Keybinding[] UFGSlotKeys = new Keybinding[] 
        {
            Hydynamics.GetKeybinding("Slot 7", KeyCode.Alpha7),
            Hydynamics.GetKeybinding("Slot 8", KeyCode.Alpha8),
            Hydynamics.GetKeybinding("Slot 9", KeyCode.Alpha9),
            Hydynamics.GetKeybinding("Slot 10", KeyCode.Alpha0),
        };

        public static Keybinding SecretButton = Hydynamics.GetKeybinding("Secret Button", KeyCode.K);

        public delegate void OnWeaponsDeployedHandler(UFGWeapon[] weapons);
        public static OnWeaponsDeployedHandler OnWeaponsDeployed;

        public static void Init()
        {
            RegisterWeapons();
            InGameCheck.OnLevelChanged += OnLevelChanged;
            CrossModEvents.SubscribeToModEvents(CheckEvent, "WeaponDeployer");
        }

        private static void CheckEvent(ModEventData data)
        {
            if(data.targetSubscriptionKey != "WeaponDeployer")
                return;

            if (data.targetModGUID != ConstInfo.GUID)
                return;

            if (data.eventName != "RedeployWeapons")
                return;

            DeployWeapons();

            CrossModEvents.UnsubscribeFromModEvents("WeaponDeployer");
        }

        private static void DeInit()
        {
            InGameCheck.OnLevelChanged -= OnLevelChanged;
            weapons.Clear();
            WeaponsRegistered = false;
        }

        private static WeaponDeployer deployer;

        private static InventoryControllerDeployer inventoryDeployer;

        [Commands.UFGDebugMethod("Deploy Weapons", "Redeploy weapons")]
        public static void DeployWeapons(bool firstTime = false, bool force = false)
        {
            if(!InGameCheck.InLevel() && !force && !inventoryMade)
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

        private static void OnLevelChanged(InGameCheck.UKLevelType levType)
        {
            inventoryMade = false;

            if (!InGameCheck.InLevel())
            {
                return;
            }

            Prep();
        }

        private static bool inventoryMade;

        private static void Prep()
        {
            if (inventoryMade)
                return;

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
            NewStyleItem("admingunkill", "<color=red>GAMING CHAIR</color>");
            NewStyleItem("brickmindkill", "<color=orange>SOUL ASSIMILATION</color>");
            NewStyleItem("brickparrykill", "BRICKED");
            NewStyleItem("tricksniperquickscope", "QUICKSCOPE");
            NewStyleItem("tricksniper360", "<color=cyan>TRICKSHOT</color>");
            NewStyleItem("tricksnipernoscope", "<color=orange>NOSCOPE</color>");
            NewStyleItem("tricksniperbankshot", "<color=orange>BANKSHOT</color>");
            NewStyleItem("ultragunsuperchargekill", "<color=cyan>EXPRESS BILLING</color>");
            NewStyleItem("ultragunkill", "BILLED");
            NewStyleItem("ultragunrainkill", "<color=red>WHAT GOES UP</color>");
            NewStyleItem("ultragunaerialkill", "DEATH FROM ABOVE");

            CanvasController canvas = MonoSingleton<CanvasController>.Instance;
            if (!canvas.TryGetComponent<InventoryControllerDeployer>(out InventoryControllerDeployer invControllerDeployer))
            {
                inventoryDeployer = canvas.gameObject.AddComponent<InventoryControllerDeployer>();
            }
            else
            {
                inventoryDeployer = invControllerDeployer;
            }

            inventoryMade = true;
        }

        private static Dictionary<string, UFGWeapon> weapons;

        public static Dictionary<string, UFGWeapon> Weapons
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

        public static UFGWeapon[] GetWeapons()
        {
            List<UFGWeapon> weaponInfos = new List<UFGWeapon>();

            foreach(KeyValuePair<string,UFGWeapon> info in Weapons)
            {
                weaponInfos.Add(info.Value);
            }

            return weaponInfos.ToArray();
        }

        public static UFGWeapon GetWeaponInfo(Type t)
        {
            UFGWeapon weaponInfo = (UFGWeapon)Attribute.GetCustomAttribute(t, typeof(UFGWeapon));

            if (weaponInfo == null)
            {
                HydraLogger.Log($"Weapon info null when requested for type {t.ToString()}", DebugChannel.Fatal);
            }

            return weaponInfo;
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

        //Registers any weapons tagged with the UFGWeapon attribute
        public static void RegisterWeapons()
        {
            if(WeaponsRegistered)
            {
                return;
            }

            Assembly assembly = Assembly.GetExecutingAssembly();

            foreach (Type type in assembly.GetTypes())
            {
                var attribute = type.GetCustomAttribute<UFGWeapon>();

                if (attribute == null)
                {
                    continue;
                }

                if (attribute.WeaponKey == "NULL")
                {
                    continue;
                }

                if (weapons == null)
                {
                    weapons = new Dictionary<string, UFGWeapon>();
                }

                if (weapons.ContainsKey(attribute.WeaponKey))
                {
                    continue;
                }


                List<WeaponAbility> weaponAbilities = new List<WeaponAbility>();
                IEnumerable<WeaponAbility> abilities = type.GetCustomAttributes<WeaponAbility>();
                foreach (WeaponAbility ability in abilities)
                {
                    if (ability == null)
                    {
                        continue;
                    }

                    if (!weaponAbilities.Contains(ability))
                    {
                        weaponAbilities.Add(ability);
                    }

                }

                attribute.SetAbilities(weaponAbilities.ToArray());
                attribute.SetType(type);

                int slot = ((attribute.Slot < 0) ? 0 : (attribute.Slot > SLOTS) ? SLOTS - 1 : attribute.Slot);

                HydraLogger.Log($"Found weapon: {attribute.DisplayName}");
                weapons.Add(attribute.WeaponKey, attribute);
            }

            WeaponsRegistered = true;
        }

        //Credit to Agent of Nyarlathotep for this
        private static Dictionary<GameObject, float> FreshnessList
        {
            get
            {               
                var field = typeof(StyleHUD).GetField("weaponFreshness", BindingFlags.NonPublic | BindingFlags.Instance);
                Dictionary<GameObject, float> freshnessList = field.GetValue(MonoSingleton<StyleHUD>.Instance) as Dictionary<GameObject, float>;          
                return freshnessList;
            }

            set
            {
                if(value != null)
                {
                    var field = typeof(StyleHUD).GetField("weaponFreshness", BindingFlags.NonPublic | BindingFlags.Instance);
                    field.SetValue(MonoSingleton<StyleHUD>.Instance, value);
                }     
            }
        }

        /// <summary>
        /// Adds weapon to style hud freshness
        /// </summary>
        /// <param name="go">gameObject to add</param>
        /// <returns></returns>
        public static bool AddWeaponToFreshnessDict(GameObject go)
        {

            if(go == null)
            {
                HydraLogger.Log($"WeaponManager: Attempted to register null gameobject into freshness dict.", DebugChannel.Error);
                return false;
            }

            try
            {
                Dictionary<GameObject, float> freshnessDict = FreshnessList;
                if (!freshnessDict.ContainsKey(go))
                {
                    freshnessDict.Add(go, 10f);
                    FreshnessList = freshnessDict;
                    return true;
                }

                HydraLogger.Log($"WeaponManager: Attempted to register existing weapon to freshness dict.", DebugChannel.Error);
                return false;

            } catch (Exception ex)
            {
                HydraLogger.Log($"WeaponManager: Could not register {go.name} to freshness dict.\n{ex.Message}\n{ex.StackTrace}", DebugChannel.Fatal);
            }

            return false;
        }

        public static InventorySlotData[] GetDefaultLoadout()
        {

            List<List<InventoryNodeData>> slotData = new List<List<InventoryNodeData>>();

            for (int i = 0; i < SLOTS; i++)
            {
                slotData.Add(new List<InventoryNodeData>());
            }

            UFGWeapon[] infos = GetWeapons();
            
            for(int i=0;i<infos.Length;i++)
            {
                int slot = ((infos[i].Slot < 0) ? 0 : (infos[i].Slot > slotData.Count) ? slotData.Count - 1 : infos[i].Slot);

                InventoryNodeData newNodeData = new InventoryNodeData(infos[i].WeaponKey, infos[i].Equipped, infos[i].StartUnlocked);
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

        public static void AddStyle(int points, string key, GameObject sourceWeapon = null, EnemyIdentifier eid = null, int count = -1, string prefix = "", string postfix ="")
        {
            if(!InGameCheck.InLevel())
            {
                return;
            }

            StyleHUD.Instance.AddPoints(points, $"hydraxous.ultrafunguns.{key}", sourceWeapon, eid, count, prefix, postfix);
        }

        public static void AddStyle(StyleEntry entry)
        {
            if (!InGameCheck.InLevel())
            {
                return;
            }

            StyleHUD.Instance.AddPoints(entry.Points, $"hydraxous.ultrafunguns.{entry.Key}", entry.SourceWeapon, entry.EnemyIdentifier, entry.Count, entry.Prefix, entry.Postfix);
        }

        public static Color GetColor(WeaponIconColor colorType)
        {
            Color color = MonoSingleton<ColorBlindSettings>.Instance.variationColors[(int)colorType];
            return color;
        }

        public static void SetWeaponUnlocked(string weaponKey, bool unlocked)
        {
            if(!Weapons.ContainsKey(weaponKey))
                return;

            if (WeaponUnlocked(weaponKey) == unlocked)
                return;

            Data.Loadout.Data.SetUnlocked(weaponKey, unlocked);
            Data.Loadout.Save();

            if(unlocked && InGameCheck.InLevel())
            {
                HudMessageReceiver.Instance.SendHudMessage($"You have unlocked a new weapon. Press [<color=orange>{InventoryControllerDeployer.inventoryKey.KeyCode}</color>] to open UFG Inventory.");
            }

            InventoryController.RefreshInventory();
            DeployWeapons();
        }

        public static bool WeaponUnlocked(string weaponKey)
        {
            if(Weapons.ContainsKey(weaponKey))
            {
                return Data.Loadout.Data.CheckUnlocked(weaponKey);
            }
            return false;
        }

        [Commands.UFGDebugMethod("UnlockAll", "Unlocks all weapons")]
        public static void UnlockAll()
        {
            foreach(KeyValuePair<string, UFGWeapon> weaponInfo in Weapons)
            {
                SetWeaponUnlocked(weaponInfo.Value.WeaponKey, true);
            }
        }

        [Commands.UFGDebugMethod("ForceDeploy", "Forcefully deploys weapons")]
        public static void ForceDeploy()
        {
            Prep();
            DeployWeapons(false, true);
        }
    }

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

        private void RemoveWeapons()
        {
            for(int i=0; i< customSlots.Count; i++)
            {
                for(int x =0; x < customSlots[i].Count; x++)
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

            if(Input.GetKeyDown(KeyCode.Keypad4))
            {
                if(spawnedRod != null)
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
