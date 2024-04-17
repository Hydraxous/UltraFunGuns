using Configgy;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace UltraFunGuns
{
    public class VoxelSelectionMenu : MonoBehaviour
    {
        [UFGAsset("VoxelSelectionMenu")] private static GameObject prefab;
        [UFGAsset("BlockSelectionIcon")] private static GameObject selectionButtonPrefab;
        public static GameObject VoxelSelectionMenuPrefab => prefab;

        private GameObject container;
        private RectTransform contentBody;

        private Button refreshButton;
        private Text refreshButtonText;
        private Text pageNumberLabel;

        private Button nextPageButton;
        private Button previousPageButton;
        private Button deselectButton;
        private Button openFolderButton;
        private Button saveButton;
        private Button clearButton;
        private Button worldsButton;

        private InputField searchField;

        private int currentPage = 0;

        private List<List<GameObject>> pages = new List<List<GameObject>>();

        public bool IsOpen => container.activeInHierarchy || (savesMenu != null && savesMenu.gameObject.activeInHierarchy);

        private Dictionary<VoxelData, VoxelMenuButton> instancedButtons = new Dictionary<VoxelData, VoxelMenuButton>();

        private VoxelHand voxelHand;

        private VoxelSavesMenu savesMenu;

        public void SetVoxelHand(VoxelHand voxelHand)
        {
            this.voxelHand = voxelHand;
        }

        GameState voxelSelectGameState;
        private void Awake()
        {
            //GUH
            InitializeReferences();
        }

        private void Start()
        {
            StartCoroutine(ButtonUpdate());
        }

        private bool initialized;

        private void InitializeReferences()
        {
            if (initialized)
                return;

            container = transform.LocateComponent<RectTransform>("Panel_SelectionMenu").gameObject;
            contentBody = transform.LocateComponent<RectTransform>("ListView_VoxelContent");

            pageNumberLabel = transform.LocateComponent<Text>("Text_PageNumber");
            
            savesMenu = GetComponentInChildren<VoxelSavesMenu>(true);

            saveButton = container.transform.LocateComponent<Button>("Button_Save");
            refreshButton = transform.LocateComponent<Button>("Button_RefreshVoxels");
            openFolderButton = transform.LocateComponent<Button>("Button_OpenVoxelsFolder");
            clearButton = transform.LocateComponent<Button>("Button_ClearAll");
            worldsButton = transform.LocateComponent<Button>("Button_Worlds");
            previousPageButton = transform.LocateComponent<Button>("Button_PreviousPage");
            nextPageButton = transform.LocateComponent<Button>("Button_NextPage");
            deselectButton = transform.LocateComponent<Button>("Button_Deselect");

            refreshButtonText = refreshButton.transform.LocateComponent<Text>("Text");

            searchField = transform.LocateComponent<InputField>("InputField_VoxelSearchBar");
            searchField.interactable = false;
            searchField.placeholder.GetComponent<Text>().text = "Search not implemented yet! Sorry!";

            worldsButton.SetClickAction(Button_Worlds);

            saveButton.SetClickAction(() =>
            {
                VoxelWorld.SaveCurrentWorld();
                saveButton.gameObject.SetActive(false);
            });

            clearButton.SetClickAction(Button_ClearAllVoxels);
            openFolderButton.SetClickAction(Button_OpenCustomVoxelFolder);
            refreshButton.SetClickAction(Button_RefreshVoxels);
            worldsButton.SetClickAction(Button_Worlds);
            deselectButton.SetClickAction(Button_DeselectHeldVoxel);

            previousPageButton.SetClickAction(() => NextPage(-1));
            nextPageButton.SetClickAction(() => NextPage(1));

            voxelSelectGameState = new GameState("VoxelSelect");
            voxelSelectGameState.cursorLock = LockMode.Unlock;
            voxelSelectGameState.cameraInputLock = LockMode.Unlock;
            voxelSelectGameState.playerInputLock = LockMode.Unlock;
            voxelSelectGameState.priority = 100;
            container.SetActive(false);

            initialized = true;
        }


        private IEnumerator ButtonUpdate()
        {
            string buttonText = refreshButtonText.text;
            bool importingLastCheck = false;

            while (true)
            {
                bool importing = VoxelDatabase.IsImportingTextures;

                if(importing)
                {
                    refreshButtonText.text = $"{Mathf.CeilToInt(VoxelDatabase.TextureImportProgress * 100f)}%";
                    refreshButton.interactable = false;
                }
                else if(importingLastCheck)
                {
                    refreshButtonText.text = buttonText;
                    refreshButton.interactable = true;
                }

                importingLastCheck = importing;
                yield return new WaitForSecondsRealtime(0.5f);
            }
        }

        private bool rebuilding;

        [Configgy.Configgable("Voxel/Palette")]
        private static int maxIconsPerFrame = 10;

        [Configgy.Configgable("Voxel/Palette")]
        private static int maxIconsPerPage = 128;


        private IEnumerator RebuildAsync()
        {
            rebuilding = true;
            int counter = 0;

            List<GameObject> currentPage = null;

            foreach (VoxelData voxel in VoxelDatabase.GetPlaceableVoxels().OrderBy(x => x.DisplayName))
            {
                //Already built the button, so skip it.
                if (instancedButtons.ContainsKey(voxel))
                    continue;

                List<GameObject> page = currentPage;

                if (counter % maxIconsPerPage == 0)
                {
                    page = new List<GameObject>();
                    pages.Add(page);
                    currentPage = page;
                }

                VoxelMenuButton b = BuildButton(voxel);
                page.Add(b.gameObject);
                b.gameObject.SetActive(this.currentPage == pages.Count-1);
                instancedButtons.Add(voxel, b);

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
        
        private void Button_Worlds()
        {
            container.SetActive(false);
            savesMenu.Open();
        }

        private void Update()
        {
            if (!IsOpen)
                return;

            if (Input.GetKeyDown(KeyCode.Escape))
                CloseMenu();

            if (Input.GetKeyDown(KeyCode.F5))
                RebuildMenu();
        }

        public void OpenMenu() 
        {
            InitializeReferences();

            RebuildMenu();
            container.SetActive(true);

            saveButton.gameObject.SetActive(VoxelWorld.IsWorldDirty());

            SetPage(currentPage);
            Pauser.Pause(container);
        }

        public void SetPage(int index)
        {
            index = Mathf.Clamp(index, 0, pages.Count-1);
            currentPage = index;

            for(int i = 0; i < pages.Count; i++)
            {
                for(int j = 0; j < pages[i].Count; j++)
                {
                    pages[i][j].SetActive(i == currentPage);
                }
            }

            pageNumberLabel.text = $"{currentPage+1}/{(pages.Count)}";
        }

        public void NextPage(int index = 1)
        {
            SetPage(currentPage + index);
        }

        public void CloseMenu()
        {
            InitializeReferences();

            if (savesMenu.gameObject.activeInHierarchy)
            {
                savesMenu.EscapeAction();
                return;
            }

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

            pages.Clear();
            instancedButtons.Clear();
        }

        public void Button_DeselectHeldVoxel()
        {
            voxelHand?.SetHeldVoxel(null);
        }

        public void Button_RefreshVoxels()
        {
            if (VoxelDatabase.IsImportingTextures)
                return;

            VoxelDatabase.ImportCustomBlocksAsync();
        }

        public void Button_OpenCustomVoxelFolder()
        {
            VoxelDatabase.OpenCustomVoxelFolder();
        }

        public void NewWorld()
        {
            OpenMenu();
            Button_Worlds();
            savesMenu.NewFile();
        }

        public void Button_ClearAllVoxels()
        {
            ModalDialogue.ShowSimple("Are you sure?", "You are about to clear everything, this is irreversable! Are you sure you want to do this?", (confirm) =>
            {
                if (confirm)
                    VoxelWorld.ClearBlocks();
            });
        }
    }
}
