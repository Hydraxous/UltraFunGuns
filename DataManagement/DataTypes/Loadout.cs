using HydraDynamics.DataPersistence;
using System;
using System.Collections.Generic;
using System.Text;

namespace UltraFunGuns
{
    [System.Serializable]
    public class Loadout : Validatable
    {
        public override bool AllowExternalRead => true;

        public InventorySlotData[] slots;

        public Loadout(InventorySlotData[] slots)
        {
            this.slots = slots;
        }

        public Loadout()
        {
            this.slots = WeaponManager.GetDefaultLoadout();
        }

        public bool CheckUnlocked(string weaponKey)
        {
            foreach (InventorySlotData slot in slots)
            {
                foreach (InventoryNodeData node in slot.slotNodes)
                {
                    if (node.weaponKey != weaponKey)
                        continue;

                    return node.weaponUnlocked;
                }
            }

            return false;
        }

        public void SetUnlocked(string weaponKey, bool unlocked)
        {
            foreach (InventorySlotData slot in slots)
            {
                foreach (InventoryNodeData node in slot.slotNodes)
                {
                    if (node.weaponKey != weaponKey)
                        continue;

                    node.weaponUnlocked = unlocked;
                    return;
                }
            }
        }

        public override bool Validate()
        {
            if (!(slots.Length > 0))
            {
                return false;
            }

            int wepCounter = 0;
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] == null)
                {
                    return false;
                }

                if (slots[i].slotNodes.Length > 0)
                {
                    wepCounter += slots[i].slotNodes.Length;
                }

                if (!(wepCounter > 0))
                {
                    return false;
                }
            }

            if (wepCounter != WeaponManager.WeaponCount)
            {
                return false;
            }

            return true;
        }
    }
}
