using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using UltraFunGuns.Datas;

namespace UltraFunGuns
{
    public class InventoryNode : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public RectTransform nodeTransform;
        public InventorySlot slot;
        public InventoryNodeData data;
        private InventoryController controller;

        public WeaponInfoCard card;

        private UFGWeapon nodeInfo;

        public int slotIndexPosition;

        public Sprite weaponIcon;

        public Button b_up, b_down, b_left, b_right, b_icon;

        public Color weaponIcon_Enabled;
        public Color weaponIcon_Disabled;
        public Color nodeBg_Enabled;
        public Color nodeBg_Disabled;

        public void Initialize(InventoryNodeData data, InventorySlot slot, int slotIndex)
        {
            controller = GetComponentInParent<InventoryController>();

            weaponIcon_Enabled = new Color(255f, 255f, 255f, 255f);
            weaponIcon_Disabled = new Color(73f, 73f, 73f, 255f);
            nodeBg_Enabled = new Color(185.0f, 185.0f, 185.0f, 110f);
            nodeBg_Disabled = new Color(49.0f, 49.0f, 49.0f, 110f);
            HydraLogger.Log(data.weaponKey + " NODE INIT!");
            this.slotIndexPosition = slotIndex;
            this.data = data;
            this.slot = slot;
            if(!WeaponManager.Weapons.TryGetValue(data.weaponKey, out nodeInfo))
            {
                HydraLogger.Log($"Inventory node could not get weapon info from key {data.weaponKey}");
            }
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

        //The chad non-layout group function
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
            buttonColors.selectedColor = GetElementColor();
            b_icon.GetComponent<Button>().colors = buttonColors;
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
                color = MonoSingleton<ColorBlindSettings>.Instance.variationColors[(int)nodeInfo.IconColor];

                if (!data.weaponEnabled)
                {
                    color *= 0.5f;
                }
                color.a = 255f;
            }
            else
            {
                color = MonoSingleton<ColorBlindSettings>.Instance.variationColors[(int)nodeInfo.IconColor];
                color *= 0.5f;
               
                if (!data.weaponEnabled)
                {
                    color *= 0.4f;
                }
                color.a = 110.0f;
            }
            return color;
        }

        public void Disappear()
        {
            Destroy(gameObject);
        }

        public override string ToString()
        {
            string check = (data.weaponEnabled) ? "(x)" : "( )";
            return String.Format("{2} [{0}] {1}", slot.ID, data.weaponKey, check);
        }

        private bool mousedOver = false;

        public void OnPointerEnter(PointerEventData eventData)
        {
            if(!mousedOver)
            {
                mousedOver = true;
                controller.SetCardWeaponInfo(nodeInfo);
                StartCoroutine(MouseHeldSequence());
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            mousedOver = false;
            StopAllCoroutines();
            if (controller != null)
            {
                controller.SetCardActive(false);
            }
        }

        IEnumerator MouseHeldSequence()
        {
            yield return new WaitForSecondsRealtime(Data.Config.Data.MouseOverNodeTime);

            while (mousedOver)
            {
                if(controller != null)
                {
                    controller.SetCardActive(true);
                }

                yield return new WaitForEndOfFrame();
            } 
        }

        private void OnDisable()
        {
            mousedOver = false;
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
