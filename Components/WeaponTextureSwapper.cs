using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.IO;
using UltraFunGuns.Util;

namespace UltraFunGuns.Components
{
    //DOA
    public class WeaponTextureSwapper : MonoBehaviour
    {
        [SerializeField] private Renderer[] texturedRenderers;


        string texturePath => Path.Combine(TextureLoader.GetTextureFolder(), "WeaponTextures");

        public string WeaponName = "UNKNOWN_WEAPON";

        private void Start()
        {
            return;
            foreach(Renderer render in texturedRenderers)
            {
                ProcessRenderer(render);
            }
        }

        private void ProcessRenderer(Renderer renderer)
        {
            if (renderer == null)
                return;

            if (renderer.material == null)
                return;

            Shader shader = renderer.material.shader;

            for (int i = 0; i < shader.GetPropertyCount(); i++)
            {
                string propertyName = shader.GetPropertyName(i);
                var propertyType = shader.GetPropertyType(i);

                if (propertyType == UnityEngine.Rendering.ShaderPropertyType.Texture)
                {
                    Texture2D texture = (Texture2D)renderer.material.GetTexture(propertyName);
                    if(texture != null)
                    {
                        if (!texture.isReadable)
                            continue;

                        if (!TryLoadTexture(WeaponName, texture.name, out Texture2D loadedTexture))
                        {
                            DumpTexture(texture);
                        }else
                        {
                            loadedTexture.filterMode = FilterMode.Point;
                            renderer.material.SetTexture(propertyName, loadedTexture);
                        }
                    }
                }
            }
        }

        private void DumpTexture(Texture2D tex)
        {
            TextureLoader.SaveTexture(Path.Combine(texturePath, WeaponName, tex.name + ".png"), tex);
        }

        private bool TryLoadTexture(string weaponName, string textureName, out Texture2D loadedTexture)
        {
            return TextureLoader.TryLoadTexture(Path.Combine(texturePath, weaponName, textureName+".png"), out loadedTexture);
        }
    }
}
