using System;
using UnityEngine;
using UnityEngine.UI;

namespace UltraFunGuns
{
    public class VoxelMenuButton : MonoBehaviour
    {
        [SerializeField] private Image[] blockTexImages;
        [SerializeField] private Button button;
        [SerializeField] private GameObject selector;

        private VoxelData data;
        public void SetVoxelData(VoxelData data)
        {
            this.data = data;

            Sprite sprite = VoxelMaterialManager.GetSprite((Texture2D)data.Material.mainTexture);

            for (int i =0;i<blockTexImages.Length;i++)
            {
                blockTexImages[i].sprite = sprite;
            }
        }

        public void SetSelected(bool selected)
        {
            selector.SetActive(selected);
        }

        public void SetButtonAction(Action<VoxelMenuButton, VoxelData> onClick)
        {
            button.onClick.AddListener(() => { onClick?.Invoke(this, data); });
        }
    }

}
