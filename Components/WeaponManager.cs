using HydraDynamics.Events;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace UltraFunGuns
{
    public static class WeaponManager
    {
        public const int SLOTS = 4, SLOT_OFFSET = 7;

        public delegate void OnWeaponsDeployedHandler(UFGWeapon[] weapons);
        public static OnWeaponsDeployedHandler OnWeaponsDeployed;

        public static void Init()
        {
            RegisterWeapons();
            InGameCheck.OnLevelChanged += OnLevelChanged;
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
            if(!InGameCheck.InLevel() && !force && !inventoryMade && !Data.Config.Data.EnableWeaponsInAllScenes)
            {
                return;
            }
  
            if (deployer == null)
            {
                if (!GunControl.Instance.TryGetComponent<WeaponDeployer>(out deployer))
                {
                    deployer = GunControl.Instance.gameObject.AddComponent<WeaponDeployer>();
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

            InitStyleItems();

            CanvasController canvas = CanvasController.Instance;
            if (!canvas.TryGetComponent<InventoryControllerDeployer>(out inventoryDeployer))
            {
                inventoryDeployer = canvas.gameObject.AddComponent<InventoryControllerDeployer>();
            }

            inventoryMade = true;
        }

        private static void InitStyleItems()
        {
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
                throw new System.Exception($"Type {t} does not have UFGWeapon attribute.");
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

                if (attribute.WeaponKey == "NULL")//???????
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
                HudMessageReceiver.Instance.SendHudMessage($"You have unlocked a new weapon. Press [<color=orange>{InventoryControllerDeployer.inventoryKey.Value}</color>] to open UFG Inventory.");
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

        public static void RemoveAllWeapons()
        {
            if (!InGameCheck.InLevel() && !inventoryMade)
            {
                return;
            }

            if (deployer == null)
                return;

            GunControl gc = GunControl.Instance;
            if (gc.TryGetComponent<WeaponDeployer>(out WeaponDeployer ultraFGPatch))
            {
                deployer = ultraFGPatch;
            }
            else
            {
                deployer = gc.gameObject.AddComponent<WeaponDeployer>();
            }

            deployer.DisposeWeapons();
        }
    }
}
