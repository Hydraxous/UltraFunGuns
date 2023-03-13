using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using UMM;
using UnityEngine;
using UnityEngine.UI;

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

        private static UKKeyBind inventoryKey = UKAPI.GetKeyBind("<color=orange>UFG</color> Inventory", KeyCode.I);

        private static bool sentVersionMessage = false;

        private bool displayingHelpMessage = false;

        [UFGAsset("UFGInventoryUI")] private static GameObject UFGInventoryUI;
        [UFGAsset("UFGInventoryButton")] private static GameObject UFGInventoryButton;
        [UFGAsset("WMUINode")] public static GameObject UFGInventoryNode { get; private set; }

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

            configHelpMessage = invController.transform.Find("ConfigMessage");
            versionHelpMessage = invController.transform.Find("VersionMessage");
            versionHelpMessage.GetComponentInChildren<Text>().text = string.Format(versionHelpMessage.GetComponentInChildren<Text>().text, UltraFunGuns.LatestVersion); //?????????????

            configHelpButton = invController.transform.Find("MenuBorder/SlotNames").GetComponent<Button>();
            configHelpButton.onClick.AddListener(SendConfigHelpMessage);
        }

        private void Update()
        {
            CheckInventoryStatus();
        }

        //TODO optimization
        private void CheckInventoryStatus()
        {
            if (!UKAPIP.InLevel())
            {
                return;
            }

            if (om.paused)
            {
                if (inventoryManagerOpen)
                {
                    invController.gameObject.SetActive(true);
                    if (!UltraFunGuns.UsingLatestVersion)
                    {
                        SendVersionHelpMessage();
                    }
                }
                else
                {
                    invController.gameObject.SetActive(false);
                    invControllerButton.gameObject.SetActive(true);
                    configHelpMessage.gameObject.SetActive(false);
                    versionHelpMessage.gameObject.SetActive(false);
                    displayingHelpMessage = false;
                }
            }
            else
            {
                if (Data.Save.Data.firstTimeModLoaded)
                {
                    MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(String.Format("UFG: Set a custom loadout for UFG weapons with [<color=orange>{0}</color>] or in the pause menu.", inventoryKey.keyBind.ToString()), "", "", 2);
                    Data.Save.Data.firstTimeModLoaded = false;
                    Data.Save.Save();
                }

                if (inventoryManagerOpen)
                {
                    CloseInventory();
                }
                invControllerButton.gameObject.SetActive(false);
            }

            if (inventoryKey.WasPerformedThisFrameInScene)
            {
                invController.RefreshSlotKeyDisplays();
                OpenInventory();
            }
        }

        public void CloseInventory()
        {
            inventoryManagerOpen = false;
            invController.SetCardActive(false);
            invController.gameObject.SetActive(false);
        }

        public void OpenInventory()
        {
            if(inventoryManagerOpen)
            {
                return;
            }

            if (!om.paused)
            {
                om.Pause();
            }

            if (Data.Save.Data.firstTimeUsingInventory)
            {
                MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("WARNING: Having UFG weapons enabled at any point will enable the Major Assists for the duration of the level.", "", "", 4);
                Data.Save.Data.firstTimeUsingInventory = false;
                Data.Save.Save();
            }
            pauseMenu.SetActive(false);
            invControllerButton.gameObject.SetActive(false);
            invController.gameObject.SetActive(true);
            inventoryManagerOpen = true;    
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
