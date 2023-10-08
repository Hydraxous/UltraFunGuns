using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
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

        private Dictionary<VoxelData, VoxelMenuButton> instancedButtons = new Dictionary<VoxelData, VoxelMenuButton>();


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

        private void Start()
        {
            StartCoroutine(ButtonUpdate());
        }

        private IEnumerator ButtonUpdate()
        {
            string buttonText = importButtonText.text;
            bool importingLastCheck = false;

            while (true)
            {
                bool importing = VoxelDatabase.IsImportingTextures;

                if(importing)
                {
                    importButtonText.text = $"{Mathf.CeilToInt(VoxelDatabase.TextureImportProgress * 100f)}%";
                    importButton.interactable = false;
                }
                else if(importingLastCheck)
                {
                    importButtonText.text = buttonText;
                    importButton.interactable = true;
                }

                importingLastCheck = importing;
                yield return new WaitForSeconds(0.5f);
            }
        }

        private bool rebuilding;

        [Configgy.Configgable("UltraFunGuns/Voxel/Palette")]
        private static int maxIconsPerFrame = 10;

        private IEnumerator RebuildAsync()
        {
            rebuilding = true;
            int counter = 0;

            foreach (VoxelData voxel in VoxelDatabase.GetPlaceableVoxels().OrderBy(x => x.DisplayName))
            {
                //Already built the button, so skip it.
                if (instancedButtons.ContainsKey(voxel))
                    continue;

                instancedButtons.Add(voxel, BuildButton(voxel));

                if (counter % maxIconsPerFrame == 0)
                    yield return new WaitForEndOfFrame();

                counter++;
            }

            rebuilding = false;
        }


        private VoxelMenuButton BuildButton(VoxelData voxelData)
        {
            GameObject newButton = GameObject.Instantiate(selectionButtonPrefab, contentBody);
            VoxelMenuButton button = newButton.GetComponent<VoxelMenuButton>();
            button.SetVoxelData(voxelData);
            button.SetSelected(false);

            if (voxelHand != null)
                if (voxelHand.CurrentVoxelData == voxelData)
                    button.SetSelected(true);

            button.SetButtonAction(ButtonPressed);
            return button;
        }


        private void RebuildMenu()
        {
            if(rebuilding)
                StopCoroutine(RebuildAsync());

            StartCoroutine(RebuildAsync());
        }

        private void ButtonPressed(VoxelMenuButton button, VoxelData data)
        {
            foreach (VoxelMenuButton vmb in instancedButtons.Values)
                vmb?.SetSelected(vmb == button);

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
            foreach (KeyValuePair<VoxelData, VoxelMenuButton> btn in instancedButtons)
            {
                if (btn.Value == null)
                    continue;

                VoxelMenuButton vmb = btn.Value;
                GameObject.Destroy(vmb.gameObject);
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

            VoxelDatabase.ImportCustomBlocksAsync(null);
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
