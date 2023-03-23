using HydraDynamics.Keybinds;
using System;
using System.Collections.Generic;
using System.Text;
using UltraFunGuns.Datas;
using UnityEngine;

namespace UltraFunGuns
{
    public class KeybindMenu : MonoBehaviour
    {
        [SerializeField] private RectTransform bindingsBoxes;
        [UFGAsset("BindingNodePrefab")] private static GameObject bindingNodePrefab;

        private List<KeybindNode> nodes = new List<KeybindNode>();

        private void Start()
        {
            PopulateMenu();
        }


        private void PopulateMenu()
        {
            if (bindingNodePrefab == null)
                return;

            Keybinding[] keybindings = Keys.KeybindManager.Bindings.Data.binds.ToArray();

            foreach(Keybinding binding in keybindings)
            {
                KeybindNode newNode = Instantiate<GameObject>(bindingNodePrefab, bindingsBoxes).GetComponent<KeybindNode>();
                newNode.SetBind(binding);
                nodes.Add(newNode);
            }


        }

    }
}
