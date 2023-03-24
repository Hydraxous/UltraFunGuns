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
        [UFGAsset("UFGKeybindNode")] private static GameObject bindingNodePrefab;

        private List<KeybindNode> nodes = new List<KeybindNode>();

        private void Start()
        {
            PopulateMenu();
        }

        private void Update()
        {
            if (!Keys.KeybindManager.RebindingKey)
            {
                if(Input.GetKeyDown(KeyCode.Escape))
                {
                    gameObject.SetActive(false);
                }
            }
        }

        private void PopulateMenu()
        {
            if (bindingNodePrefab == null)
                return;

            Keybinding[] keybindings = Keys.KeybindManager.Bindings.Data.GetBinds();

            Debug.Log($"Keybinds populating. {keybindings.Length}");
            foreach (var keybind in keybindings)
            {
                Debug.Log($"{keybind.Name}:{keybind.KeyCode}");
            }

            foreach(Keybinding binding in keybindings)
            {
                if(binding.KeybindManager == null)
                {
                    continue;
                }

                KeybindNode newNode = Instantiate<GameObject>(bindingNodePrefab, bindingsBoxes).GetComponent<KeybindNode>();
                newNode.SetBind(binding);
                nodes.Add(newNode);
            }
        }

        public void RefreshNodes()
        {
            foreach(KeybindNode node in nodes)
            {
                node.RefreshData();
            }
        }

    }
}
