using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace UltraFunGuns
{
    public class InventorySlot : MonoBehaviour
    {
        InventoryController inventoryController;
        public int ID;
        public GameObject nodePrefab;
        public List<InventoryNode> nodes;

        public RectTransform r_transform;

        public void Initialize(InventoryNodeData[] nodeDatas, int ID, InventoryController inventoryController)
        {
            this.inventoryController = inventoryController;

            r_transform = transform.Find("Slot").GetComponent<RectTransform>();
            HydraLoader.prefabRegistry.TryGetValue("WMUINode", out nodePrefab);
            this.ID = ID;

            SetupNodes(nodeDatas);
        }

        public void ChangeNodeOrder(InventoryNode node, int slotsToMove)
        {
            List<InventoryNode> nodeListCopy = nodes;
            int newSlot = node.slotIndexPosition + slotsToMove;
            nodeListCopy.Remove(node);
            if (newSlot >= nodes.Count)
            {       
                nodeListCopy.Insert(0, node);
            }
            else if(newSlot < 0)
            {
                nodeListCopy.Add(node);
            }else
            {
                nodeListCopy.Insert(newSlot, node);
            }
            nodes = nodeListCopy;
            Refresh();
        }

        public void InsertNode(InventoryNode node)
        {
            RectTransform nodeTransform = node.GetComponent<RectTransform>();
            nodeTransform.SetParent(r_transform);
            int oldCount = nodes.Count;
            int newSlotPosition = node.slotIndexPosition;
            if(node.slotIndexPosition >= oldCount)
            {
                nodes.Add(node);
                newSlotPosition = oldCount;
            }else
            {
                nodes.Insert(node.slotIndexPosition, node);
            } 
            node.ChangeSlot(this, newSlotPosition);
        }

        public void RemoveNode(InventoryNode node)
        {
            nodes.Remove(node);
            Refresh();
        }

        private void CreateNewNode(InventoryNodeData nodeData, int slotIndex, int totalNodes)
        {
            InventoryNode newNode = GameObject.Instantiate<GameObject>(nodePrefab, r_transform).GetComponent<InventoryNode>();
            newNode.Initialize(nodeData, this, slotIndex);
            nodes.Add(newNode);
        }

        public void ButtonPressed(InventoryNode node, string button)
        {
            inventoryController.ButtonPressed(node, this, button);
        }

        private void SetupNodes(InventoryNodeData[] nodeDatas)
        {
            if (nodes.Count > 0)
            {
                foreach (InventoryNode node in nodes)
                {
                    nodes.Remove(node);
                    node.Disappear();
                }
                nodes.Clear();//TODO Redundant maybe
            }

            for (int i = 0; i < nodeDatas.Length; i++)
            {
                CreateNewNode(nodeDatas[i], i, nodeDatas.Length);
            }
        }

        public void Refresh()
        {
            for(int i = 0; i < nodes.Count; i++)
            {
                nodes[i].slotIndexPosition = i;
                nodes[i].RefreshPosition();
            }
        }

        public InventorySlotData GetSlotData()
        {
            List<InventoryNodeData> nodeData = new List<InventoryNodeData>();
            foreach(InventoryNode node in nodes)
            {
                InventoryNodeData newNodeData = node.data;
                nodeData.Add(newNodeData);
            }
            InventorySlotData slotData = new InventorySlotData(nodeData.ToArray());
            return slotData;
        }
    }

    [System.Serializable]
    public class InventorySlotData
    {
        public InventoryNodeData[] slotData;

        public InventorySlotData(InventoryNodeData[] slotData)
        {
            this.slotData = slotData;
        }

        public InventorySlotData()
        {

        }
    }
}
