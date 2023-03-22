using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using BepInEx;
using Newtonsoft.Json;

namespace UltraFunGuns.Datas
{
    public static class DataManager
    {
        public static DataFile<Invdata> InventoryData { get; private set; } = new DataFile<Invdata>(new Invdata(),"testdata.ufg");

        const string FOLDER_NAME = "UFG_Data";

        public static string GetDataPath(params string[] subpath)
        {
            string modDir = Assembly.GetExecutingAssembly().Location;
            modDir = Path.GetDirectoryName(modDir);
            string localPath = Path.Combine(modDir, FOLDER_NAME);

            if (!Directory.Exists(localPath))
            {
                Directory.CreateDirectory(localPath);
            }

            if (subpath.Length > 0)
            {
                string subLocalPath = Path.Combine(subpath);
                localPath = Path.Combine(localPath, subLocalPath);
            }

            return localPath;
        }

        //Save to file.
        public static void SaveData<T>(DataFile<T> data) where T : Validatable
        {
            string serializedData = JsonConvert.SerializeObject(data.Data, data.Formatting);
            string dataFilePath = GetDataPath(data.FileName);
            File.WriteAllText(dataFilePath, serializedData);
            HydraLogger.Log($"{data.FileName} Data saved.");
        }

        //Load from file.
        public static T LoadData<T>(DataFile<T> data) where T : Validatable
        {
            T dataObject = null;

            if(data == null)
            {
                HydraLogger.Log("FATAL ERROR. A persistent data variable was attempted to be loaded by the data manager, but it was never initialized and therefore has no fallback.", DebugChannel.Fatal);
                return null;
            }

            string dataFilePath = GetDataPath(data.FileName);

            if (File.Exists(dataFilePath))
            {
                string jsonData;
                using (StreamReader reader = new StreamReader(dataFilePath))
                {
                    jsonData = reader.ReadToEnd();
                }

                dataObject = JsonConvert.DeserializeObject<T>(jsonData);
                try
                {
                    if (dataObject.Validate())
                        return dataObject;
                }
                catch (Exception ex)
                {
                    //HydraLogger.Log($"Data object invalid!\n{ex.Message}\n{ex.StackTrace}", DebugChannel.Fatal);
                }

            }

            dataObject = default(T);
            //HydraLogger.Log($"Loaded: {dataObject.FileName()}");
            return dataObject;
        }
    }

    public class DataFile<T> where T : Validatable
    {
        private bool autoSave = true;
        public string FileName { get; }

        public Formatting Formatting { get; }
        private T fallbackData;
        private T data;
        public T Data
        {
            get
            {
                if(data == null)
                {
                    Load();
                }
                return data;
            }

            set
            {
                data = value;
                if(autoSave)
                {
                    Save();
                }
            }
        }

        public DataFile(T fallback, string filename, Formatting jsonFormatting = Formatting.None)
        {
            FileName = filename;
            Formatting = jsonFormatting;
            fallbackData = fallback;
        }

        public void SetAutoSave(bool enabled)
        {
            autoSave = enabled;
            if(autoSave)
            {
                Save();
            }
        }
            
        public void Save()
        {
            DataManager.SaveData<T>(this);
        }

        public void Load()
        {
            if(FileName.IsNullOrWhiteSpace())
            {
                HydraLogger.Log($"Data object was incorrectly set up. It is null and cannot be loaded. Please check this Hydra.", DebugChannel.Fatal);
                New();
                return;
            }

            data = DataManager.LoadData(this);

            if(data == null)
            {
                New();
            }
        }

        public void New()
        {
            if(fallbackData == null)
            {
                HydraLogger.Log($"Data object was incorrectly set up. It is null and cannot be loaded. Please check this Hydra.", DebugChannel.Fatal);
                data = default(T);
                return;
            }

            data = fallbackData;
            Save();
        }

        public bool Validate()
        {
            return Data.Validate();
        }
    }

    [System.Serializable]
    public class Invdata : Validatable
    {
        public string text;
        public int number;

        public Invdata(string text = "lol", int number = 2)
        {
            this.text = text;
            this.number = number;
        }

        public override bool Validate()
        {
            return true;
        }
    }

    public abstract class Validatable
    {
        public abstract bool Validate();
    }
}
