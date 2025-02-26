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

            deployer.DeployWeapons();
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
            NewStyleItem("vaporized", CustomStyleDefinitions.VAPORIZED);
            NewStyleItem("vibecheck", CustomStyleDefinitions.VIBE_CHECK);
            NewStyleItem("v2kill", CustomStyleDefinitions.V2_KILL);
            NewStyleItem("gabrielkill", CustomStyleDefinitions.GABRIEL_KILL);
            NewStyleItem("wickedkill", CustomStyleDefinitions.WICKED_KILL);
            NewStyleItem("minoskill", CustomStyleDefinitions.MINOS_KILL);
            NewStyleItem("orbited", CustomStyleDefinitions.ORBITED);
            NewStyleItem("egged", CustomStyleDefinitions.EGGED);
            NewStyleItem("eggstrike", CustomStyleDefinitions.EGG_STRIKE);
            NewStyleItem("eggsplosion", CustomStyleDefinitions.EGG_SPLOSION);
            NewStyleItem("refractionhit", CustomStyleDefinitions.LASER_REFRACTION_HIT);
            NewStyleItem("discohit", CustomStyleDefinitions.LASER_DISCO_HIT);
            NewStyleItem("dodgeballhit", CustomStyleDefinitions.DODGEBALL_HIT);
            NewStyleItem("dodgeballparry", CustomStyleDefinitions.DODGEBALL_PARRY);
            NewStyleItem("dodgeballparryhit", CustomStyleDefinitions.DODGEBALL_PARRY_HIT);
            NewStyleItem("dodgeballreversehit", CustomStyleDefinitions.DODGEBALL_REVERSE_HIT);
            NewStyleItem("fingergunhit", CustomStyleDefinitions.FINGER_GUN_HIT);
            NewStyleItem("fingergunfullpenetrate", CustomStyleDefinitions.FINGER_GUN_PENETRATE);
            NewStyleItem("fingergunprojhit", CustomStyleDefinitions.FINGER_GUN_PROJECTILE_HIT);
            NewStyleItem("admingunkill", CustomStyleDefinitions.ADMIN_GUN_KILL);
            NewStyleItem("brickmindkill", CustomStyleDefinitions.BRICK_MIND_KILL);
            NewStyleItem("brickparrykill", CustomStyleDefinitions.BRICK_PARRY_KILL);
            NewStyleItem("tricksniperquickscope", CustomStyleDefinitions.TRICKSNIPER_QUICKSCOPE);
            NewStyleItem("tricksniper360", CustomStyleDefinitions.TRICKSNIPER_360);
            NewStyleItem("tricksnipernoscope", CustomStyleDefinitions.TRICKSNIPER_NOSCOPE);
            NewStyleItem("tricksniperbankshot", CustomStyleDefinitions.TRICKSNIPER_BANKSHOT);
            NewStyleItem("ultragunsuperchargekill", CustomStyleDefinitions.ULTRAGUN_SUPERCHARGE_KILL);
            NewStyleItem("ultragunkill", CustomStyleDefinitions.ULTRAGUN_KILL);
            NewStyleItem("ultragunrainkill", CustomStyleDefinitions.ULTRAGUN_RAIN_KILL);
            NewStyleItem("ultragunaerialkill", CustomStyleDefinitions.ULTRAGUN_AERIAL_KILL);

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
                UltraFunGuns.Log.LogError($"Weapon info null when requested for type {t.ToString()}");
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

                UltraFunGuns.Log.Log($"Found weapon: {attribute.DisplayName}");
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
                    UltraFunGuns.Log.Log($"Found weapon: {infos[i].DisplayName}");
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
