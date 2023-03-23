using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;
using BepInEx;
using Newtonsoft.Json;
using UnityEngine;

namespace UltraFunGuns.Datas
{

    public static class DataManager
    {

        private const string FOLDER_SUFFIX = "_Data";

        public static string GetDataPath(params string[] subpath)
        {
            Assembly currentAssembly = Assembly.GetExecutingAssembly();
            string modDir = currentAssembly.Location;
            string asmName = currentAssembly.GetName().Name;
            modDir = Path.GetDirectoryName(modDir);
            string localPath = Path.Combine(modDir, asmName+FOLDER_SUFFIX);

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

        public static void SaveAll()
        {
            
        }

        //Save to file.
        public static void SaveData<T>(DataFile<T> data) where T : Validatable
        {
            string serializedData = JsonConvert.SerializeObject(data.Data, data.Formatting);
            string dataFilePath = GetDataPath(data.FileName);
            File.WriteAllText(dataFilePath, serializedData);
            HydraLogger.Log($"{data.FileName} Data saved.");
            Debug.LogError("HDL: Data file was incorrectly setup. This is a result of an initialization error.");
        }

        //Load from file.
        public static T LoadData<T>(DataFile<T> data) where T : Validatable
        {
            T dataObject = null;

            if(data == null)
            {
                Debug.LogError("HydraLib: Data file was incorrectly setup. This is a result of an initialization error.");
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
}
