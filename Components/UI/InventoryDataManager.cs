using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;
using Newtonsoft.Json;

namespace UltraFunGuns
{
    public static class InventoryDataManager
    {

        private static InventoryControllerData inventory;

        public static bool Initialized = false;

        private static string loadoutDataPath;

        public static bool Initialize()
        {
            try
            {
                loadoutDataPath = (Assembly.GetExecutingAssembly().Location.Replace("UltraFunGuns.dll", "loudoutData.json")); //TODO check for problems
                if(!LoadInventoryData())
                {
                    return false;
                }
                Initialized = true;
                return true;
            }catch(Exception e)
            {
                return false;
            }      
        }

        public static InventoryControllerData GetInventoryData()
        {

            if (inventory == null)
            {
                if (LoadInventoryData())
                {
                    return inventory;
                }
            }
            else
            {
                return inventory;
            }
            return null;
        }

        public static bool LoadInventoryData()
        {
            string loadoutData = null;

            if (!File.Exists(loadoutDataPath))
            {
                FirstTimeLoad();
            }

            try
            {
                using (StreamReader reader = new StreamReader(loadoutDataPath))
                {
                    loadoutData = reader.ReadToEnd();
                }
                inventory = JsonConvert.DeserializeObject<InventoryControllerData>(loadoutData);
                return true;
            }catch (System.Exception e)
            {
                File.Delete(loadoutDataPath);
                return LoadInventoryData();
            }
        }

        public static void SaveInventoryData(InventoryControllerData data)
        {
            inventory = data;
            string loadoutData = JsonConvert.SerializeObject(inventory);
            Console.WriteLine("GOOOOOVER: " + loadoutData);
            File.WriteAllText(loadoutDataPath, loadoutData);
        }

        //REGISTRY: Default loadout ALL GUNS HERE.
        private static void FirstTimeLoad()
        {

            List<InventoryNodeData> slot1 = new List<InventoryNodeData>();
            List<InventoryNodeData> slot2 = new List<InventoryNodeData>();
            List<InventoryNodeData> slot3 = new List<InventoryNodeData>();
            List<InventoryNodeData> slot4 = new List<InventoryNodeData>();

        
            slot1.Add(new InventoryNodeData("SonicReverberator", true, 0));
            slot2.Add(new InventoryNodeData("Dodgeball", true, 2));
            slot2.Add(new InventoryNodeData("EggToss", true, 3));
            slot3.Add(new InventoryNodeData("Focalyzer", true, 2));
            slot3.Add(new InventoryNodeData("FocalyzerAlternate", true, 0));
            slot4.Add(new InventoryNodeData("Tricksniper", true, 2));
            slot4.Add(new InventoryNodeData("Bulletstorm", true, 0));
    
            List<InventorySlotData> newSlotDatas = new List<InventorySlotData> { new InventorySlotData(slot1.ToArray()), new InventorySlotData(slot2.ToArray()), new InventorySlotData(slot3.ToArray()), new InventorySlotData(slot4.ToArray()) };
            InventoryControllerData defaultData = new InventoryControllerData(newSlotDatas.ToArray());
            SaveInventoryData(defaultData);
        }
    }
}
