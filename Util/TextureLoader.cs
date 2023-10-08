using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using HydraDynamics;

namespace UltraFunGuns.Util
{
    public static class TextureLoader
    {
        public static string GetTextureFolder()
        {
            return HydraDynamics.DataPersistence.DataManager.GetDataPath("tex");
        }

        private static Texture2D[] cachedTextures = new Texture2D[0];

        private static bool initialized = false;

        public static void Init()
        {
            if (!initialized)
            {
                InGameCheck.OnLevelChanged += OnLevelChanged;
                initialized = true;
            }
        }

        private static void OnLevelChanged(InGameCheck.UKLevelType ltype)
        {
            if (InGameCheck.InLevel())
            {
                RefreshTextures();
            }
        }

        public static void RefreshTextures()
        {
            CleanCachedTextures();
            cachedTextures = FindTextures();
        }

        public static void SaveTexture(string path, Texture2D texture)
        {
            if (texture == null)
                return;

            byte[] bytes = texture.EncodeToPNG();
            if (!Directory.Exists(Path.GetDirectoryName(path)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }
            File.WriteAllBytes(path, bytes);
        }

        public static bool TryLoadTexture(string path, out Texture2D tex, bool checkerIfNull = false)
        {
            tex = null;
            if (!File.Exists(path))
            {
                HydraLogger.Log("Invalid location: " + path);
                return false;
            }

            byte[] byteArray = null;
            try
            {
                byteArray = File.ReadAllBytes(path);
            }
            catch (System.Exception e)
            {
                HydraLogger.Log("Invalid path: " + path);
            }

            tex = new Texture2D(16, 16);

            if (!tex.LoadImage(byteArray))
            {
                HydraLogger.Log("texture loading failed!");
                if (checkerIfNull)
                {
                    Checker(ref tex);
                }
                return false;
            }

            return true;
        }

        public static Texture2D PullRandomTexture()
        {
            if (cachedTextures.Length > 0)
            {
                int rand = UnityEngine.Random.Range(0, cachedTextures.Length);
                return cachedTextures[rand];
            }

            return null;
        }

        private static List<Texture2D> additionalTextures = new List<Texture2D>();

        public static void AddTextureToCache(Texture2D texture)
        {
            List<Texture2D> oldCache = new List<Texture2D>(cachedTextures);
            oldCache.Add(texture);
            additionalTextures.Add(texture);
            cachedTextures = oldCache.ToArray();
        }


        private static void CleanCachedTextures()
        {
            if (cachedTextures != null)
            {
                int len = cachedTextures.Length;
                for (int i = 0; i < len; i++)
                {
                    if (cachedTextures[i] != null)
                    {
                        if (!additionalTextures.Contains(cachedTextures[i]))
                        {
                            UnityEngine.Object.Destroy(cachedTextures[i]);
                        }
                    }
                }

                cachedTextures = null;
            }
        }

        private static Texture2D[] FindTextures()
        {

            List<Texture2D> newTextures = new List<Texture2D>();

            string path = GetTextureFolder();
            string[] pngs = System.IO.Directory.GetFiles(path, "*.png", SearchOption.AllDirectories);
            string[] jpgs = System.IO.Directory.GetFiles(path, "*.jpg", SearchOption.AllDirectories);

            for (int i = 0; i < pngs.Length; i++)
            {
                if (TryLoadTexture(ImagePath(pngs[i]), out Texture2D newTex, false))
                {
                    newTextures.Add(newTex);
                }
            }

            for (int i = 0; i < jpgs.Length; i++)
            {
                if (TryLoadTexture(ImagePath(jpgs[i]), out Texture2D newTex, false))
                {
                    newTextures.Add(newTex);
                }
            }

            string ImagePath(string filename)
            {
                string imagePath = GetTextureFolder();
                imagePath = Path.Combine(path, filename);
                return imagePath;
            }

            for (int i = 0; i < additionalTextures.Count; i++)
            {
                newTextures.Add(additionalTextures[i]);
            }

            HydraLogger.Log($"Found {newTextures.Count} textures");

            return newTextures.ToArray();
        }

        private static void Checker(ref Texture2D tex)
        {
            for (int y = 0; y < tex.height; y++)
            {
                for (int x = 0; x < tex.width; x++)
                {
                    bool Xeven = ((x % 2) == 0);
                    bool Yeven = ((y % 2) == 0);
                    if (Yeven != Xeven)
                    {
                        Xeven = !Xeven;
                    }
                    Color col = (Xeven) ? Color.white : Color.black;
                    tex.SetPixel(x, y, col);
                }
            }

            tex.Apply();
        }
    }
}
