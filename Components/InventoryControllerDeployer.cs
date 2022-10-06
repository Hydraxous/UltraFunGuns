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
                    if(invController.data.firstTimeModLoaded)
                    {
                        MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("UFG: You can set a custom loadout for UFG weapons in the pause menu.", "", "", 2);
                        invController.data.firstTimeModLoaded = false;
                    }

                    if (inventoryManagerOpen)
                    {
                        inventoryManagerOpen = false;
                        invController.gameObject.SetActive(false);
                    }
                    invControllerButton.gameObject.SetActive(false);
                }
            }

        }

        public void DoButtonAction()
        {
            if(invController.data.firstTimeUsingInventory)
            {
                MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("WARNING: Having UFG weapons enabled at any point will enable the Major Assists for the duration of the level.", "", "", 4);
                invController.data.firstTimeUsingInventory = false;
            }
            pauseMenu.SetActive(false);
            invControllerButton.gameObject.SetActive(false);
            invController.gameObject.SetActive(true);
            inventoryManagerOpen = true;
        }
    }
}
