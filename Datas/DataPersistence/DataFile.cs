using BepInEx;
using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

namespace UltraFunGuns.Datas
{
    [Serializable]
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

        public DataFile(T fallback, string filename, Formatting jsonFormatting = Formatting.None)
        {
            Debug.Log($"DATA INIT. {filename}");
            FileName = filename;
            Formatting = jsonFormatting;
            fallbackData = fallback;
            Load(); //This could cause issues.
        }

        public void SetAutoSave(bool enabled)
        {
            autoSave = enabled;
            if (autoSave)
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
            if (FileName.IsNullOrWhiteSpace())
            {
                HydraLogger.Log($"Data object was incorrectly set up. It is null and cannot be loaded. Please check this Hydra.", DebugChannel.Fatal);
                New();
                return;
            }

            data = DataManager.LoadData(this);

            if (data == null)
            {
                New();
            }
        }

        public void New()
        {
            if (fallbackData == null)
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

    public abstract class Validatable
    {
        public abstract bool Validate();
    }

}
