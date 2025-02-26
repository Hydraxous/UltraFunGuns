using Newtonsoft.Json;
using System;

namespace UltraFunGuns
{
    [Serializable]
    /// <summary>
    /// DataFile generic. Use this with a Validatable class to serialize it.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DataFile<T> where T : Validatable
    {

        private bool autoSave = true;

        /// <summary>
        /// Filename with extension
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// Json Formatting type
        /// </summary>
        public Formatting Formatting { get; }

        private T fallbackData;

        /// <summary>
        /// This is data that is stored on the object.
        /// </summary>
        private T data;


        /// <summary>
        /// Returns the data, loading if absent. Saves the data on set.
        /// </summary>
        public T Data
        {
            get
            {
                if (data == null)
                {
                    Load();
                }
                return data;
            }

            set
            {
                data = value;
                if (autoSave)
                {
                    Save();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataManager">The DataManager that will manage this file. This is needed for disk IO.</param>
        /// <param name="fallback">The fallback or default data in the event the data is not able to be loaded.</param>
        /// <param name="filename">filename with extension.</param>
        /// <param name="jsonFormatting">Json formatting mode</param>
        public DataFile(T fallback, string filename, Formatting jsonFormatting = Formatting.None)
        {
            FileName = filename;
            Formatting = jsonFormatting;
            fallbackData = fallback;
            Load();
        }


        /// <summary>
        /// Sets the autosave mode of this file.
        /// </summary>
        /// <param name="enabled">Enable autosave</param>
        public void SetAutoSave(bool enabled)
        {
            autoSave = enabled;
            if (autoSave)
            {
                Save();
            }
        }

        /// <summary>
        /// Saves the file to disk. This is usually done automatically.
        /// </summary>
        public void Save()
        {
            DataManager.SaveData<T>(this);
        }


        /// <summary>
        /// Loads the file from disk. This is usually done automatically.
        /// </summary>
        public void Load()
        {
            if (FileName == null)
            {
                //UltraFunGuns.Log.LogError($"Data object was incorrectly set up. It is null and cannot be loaded. Please check this Hydra.");
                New();
                return;
            }

            data = DataManager.LoadData(this);

            if (data == null)
            {
                New();
            }
        }


        /// <summary>
        /// Resets the data to the default or fallback provided.
        /// </summary>
        public void New()
        {
            if (fallbackData == null)
            {
                //UltraFunGuns.Log.LogError($"Data object was incorrectly set up. It is null and cannot be loaded. Please check this Hydra.");
                data = default(T);
                return;
            }

            data = fallbackData;
            Save();
        }

        /// <summary>
        /// Validates the data within the file.
        /// </summary>
        /// <returns></returns>
        public bool Validate()
        {
            return Data.Validate();
        }
    }

    /// <summary>
    /// This class should be inherited to store persistent data.
    /// </summary>
    public abstract class Validatable
    {
        public abstract bool Validate();

        /// <summary>
        /// Controls if a mod that is not the mod that defines the datafile can read the data.
        /// </summary>
        ///
        public abstract bool AllowExternalRead { get; }
    }
}