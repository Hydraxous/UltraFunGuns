using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.CompilerServices;

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
        }

        private void Update()
        {
            CheckThings();
            NewCheckStatus();
        }

        private void CheckThings()
        {
            if(om == null)
            {
                om = MonoSingleton<OptionsManager>.Instance;
            }

            if(canvas == null)
            {
                canvas = GetComponent<RectTransform>();
            }

            if(pauseMenu == null)
            {
                pauseMenu = transform.Find("PauseMenu").gameObject;
            }

            if (invController == null)
            {
                HydraLoader.prefabRegistry.TryGetValue("UFGInventoryUI", out GameObject controllerObject);
                invController = GameObject.Instantiate<GameObject>(controllerObject, canvas).GetComponent<InventoryController>();
                invController.gameObject.SetActive(false);
                configHelpMessage = invController.transform.Find("ConfigMessage");
                configHelpButton = invController.transform.Find("MenuBorder/SlotNames").GetComponent<Button>();
                configHelpButton.onClick.AddListener(SendConfigHelpMessage);
            }

            if (invControllerButton == null)
            {
                HydraLoader.prefabRegistry.TryGetValue("UFGInventoryButton", out GameObject pauseMenuInvButton);
                invControllerButton = GameObject.Instantiate<GameObject>(pauseMenuInvButton, canvas).GetComponent<Button>();
                invControllerButton.onClick.AddListener(OpenInventory);
                invControllerButton.gameObject.SetActive(false);
            }
        }


        private void NewCheckStatus()
        {
            if (!UltraFunGuns.InLevel())
            {
                return;
            }

            if (inventoryManagerOpen)
            {

                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    CloseInventory();
                }
            }

            invControllerButton.gameObject.SetActive(om.paused && !inventoryManagerOpen);

            if (Input.GetKeyDown(UltraFunGuns.INVENTORY_KEY.Value) && !om.paused)
            {
                OpenInventory();
            }
        }

        public void OpenInventory()
        {
            if (invController.gameObject.activeInHierarchy)
            {
                return;
            }

            if (om.paused)
            {
                om.UnPause();
            }

            om.paused = true;

            invController.RefreshSlotKeyDisplays();

            GameState ufgInvState = new GameState("ufg_inv", invController.gameObject);
            ufgInvState.cursorLock = LockMode.Unlock;
            ufgInvState.playerInputLock = LockMode.Lock;
            ufgInvState.cameraInputLock = LockMode.Lock;
            ufgInvState.priority = 2;
            GameStateManager.Instance.RegisterState(ufgInvState);

            invControllerButton.gameObject.SetActive(false);
            invController.gameObject.SetActive(true);
            inventoryManagerOpen = true;
        }

        public void CloseInventory()
        {
            inventoryManagerOpen = false;
            om.paused = false;
            configHelpMessage.gameObject.SetActive(false);
            invController.gameObject.SetActive(false);
            if (invController.data.firstTimeModLoaded)
            {
                MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(String.Format("UFG: Set a custom loadout for UFG weapons with [<color=orange>{0}</color>] or in the pause menu.", inventoryKey.ToString()), "", "", 2);
                invController.data.firstTimeModLoaded = false;

            }
            else if (!invController.data.lecturedAboutVersion && !UltraFunGuns.UsingLatestVersion)
            {
                MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage($"UFG: You are using an outdated version of UltraFunGuns. Consider updating to <color=orange>{UltraFunGuns.RELEASE_VERSION}</color>.", "", "", 2);
                invController.data.lecturedAboutVersion = true;
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
