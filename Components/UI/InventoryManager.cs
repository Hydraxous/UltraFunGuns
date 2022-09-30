using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using UnityEngine.Serialization;
using System.IO;

namespace UltraFunGuns
{
    public static class InventoryManager
    {

        public static InventoryLoadout currentLoadout;

        public static bool Initialized = false;

        private static string loadoutDataPath;

        public static void Initialize()
        {
            string loadoutDataPath = (Assembly.GetExecutingAssembly().Location + "loadoutData.json"); //TODO check for problems
            LoadLoadout();
            Initialized = true;
        }

        public static void LoadLoadout()
        {
            string loadoutData = null;

            if (!File.Exists(loadoutDataPath))
            {
                CreateNewLoadout();
            }

            using (StreamReader reader = new StreamReader(loadoutDataPath))
            {
                loadoutData = reader.ReadToEnd();
            }

            try
            {
                currentLoadout = UnityEngine.JsonUtility.FromJson<InventoryLoadout>(loadoutData);
            }catch (System.Exception e)
            {
                CreateNewLoadout();
            }
        }

        public static void CreateNewLoadout()
        {
            if(currentLoadout != null)
            {
                InventoryLoadout oldLoadout = currentLoadout;
            }
            

            //TODO MAKE THE LOADOUT HERE! :D
        }

        public static void SaveLoadout()
        {
            if (!File.Exists(loadoutDataPath))
            {
                CreateNewLoadout();
            }

            string loadoutData = UnityEngine.JsonUtility.ToJson(currentLoadout);
            File.WriteAllText(loadoutDataPath, loadoutData);
        }
    }

    [System.Serializable]
    public class InventoryLoadout
    {
        List<InventoryItem> loadout = new List<InventoryItem>();
        public bool setOnStart;

        class InventoryItem
        {
            string weaponName;
            int slot;
            int orderInSlot;
        }
    }
}
