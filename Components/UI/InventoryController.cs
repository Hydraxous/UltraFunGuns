using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;


namespace UltraFunGuns
{
    public class InventoryController : MonoBehaviour
    {
        private OptionsManager om;

        private static int maxSlots = 4; //Hardcoded for now.
        List<InventorySlot> slots = new List<InventorySlot>();
        GunControl gc;
        public InventoryControllerData data;

        private Coroutine waitForUnpause;

        private void Awake()
        {
            om = MonoSingleton<OptionsManager>.Instance;
            gc = MonoSingleton<GunControl>.Instance;
            data = InventoryDataManager.GetInventoryData();
        }

        private void Start()
        {
            CreateNewSlots(data);
            SetSlotKeyDisplays();
        }

        private void CreateNewSlots(InventoryControllerData slotData)
        {
            for (int i = 0; i < maxSlots; i++)
            {
                slots.Add(transform.Find(String.Format("MenuBorder/WeaponSlots/Slot{0}Wrapper", i)).gameObject.AddComponent<InventorySlot>());
                slots[i].Initialize(slotData.data[i].slotData,i,this);
            }
        }

        private void SetSlotKeyDisplays()
        {
            List<Text> slotNameTexts = new List<Text>();
            for(int i = 0; i < slots.Count; i++)
            {
                slotNameTexts.Add(transform.Find(String.Format("MenuBorder/SlotNames/Slot{0}Name", i)).GetComponent<Text>());
                
            }
            slotNameTexts[0].text = slotNameTexts[0].text.Replace("KEY", UltraFunGuns.SLOT_7_KEY.Value.ToString());
            slotNameTexts[1].text = slotNameTexts[1].text.Replace("KEY", UltraFunGuns.SLOT_8_KEY.Value.ToString());
            slotNameTexts[2].text = slotNameTexts[2].text.Replace("KEY", UltraFunGuns.SLOT_9_KEY.Value.ToString());
            slotNameTexts[3].text = slotNameTexts[3].text.Replace("KEY", UltraFunGuns.SLOT_10_KEY.Value.ToString());
        }

        public void ButtonPressed(InventoryNode node, InventorySlot slot, string buttonPressed)
        {
            switch(buttonPressed)
            {
                case "Icon":
                    node.data.weaponEnabled = !node.data.weaponEnabled;
                    node.Refresh();
                    break;
                case "Up":
                    slot.ChangeNodeOrder(node, 1);
                    break;
                case "Down":
                    slot.ChangeNodeOrder(node, -1);
                    break;
                case "Right":
                    slot.RemoveNode(node);
                    int newSlot1 = slot.ID + 1;
                    if(newSlot1 >= slots.Count)
                    {
                        newSlot1 = 0;
                    }else if(newSlot1 < 0)
                    {
                        newSlot1 = Mathf.Clamp(slots.Count, 0, slots.Count - 1);
                    }
                    slots[newSlot1].InsertNode(node);
                    break;
                case "Left":
                    slot.RemoveNode(node);
                    int newSlot2 = slot.ID + 1;
                    if (newSlot2 >= slots.Count)
                    {
                        newSlot2 = 0;
                    }
                    else if (newSlot2 < 0)
                    {
                        newSlot2 = Mathf.Clamp(slots.Count, 0, slots.Count - 1);
                    }
                    slots[newSlot2].InsertNode(node);
                    break;
            }
            SaveInventoryData();
            Refresh();
            if(UltraFunGuns.InLevel())
            {
                RedeployWeapons();
            }
        }

        private void RedeployWeapons()//This will cause the UFGWeaponManager to deploy the weapons again as well as vanilla weapons.
        {
            MonoSingleton<GunSetter>.Instance.ResetWeapons();
        }

        public void Refresh()
        {
            foreach(InventorySlot slot in slots)
            {
                slot.Refresh();
            }
        }

        public void SaveInventoryData()
        {
            data = GetCurrentInventoryData();
            InventoryDataManager.SaveInventoryData(data);
        }

        public InventoryControllerData GetCurrentInventoryData()
        {
            List<InventorySlotData> newNodeArray = new List<InventorySlotData>();
            for(int i = 0; i < slots.Count; i++)
            {
                newNodeArray.Add(slots[i].GetSlotData());
            }
            return new InventoryControllerData(newNodeArray.ToArray());
        }
    }

    [System.Serializable]
    public class InventoryControllerData
    {
        public InventorySlotData[] data;
        public InventoryControllerData(InventorySlotData[] data)
        {
            this.data = data;
        }

        public InventoryControllerData()
        {

        }
    }
}
