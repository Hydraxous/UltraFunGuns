using GameConsole;
using HydraDynamics.Keybinds;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace UltraFunGuns
{
    public class KeybindNode : MonoBehaviour
    {
        [SerializeField] private Text displayName, currentBind;
        [SerializeField] private Button rebindButton;
        private Keybinding binding;

        public void DoRebind()
        {
            if (binding == null)
                return;

            if(KeybindManager.RebindingKey)
            {
                KeybindManager.CancelRebinding();
            }

            if (currentBind != null)
            {
                currentBind.text = "<color=orange>???</color>";
            }

            if (rebindButton != null)
            {
                rebindButton.interactable = false;
            }

            binding.Rebind((bind) =>
            {
                if (currentBind != null)
                    currentBind.text = $"<color=orange>{bind.KeyCode}</color>";

                if (rebindButton != null)
                    rebindButton.interactable = true;
            });
        }

        public void SetBind(Keybinding binding)
        {
            this.binding = binding;
            RefreshData();
        }

        public void RefreshData()
        {
            if (binding == null)
                return;

            if (displayName != null)
                displayName.text = binding.Name;

            if (currentBind != null)
                currentBind.text = $"<color=orange>{binding.KeyCode}</color>";
        }

    }
}
