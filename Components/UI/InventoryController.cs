﻿using System;
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
        private List<InventorySlot> slots = new List<InventorySlot>();
        private List<Text> slotKeyNames = new List<Text>();
        GunControl gc;
        public InventoryControllerData data;

        private void Awake()
        {
            om = MonoSingleton<OptionsManager>.Instance;
            gc = MonoSingleton<GunControl>.Instance;
            data = InventoryDataManager.GetInventoryData();
        }

        private void Start()
        {
            CreateNewSlots();
            SetSlotKeyDisplays();
        }

        private void CreateNewSlots()
        {
            for (int i = 0; i < maxSlots; i++)
            {
                InventorySlot newSlot = transform.Find(String.Format("MenuBorder/WeaponSlots/Slot{0}Wrapper", i)).gameObject.AddComponent<InventorySlot>();
                slots.Add(newSlot);
            }

            for(int j = 0; j < slots.Count; j++)
            {
                slots[j].Initialize(data.slots[j], j, this);
            }
        }

        private void SetSlotKeyDisplays()
        {
            List<Text> slotNameTexts = new List<Text>();
            for(int i = 0; i < slots.Count; i++)
            {
                slotKeyNames.Add(transform.Find(String.Format("MenuBorder/SlotNames/Slot{0}Name", i)).GetComponent<Text>());
            }

            RefreshSlotKeyDisplays();
        }

        public void RefreshSlotKeyDisplays()
        {
            if(slotKeyNames.Count > 0)
            {
                for (int i=0; i<slotKeyNames.Count; i++)
                {
                    slotKeyNames[i].text = string.Format("Slot {0} [<color=orange>{1}</color>]", UFGWeaponManager.SlotOffset+i, UFGWeaponManager.UFGSlotKeys[i].keyBind.ToString());
                }
            }
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
                    slot.ChangeNodeOrder(node, -1);
                    break;
                case "Down":
                    slot.ChangeNodeOrder(node, 1);
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
                    int newSlot2 = slot.ID - 1;
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
            Debug.Log("UFG: Inventory saved.");
        }

        public InventoryControllerData GetCurrentInventoryData()
        {
            List<InventorySlotData> newNodeArray = new List<InventorySlotData>();
            for(int i = 0; i < slots.Count; i++)
            {
                newNodeArray.Add(slots[i].GetSlotData());
            }
            return new InventoryControllerData(newNodeArray.ToArray(), data.modVersion, data.firstTimeUsingInventory, data.firstTimeModLoaded);
        }
    }

    [System.Serializable]
    public class InventoryControllerData
    {
        public string modVersion;
        public bool firstTimeModLoaded;
        public bool firstTimeUsingInventory;
        public InventorySlotData[] slots;
        public InventoryControllerData(InventorySlotData[] slots, string modVersion, bool firstTimeUse = false, bool firstTimeModLoaded = false)
        {
            this.firstTimeModLoaded = firstTimeModLoaded;
            this.firstTimeUsingInventory = firstTimeUse;
            this.slots = slots;
            this.modVersion = modVersion;
        }

        public InventoryControllerData()
        {

        }
    }
}
