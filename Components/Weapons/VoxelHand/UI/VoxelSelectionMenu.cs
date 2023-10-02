using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using UltraFunGuns.UI;
using UnityEngine;
using UnityEngine.UI;

namespace UltraFunGuns
{
    public class VoxelSelectionMenu : MonoBehaviour
    {
        [UFGAsset("VoxelSelectionMenu")] private static GameObject prefab;
        public static GameObject VoxelSelectionMenuPrefab => prefab;

        [SerializeField] private GameObject container;
        [SerializeField] private RectTransform contentBody;
        [SerializeField] private GameObject selectionButtonPrefab;

        [SerializeField] private Button importButton;
        [SerializeField] private Text importButtonText;

        public bool IsOpen => container.activeInHierarchy;

        private List<VoxelMenuButton> instancedButtons = new List<VoxelMenuButton>();
        
        private VoxelHand voxelHand;

        public void SetVoxelHand(VoxelHand voxelHand)
        {
            this.voxelHand = voxelHand;
        }

        GameState voxelSelectGameState;
        private void Awake()
        {
            voxelSelectGameState = new GameState("VoxelSelect", container);
            voxelSelectGameState.cursorLock = LockMode.Unlock;
            voxelSelectGameState.cameraInputLock = LockMode.Unlock;
            voxelSelectGameState.playerInputLock = LockMode.Unlock;
            voxelSelectGameState.priority = 100;
            container.SetActive(false);
        }

        private void RebuildMenu()
        {
            ClearMenu();

            VoxelData[] placeable = VoxelDatabase.GetPlaceableVoxels();
            foreach (VoxelData voxel in placeable.OrderBy(x=>x.DisplayName))
            {
                GameObject newButton = GameObject.Instantiate(selectionButtonPrefab, contentBody);
                VoxelMenuButton button = newButton.GetComponent<VoxelMenuButton>();
                button.SetVoxelData(voxel);
                button.SetSelected(false);

                if (voxelHand != null)
                    if (voxelHand.CurrentVoxelData == voxel)
                        button.SetSelected(true);
                
                button.SetButtonAction(ButtonPressed);
                instancedButtons.Add(button);
            }
        }

        private void ButtonPressed(VoxelMenuButton button, VoxelData data)
        {
            instancedButtons.ForEach(x =>
            {
                x?.SetSelected(x == button);
            });

            voxelHand?.SetHeldVoxel(data);
        }

        private void Update()
        {
            if (!IsOpen)
                return;

            if (Input.GetKeyDown(KeyCode.Escape))
                CloseMenu();
        }

        public void OpenMenu() 
        {
            RebuildMenu();
            container.SetActive(true);
            Time.timeScale = 0f;
            OptionsManager.Instance.paused = true;
            CameraController.Instance.enabled = false;
            GameStateManager.Instance.RegisterState(voxelSelectGameState);
        }

        public void CloseMenu()
        {
            OptionsManager.Instance.paused = false;
            CameraController.Instance.enabled = true;
            Time.timeScale = 1f;
            GameStateManager.Instance.PopState(voxelSelectGameState.key);
            container.SetActive(false);
        }

        private void ClearMenu()
        {
            for(int i=0;i< instancedButtons.Count;i++)
            {
                if (instancedButtons[i] == null)
                    continue;

                VoxelMenuButton vmb = instancedButtons[i];
                instancedButtons[i] = null;

                Destroy(vmb.gameObject);
            }

            instancedButtons.Clear();
        }

        //TODO this is horrid. Assumes the VoxelHand calls OpenMenu and provides it's SetHeldVoxel method as onSelect. Disgusting... Fix this.
        public void Button_DeselectHeldVoxel()
        {
            voxelHand?.SetHeldVoxel(null);
        }

        public void Button_RefreshVoxels()
        {
            if (VoxelDatabase.IsImportingTextures)
                return;

            StartCoroutine(ImportCustomVoxel());
        }

        private IEnumerator ImportCustomVoxel()
        {
            string buttonText = importButtonText.text;
            importButton.interactable = false;
            
            VoxelDatabase.ImportCustomBlocksAsync(null);
            while(VoxelDatabase.IsImportingTextures)
            {
                importButtonText.text =$"{Mathf.CeilToInt(VoxelDatabase.TextureImportProgress*100f)}%";
                yield return new WaitForEndOfFrame();
            }

            importButtonText.text = buttonText;
            importButton.interactable = true;
        }

        public void Button_OpenCustomVoxelFolder()
        {
            VoxelDatabase.OpenCustomVoxelFolder();
        }

        public void Button_SaveCurrentScene()
        {
            VoxelWorld.SaveWorld();
        }

        public void Button_LoadCurrentScene()
        {
            VoxelWorld.LoadWorld();
        }

        public void Button_ClearAllVoxels()
        {
            VoxelWorld.ClearBlocks();
        }
    }
}
