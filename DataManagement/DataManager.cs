using Newtonsoft.Json;
using System;
using System.IO;
using UnityEngine;

namespace UltraFunGuns
{
    public static class DataManager
    {

        /// <summary>
        /// Saves a Datafile to disk
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        internal static void SaveData<T>(DataFile<T> data) where T : Validatable
        {
            if (!data.Data.Validate())
            {
                Debug.LogError($"Invalid data!! {data.FileName}");
                return;
            }

            string serializedData = JsonConvert.SerializeObject(data.Data, data.Formatting);
            string dataFilePath = Path.Combine(Paths.DataFolder, data.FileName);
            File.WriteAllText(dataFilePath, serializedData);
        }

        /// <summary>
        /// Loads a Datafile from disk
        /// </summary>
        /// <typeparam name="T">Validatable class</typeparam>
        /// <param name="data">DataFile object it will load.</param>
        /// <returns></returns>
        internal static T LoadData<T>(DataFile<T> data) where T : Validatable
        {
            T dataObject = null;

            if (data == null)
            {
                Debug.LogError("UFG: Data file was incorrectly setup. This is a result of an initialization error.");
                return null;
            }

            string dataFilePath = Path.Combine(Paths.DataFolder, data.FileName);

            if (File.Exists(dataFilePath))
            {
                string jsonData;
                using (StreamReader reader = new StreamReader(dataFilePath))
                {
                    jsonData = reader.ReadToEnd();
                }

                try
                {
                    dataObject = JsonConvert.DeserializeObject<T>(jsonData);
                    if (dataObject.Validate())
                        return dataObject;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"UFG: Data file {data.FileName} could not be validated or deserialized.");
                }

            }

            data.New();
            dataObject = data.Data;
            return dataObject;
        }



    }
}
