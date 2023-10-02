using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    public static class VoxelMaterialManager
    {
        private static Dictionary<Texture2D, Material> materials = new Dictionary<Texture2D, Material>();
        private static Dictionary<Texture2D, Sprite> sprites = new Dictionary<Texture2D, Sprite>();


        //SO JANK AAAAAAAAA
        [UFGAsset("voxelHandMaterial")]
        private static Material voxelMaterial;
        private static Shader voxelShader => voxelMaterial.shader;

        

        public static Material GetMaterial(Texture2D texture)
        {
            if(materials.ContainsKey(texture))
                return materials[texture];

            Material newMaterial = new Material(voxelShader);
            newMaterial.mainTexture = texture;
            materials.Add(texture, newMaterial);

            return newMaterial;
        }

        [UFGAsset("cobblestone")]
        private static Texture2D defaultTexture;

        public static Material GetDefaultMaterial()
        {
            return GetMaterial(defaultTexture);
        }


        public static Sprite GetSprite(Texture2D texture)
        {
            if (!sprites.ContainsKey(texture))
            {
                Sprite newSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                sprites.Add(texture, newSprite);
            }
            
            return sprites[texture];
        }

    }
}
