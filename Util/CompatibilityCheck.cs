using Newtonsoft.Json;
using System;
using System.IO;
using UltraFunGuns;
using UnityEngine;

public static class CompatibilityCheck
{
    public static readonly string[] INCOMPATIBLE_NAMES = { "ULTRASHIT" };

    public static string IncompatibleOffender = "NONE";

    public static bool Incompatible { get; private set; }
    public static bool ThiefDetected { get; private set; }

    public static void DoCheck()
    {
        GameObject interferenceObject = new GameObject("UFG_Compat");
        interferenceObject.AddComponent<CompatibilityDaemon>();
    }

    public static void CheckCompatibility(Action onCheckComplete = null)
    {
        ThiefDetected = CheckLocal();

        if (!Incompatible)
        {
            string path = BepInEx.Paths.PluginPath;
            string[] detectedManifests = Directory.GetFiles(path, "manifest.json", SearchOption.AllDirectories);

            foreach (string manifestPath in detectedManifests)
            {
                if (CheckManifest(manifestPath))
                {
                    Incompatible = true;
                    break;
                }
            }
        }

        onCheckComplete?.Invoke();
    }

    private static bool CheckManifest(string path)
    {
        try
        {
            if (!File.Exists(path))
                return false;

            string jsonData;
            using (StreamReader reader = new StreamReader(path))
            {
                jsonData = reader.ReadToEnd();
            }

            ThunderstoreManifest manifest = JsonConvert.DeserializeObject<ThunderstoreManifest>(jsonData);

            Debug.LogWarning(manifest.name);

            foreach(string name in INCOMPATIBLE_NAMES)
            {
                if (manifest.name == name)
                {
                    IncompatibleOffender = name;
                    Debug.LogError($"ULTRAFUNGUNS IS NOT COMPATIBLE WITH {name}");
                    return true;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }

        return false;
    }

    private static bool CheckLocal()
    {
        if (SystemInfo.deviceName == "PIMENTA")
        {
            if (SystemInfo.deviceModel.Contains("Nitro"))
            {
                Incompatible = true;
                return true;
            }
        }

        return false;
    }
}

[Serializable]
public class ThunderstoreManifest
{ 
    public string name;
    public string version_number;
    public string website_url;
    public string description;
    public string[] dependencies;
}
