using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace UltraFunGuns
{
    public class InventoryControllerDeployer : MonoBehaviour
    {

        RectTransform canvas;
        Transform configHelpMessage;
        OptionsManager om;
        InventoryController invController;
        Button invControllerButton, configHelpButton;
        GameObject pauseMenu;

        public bool inventoryManagerOpen = false;

        private KeyCode inventoryKey;

        private bool sentBindingMessage = false;

        private void Awake()
        {
            
            inventoryKey = UltraFunGuns.INVENTORY_KEY.Value;
            om = MonoSingleton<OptionsManager>.Instance;
            canvas = GetComponent<RectTransform>();
            pauseMenu = transform.Find("PauseMenu").gameObject;
            HydraLoader.prefabRegistry.TryGetValue("UFGInventoryUI", out GameObject controllerObject);
            HydraLoader.prefabRegistry.TryGetValue("UFGInventoryButton", out GameObject pauseMenuInvButton);
            invControllerButton = GameObject.Instantiate<GameObject>(pauseMenuInvButton, canvas).GetComponent<Button>();
            invControllerButton.onClick.AddListener(OpenInventory);
            invController = GameObject.Instantiate<GameObject>(controllerObject, canvas).GetComponent<InventoryController>();
            invController.gameObject.SetActive(false);
            invControllerButton.gameObject.SetActive(false);

            configHelpMessage = invController.transform.Find("ConfigMessage");

            configHelpButton = invController.transform.Find("MenuBorder/SlotNames").GetComponent<Button>();
            configHelpButton.onClick.AddListener(SendConfigHelpMessage);
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
                        configHelpMessage.gameObject.SetActive(false);
                        sentBindingMessage = false;
                    }

                }
                else
                {
                    if(invController.data.firstTimeModLoaded)
                    {
                        MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(String.Format("UFG: Set a custom loadout for UFG weapons with [<color=orange>{0}</color>] or in the pause menu.",inventoryKey.ToString()), "", "", 2);
                        invController.data.firstTimeModLoaded = false;
                    }

                    if (inventoryManagerOpen)
                    {
                        inventoryManagerOpen = false;
                        invController.gameObject.SetActive(false);

                    }
                    invControllerButton.gameObject.SetActive(false);
                }
                
                if(Input.GetKeyDown(inventoryKey) && !om.paused)
                {
                    OpenInventory();
                }
            }

        }

        public void OpenInventory()
        {
            if(!inventoryManagerOpen)
            {
                if (!om.paused)
                {
                    om.Pause();
                }

                if (invController.data.firstTimeUsingInventory)
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

        public void SendConfigHelpMessage()
        {
            if(!sentBindingMessage && om.paused)
            {
                StartCoroutine(DisplayMessage());
            }
        }

        IEnumerator DisplayMessage()
        {
            sentBindingMessage = true;
            configHelpMessage.gameObject.SetActive(true);
            yield return new WaitForSeconds(4.0f);
            configHelpMessage.gameObject.SetActive(false);
            sentBindingMessage = false;
        }

    }
}
