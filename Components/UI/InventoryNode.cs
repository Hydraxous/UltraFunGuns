using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace UltraFunGuns
{
    public class InventoryNode : MonoBehaviour
    {
        public InventorySlot slot;
        public InventoryNodeData data;
        public int slotIndexPosition;

        public Sprite weaponIcon;

        public Button b_up, b_down, b_left, b_right, b_icon;

        private Color weaponIcon_Enabled = new Color(255f,255f,255f,255f);
        private Color weaponIcon_Disabled = new Color(73f, 73f, 73f, 255f);
        private Color nodeBg_Enabled = new Color(185.0f, 185.0f, 185.0f, 110f);
        private Color nodeBg_Disabled = new Color(49.0f, 49.0f, 49.0f, 110f);

        public void Initialize(InventoryNodeData data, InventorySlot slot, int slotIndex)
        {
            this.slotIndexPosition = slotIndex;
            this.data = data;
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
            RefreshPosition();
        }

        public void RefreshPosition()
        {
            float anchorPercentage = 1 / slot.nodes.Count;
            float anchorMax = (slot.nodes.Count - slotIndexPosition) * anchorPercentage;
            float anchorMin = (slot.nodes.Count - slotIndexPosition + 1) * anchorPercentage;

            RectTransform nodeTransform = GetComponent<RectTransform>();

            nodeTransform.anchorMin = new Vector2(0.0f, anchorMin);
            nodeTransform.anchorMax = new Vector2(1.0f, anchorMax);
            nodeTransform.anchoredPosition = Vector2.zero;
        }

        public void ChangeSlot(InventorySlot newSlot, int newSlotIndex)
        {
            this.slot = newSlot;
            this.slotIndexPosition = newSlotIndex;
            RefreshPosition();
        }

        public void Refresh()
        {
            b_icon.GetComponent<Image>().sprite = weaponIcon;
            if(data.weaponEnabled)
            {
                GetComponent<Image>().color = nodeBg_Enabled;
                b_icon.GetComponent<Image>().color = weaponIcon_Enabled;
            }else
            {
                GetComponent<Image>().color = nodeBg_Disabled;
                b_icon.GetComponent<Image>().color = weaponIcon_Disabled;
            }
        }

        public void ButtonPressed(string button)
        {
            slot.ButtonPressed(this, button);
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

        public InventoryNodeData(string weaponKey, bool enabled = true)
        {
            this.weaponKey = weaponKey;
            this.weaponEnabled = enabled;
        }

        public InventoryNodeData()
        {

        }
    }
}
