using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace UltraFunGuns
{
    public class InventoryControllerDeployer : MonoBehaviour
    {

        RectTransform canvas;
        OptionsManager om;
        InventoryController invController;
        Button invControllerButton;
        GameObject pauseMenu;

        public bool inventoryManagerOpen = false;

        private void Awake()
        {
            Debug.Log("InventoryContoller Spawned");
            om = MonoSingleton<OptionsManager>.Instance;
            canvas = GetComponent<RectTransform>();
            pauseMenu = transform.Find("PauseMenu").gameObject;
            HydraLoader.prefabRegistry.TryGetValue("UFGInventoryUI", out GameObject controllerObject);
            HydraLoader.prefabRegistry.TryGetValue("UFGInventoryButton", out GameObject pauseMenuInvButton);
            invControllerButton = GameObject.Instantiate<GameObject>(pauseMenuInvButton, canvas).GetComponent<Button>();
            invControllerButton.onClick.AddListener(DoButtonAction);
            invController = GameObject.Instantiate<GameObject>(controllerObject, canvas).GetComponent<InventoryController>();
            invController.gameObject.SetActive(false);
            invControllerButton.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (UltraFunGuns.InLevel())
            { 
                if (om.paused)
                {
                    if (inventoryManagerOpen)
                    {
                        invController.gameObject.SetActive(true);
                    }
                    else
                    {
                        invController.gameObject.SetActive(false);
                        invControllerButton.gameObject.SetActive(true);
                    }

                }
                else
                {
                    if (inventoryManagerOpen)
                    {
                        inventoryManagerOpen = false;
                        invController.gameObject.SetActive(false);
                    }
                }
            }

        }

        public void DoButtonAction()
        {
            pauseMenu.SetActive(false);
            invControllerButton.gameObject.SetActive(false);
            invController.gameObject.SetActive(true);
            inventoryManagerOpen = true;
        }
    }
}
