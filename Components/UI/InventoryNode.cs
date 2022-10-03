using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace UltraFunGuns
{
    public class InventoryNode : MonoBehaviour
    {
        public RectTransform nodeTransform;
        public InventorySlot slot;
        public InventoryNodeData data;
        public int slotIndexPosition;

        public Sprite weaponIcon;

        public Button b_up, b_down, b_left, b_right, b_icon;

        public Color weaponIcon_Enabled;
        public Color weaponIcon_Disabled;
        public Color nodeBg_Enabled;
        public Color nodeBg_Disabled;

        public void Initialize(InventoryNodeData data, InventorySlot slot, int slotIndex)
        {
            weaponIcon_Enabled = new Color(255f, 255f, 255f, 255f);
            weaponIcon_Disabled = new Color(73f, 73f, 73f, 255f);
            nodeBg_Enabled = new Color(185.0f, 185.0f, 185.0f, 110f);
            nodeBg_Disabled = new Color(49.0f, 49.0f, 49.0f, 110f);
            Debug.Log(data.weaponKey + " NODE INIT!");
            this.slotIndexPosition = slotIndex;
            this.data = data;
            this.slot = slot;
            nodeTransform = GetComponent<RectTransform>();
            HydraLoader.dataRegistry.TryGetValue(this.data.weaponKey + "_weaponIcon", out UnityEngine.Object obj);
            weaponIcon = (Sprite)obj;
            if (weaponIcon == null)
            {
                HydraLoader.dataRegistry.TryGetValue("debug_weaponIcon", out UnityEngine.Object obj2);
                weaponIcon = (Sprite)obj2;
            }

            gameObject.name = this.data.weaponKey + ".Node";
            b_icon = transform.Find("Icon").GetComponent<Button>();
            b_icon.onClick.AddListener(() => ButtonPressed("Icon"));
            b_up = transform.Find("UpButton").GetComponent<Button>();
            b_up.onClick.AddListener(() => ButtonPressed("Up"));
            b_right = transform.Find("RightButton").GetComponent<Button>();
            b_right.onClick.AddListener(() => ButtonPressed("Right"));
            b_down = transform.Find("DownButton").GetComponent<Button>();
            b_down.onClick.AddListener(() => ButtonPressed("Down"));
            b_left = transform.Find("LeftButton").GetComponent<Button>();
            b_left.onClick.AddListener(() => ButtonPressed("Left"));

            Refresh();
        }

        public void RefreshPosition()
        {
            float anchorPercentage = 1.0f / ((float) slot.nodes.Count);
            float anchorMin = (slot.nodes.Count - (slotIndexPosition + 1)) * anchorPercentage;
            float anchorMax = (slot.nodes.Count - slotIndexPosition) * anchorPercentage;

            nodeTransform.anchorMin = new Vector2(0.0f, anchorMin);
            nodeTransform.anchorMax = new Vector2(1.0f, anchorMax);

            nodeTransform.anchoredPosition = Vector2.zero;
        }

        public void ChangeSlot(InventorySlot newSlot, int newSlotIndex)
        {
            this.slot = newSlot;
            this.slotIndexPosition = newSlotIndex;
            Refresh();
        }

        public void Refresh()
        {
            //TODO CHECK HERE FOR COLOR VIS BUG
            b_icon.GetComponent<Image>().sprite = weaponIcon;
            var buttonColors = b_icon.GetComponent<Button>().colors;
            buttonColors.normalColor = GetElementColor();
            gameObject.GetComponent<Image>().color = GetElementColor(false);

            if (slotIndexPosition == 0 && slot.nodes.Count == 1)
            {
                b_up.gameObject.SetActive(false);
                b_down.gameObject.SetActive(false);
            }
            else
            {
                b_up.gameObject.SetActive(true);
                b_down.gameObject.SetActive(true);
            }

            RefreshPosition();
        }

        public void ButtonPressed(string button)
        {
            slot.ButtonPressed(this, button);
        }

        private Color GetElementColor(bool icon = true)
        {
            Color color = Color.white;
            if (icon)
            {
                color = MonoSingleton<ColorBlindSettings>.Instance.variationColors[data.weaponVariation];
                color.a = 255f;

                if(!data.weaponEnabled)
                {
                    color = new Color(73f, 73f, 73f, 255f);
                }
            }
            else
            {
                color = MonoSingleton<ColorBlindSettings>.Instance.variationColors[data.weaponVariation];
                color *= 0.5f;
                color.a = 110.0f;

                if (!data.weaponEnabled)
                {
                    color *= 0.5f;
                    color.a = 110.0f;
                }
            }
            return color;
        }

        public void Disappear()
        {
            Destroy(gameObject);
        }
    }

    [System.Serializable]
    public class InventoryNodeData
    {
        public string weaponKey;
        public bool weaponEnabled;
        public int weaponVariation;

        public InventoryNodeData(string weaponKey, bool enabled = true, int weaponVariation = 0)
        {
            this.weaponKey = weaponKey;
            this.weaponEnabled = enabled;
            this.weaponVariation = weaponVariation;
        }

        public InventoryNodeData()
        {

        }
    }
}
