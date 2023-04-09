using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UltraFunGuns.Datas;
using HydraDynamics.Keybinds;
using HydraDynamics;

namespace UltraFunGuns
{
    public class InventoryControllerDeployer : MonoBehaviour
    {
        RectTransform canvas;
        Transform configHelpMessage, versionHelpMessage;
        OptionsManager om;
        InventoryController invController;
        Button invControllerButton, configHelpButton;
        GameObject pauseMenu;

        public bool inventoryManagerOpen = false;

        public static Keybinding inventoryKey { get; private set; } = Hydynamics.GetKeybinding("Inventory", KeyCode.I);

        private static bool sentVersionMessage = false;

        private bool displayingHelpMessage = false;

        [UFGAsset("UFGInventoryUI")] private static GameObject UFGInventoryUI;
        [UFGAsset("UFGInventoryButton")] private static GameObject UFGInventoryButton;
        [UFGAsset("WMUINode")] public static GameObject UFGInventoryNode { get; private set; }
        [UFGAsset("UFGKeybindsMenu")] private static GameObject UFGKeybindsUI;


        private KeybindMenu keybindMenu;


        //TODO optimization
        private void Awake()
        {
            om = MonoSingleton<OptionsManager>.Instance;
            canvas = GetComponent<RectTransform>();
            pauseMenu = transform.Find("PauseMenu").gameObject;

            invControllerButton = GameObject.Instantiate<GameObject>(UFGInventoryButton, canvas).GetComponent<Button>();
            invControllerButton.onClick.AddListener(OpenInventory);

            invController = GameObject.Instantiate<GameObject>(UFGInventoryUI, canvas).GetComponent<InventoryController>();
            invController.gameObject.SetActive(false);
            invControllerButton.gameObject.SetActive(false);

            keybindMenu = GameObject.Instantiate<GameObject>(UFGKeybindsUI, canvas).GetComponent<KeybindMenu>();
            keybindMenu.gameObject.SetActive(false);

            configHelpMessage = invController.transform.Find("ConfigMessage");
            versionHelpMessage = invController.transform.Find("VersionMessage");
            versionHelpMessage.GetComponentInChildren<Text>().text = string.Format(versionHelpMessage.GetComponentInChildren<Text>().text, UltraFunGuns.LatestVersion); //?????????????

            configHelpButton = invController.transform.Find("MenuBorder/SlotNames").GetComponent<Button>();
            configHelpButton.onClick.AddListener(OpenKeybindMenu);
        }

        private void Update()
        {
            CheckStatus();
        }

        private void CheckStatus()
        {
            if (!UKAPIP.InLevel())
            {
                return;
            }

            if(inventoryManagerOpen)
            {
                if (!UltraFunGuns.UsingLatestVersion)
                {
                    SendVersionHelpMessage();
                }

                if(Input.GetKeyDown(KeyCode.Escape))
                {
                    CloseInventory();
                }

            }else
            {
                displayingHelpMessage = false;
                if (Data.SaveInfo.Data.firstTimeModLoaded)
                {
                    MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(String.Format("UFG: Set a custom loadout for UFG weapons with [<color=orange>{0}</color>] or in the pause menu.", inventoryKey.KeyCode.ToString()), "", "", 2);
                    Data.SaveInfo.Data.firstTimeModLoaded = false;
                    Data.SaveInfo.Save();
                }
            }
            
            invControllerButton.gameObject.SetActive(om.paused && !inventoryManagerOpen);

            if (inventoryKey.WasPerformedThisFrame)
            {
                OpenInventory();
            }
        }

        public void CloseInventory()
        {
            inventoryManagerOpen = false;
            om.paused = false;
            invController.SetCardActive(false);
            configHelpMessage.gameObject.SetActive(false);
            versionHelpMessage.gameObject.SetActive(false);
            displayingHelpMessage = false;
            invController.gameObject.SetActive(false);
        }

        public void OpenInventory()
        {
            if (invController.gameObject.activeInHierarchy)
            {
                return;
            }

            if(om.paused)
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

            if (Data.SaveInfo.Data.firstTimeUsingInventory)
            {
                //MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("WARNING: Having UFG weapons enabled at any point will enable the Major Assists for the duration of the level.", "", "", 4);
                Data.SaveInfo.Data.firstTimeUsingInventory = false;
                Data.SaveInfo.Save();
            }
            invControllerButton.gameObject.SetActive(false);
            invController.gameObject.SetActive(true);
            inventoryManagerOpen = true;
        }

        public void OpenKeybindMenu()
        {
            if(keybindMenu.gameObject.activeInHierarchy)
            {
                return;
            }

            if (inventoryManagerOpen)
            {
                CloseInventory();
            }

            if (om.paused)
            {
                om.UnPause();
            }

            om.paused = true;

            keybindMenu.gameObject.SetActive(true);
            keybindMenu.RefreshNodes();

            GameState ufgKeybindState = new GameState("ufg_keybinds", keybindMenu.gameObject);
            ufgKeybindState.cursorLock = LockMode.Unlock;
            ufgKeybindState.playerInputLock = LockMode.Lock;
            ufgKeybindState.cameraInputLock = LockMode.Lock;
            ufgKeybindState.priority = 3;
            GameStateManager.Instance.RegisterState(ufgKeybindState);
        }

        public void CloseKeybindMenu()
        {
            om.UnPause();
            keybindMenu.gameObject.SetActive(false);
        }

        public void SendConfigHelpMessage()
        {
            if(om.paused && !displayingHelpMessage)
            {
                StartCoroutine(DisplayHelpMessage(configHelpMessage));
            }
        }

        public void SendVersionHelpMessage()
        { 
            if (!sentVersionMessage && om.paused && !displayingHelpMessage && !Data.Config.Data.DisableVersionMessages)
            {
                sentVersionMessage = true;
                StartCoroutine(DisplayHelpMessage(versionHelpMessage));
            }
        }

        private IEnumerator DisplayHelpMessage(Transform message)
        {
            displayingHelpMessage = true;
            message.gameObject.SetActive(true);

            yield return new WaitForSecondsRealtime(4);

            message.gameObject.SetActive(false);
            displayingHelpMessage = false;
        }

        

    }
}
