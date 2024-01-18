using Configgy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace UltraFunGuns
{
    //TODO make the asset for this.
    public class VoxelSavesMenu : MonoBehaviour
    {
        private GameObject listViewPanel;

        public Button newButton;
        public Button saveButton;
        public Button loadButton;
        public Button editButton;
        public Button openInFolderButton;
        public Button deleteButton;
        public Button discordButton;
        public Button confirmButton;
        public Button cancelButton;
        public Button returnButton;
        private Button[] SidePanelButtons;

        private RectTransform listViewContentBody;

        private VoxelFileInspector inspector;
        private VoxelSelectionMenu selectionMenu;
        public VoxelWorldFileHeader SelectedFile { get; private set; }

        [SerializeField] private GameObject buttonFilePrefab;

        private List<FileSelecitonButton> instancedButtons = new List<FileSelecitonButton>();

        private void Awake()
        {
            InitializeReferences();
        }

        private bool initialized;

        private void InitializeReferences()
        {
            if (initialized)
                return;

            selectionMenu = GetComponentInParent<VoxelSelectionMenu>();

            inspector = GetComponentInChildren<VoxelFileInspector>(true);
            inspector.gameObject.SetActive(false);

            listViewPanel = LocateComponent<RectTransform>("ListView").gameObject;
            listViewContentBody = LocateComponent<RectTransform>("ListView_ContentBody");

            returnButton = LocateComponent<Button>("Button_Return");
            returnButton.SetClickAction(() =>
            {
                inspector.Close();
                gameObject.SetActive(false);
            });

            cancelButton = LocateComponent<Button>("Button_Cancel");
            confirmButton = LocateComponent<Button>("Button_Confirm");
            newButton = LocateComponent<Button>("Button_New");
            saveButton = LocateComponent<Button>("Button_Save");
            loadButton = LocateComponent<Button>("Button_Load");
            editButton = LocateComponent<Button>("Button_Edit");
            openInFolderButton = LocateComponent<Button>("Button_OpenInFolder");
            deleteButton = LocateComponent<Button>("Button_Delete");
            discordButton = LocateComponent<Button>("Button_Discord");
            discordButton.onClick.RemoveAllListeners();
            discordButton.onClick.AddListener(() =>
            {
                Application.OpenURL(ConstInfo.DISCORD_URL);
            });

            SidePanelButtons = new Button[]
            {
                returnButton,
                cancelButton,
                confirmButton,
                newButton,
                saveButton,
                loadButton,
                editButton,
                openInFolderButton,
                deleteButton,
                discordButton
            };

            initialized = true;
        }

        private void SelectEntry(VoxelWorldFileHeader header, FileSelecitonButton button)
        {
            foreach (FileSelecitonButton b in instancedButtons)
            {
                b.IsSelected = (button == b);
                b.RefreshValues();
            }

            DisableAllSideButtons();
            NonSelectedButtons();

            editButton.gameObject.SetActive(true);
            editButton.SetClickAction(() =>
            {
                listViewPanel.SetActive(false);
                inspector.OpenWithFile(header);
            });

            bool isLoaded = VoxelWorld.CurrentFile != null && VoxelWorld.CurrentFile.Header.FilePath == header.FilePath;

            saveButton.gameObject.SetActive(isLoaded);
            saveButton.SetClickAction(() =>
            {
                VoxelWorld.SaveCurrentWorld();
            });

            loadButton.gameObject.SetActive(true);
            loadButton.SetClickAction(() =>
            {
                Load(header);
            });

            openInFolderButton.gameObject.SetActive(true);
            openInFolderButton.SetClickAction(() =>
            {
                Application.OpenURL($"file://{Path.GetDirectoryName(header.FilePath)}");
            });

            deleteButton.gameObject.SetActive(true);
            deleteButton.SetClickAction(() =>
            {
                Delete(header);
            });
        }

        public void DeselectAll()
        {
            foreach (FileSelecitonButton b in instancedButtons)
            {
                b.IsSelected = false;
                b.RefreshValues();
            }
            DisableAllSideButtons();
            NonSelectedButtons();
        }

        public void RebuildList()
        {
            ClearList();
            PopulateList(VoxelSaveManager.LoadHeaders());
        }

        private void ClearList()
        {
            foreach (FileSelecitonButton button in instancedButtons)
            {
                button.Dispose();
            }

            instancedButtons.Clear();
        }
        
        private void UpdateList()
        {
            foreach (FileSelecitonButton button in instancedButtons)
            {
                button.RefreshValues();
            }
        }

        private void PopulateList(IEnumerable<VoxelWorldFileHeader> headers)
        {
            foreach (VoxelWorldFileHeader header in headers.OrderBy(x=>x.DisplayName))
            {
                GameObject newButton = Instantiate(buttonFilePrefab, listViewContentBody);
                FileSelecitonButton fileButton = new FileSelecitonButton(header, newButton);
                fileButton.RefreshValues();
                fileButton.Button.SetClickAction(() =>
                {
                    SelectEntry(header, fileButton);
                });

                instancedButtons.Add(fileButton);
            }
        }

        public void DisableAllSideButtons()
        {
            for(int i = 0; i < SidePanelButtons.Length; i++)
            {
                SidePanelButtons[i].gameObject.SetActive(false);
            }
        }

        private T LocateComponent<T>(string name) where T : Component
        {
            return transform.GetComponentsInChildren<T>().Where(x => x.name == name).FirstOrDefault();
        }

        public void Load(VoxelWorldFileHeader header)
        {
            bool worldDirty = VoxelWorld.IsWorldDirty();

            Action loadConfirmation = () =>
            {
                ModalDialogue.ShowSimple($"Load {header.DisplayName}?", $"Loading {header.DisplayName} will clear all blocks in the current environment. Are you sure?", (confirm) =>
                {
                    if (confirm)
                    {
                        VoxelWorldFile data = VoxelSaveManager.LoadAtFilePath(header.FilePath);
                        VoxelWorld.LoadWorld(data);
                        RebuildList();
                    }
                });
            };

            if (worldDirty)
            {
                ModalDialogue.ShowComplex("Unsaved Changes", "Your voxel world has unsaved changes. What would you like to do?",
                    new DialogueBoxOption()
                    {
                        Color = Color.green,
                        Name = "Save",
                        OnClick = () =>
                        {
                            VoxelWorld.SaveCurrentWorld();
                            loadConfirmation();
                        }
                    },
                    new DialogueBoxOption()
                    {
                        Color = Color.red,
                        Name = "Discard Changes",
                        OnClick = loadConfirmation
                    },
                    new DialogueBoxOption()
                    {
                        Color = Color.white,
                        Name = "Cancel",
                        OnClick = () => {}
                    }
                    );
            }
            else
            {
                loadConfirmation();
            }
        }

        public void Delete(VoxelWorldFileHeader header, Action<bool> callBack = null)
        {
            ModalDialogue.ShowSimple($"Delete {header.DisplayName}?", $"Are you sure you want to delete {header.DisplayName}?", (confirm) =>
            {
                if (confirm)
                {
                    VoxelSaveManager.DeleteFile(header);
                    DeselectAll();
                    RebuildList();
                }

                callBack?.Invoke(confirm);
            });
        }

        public void Open()
        {
            gameObject.SetActive(true);
            Pauser.Pause(gameObject);
            InitializeReferences();
            DeselectAll();
            listViewPanel.SetActive(true);

            if(inspector.gameObject.activeInHierarchy)
                inspector.Close();

            RebuildList();
        }

        public void NewFile()
        {
            InitializeReferences();

            listViewPanel.SetActive(false);
            inspector.Open();
            inspector.CreationMode();
        }

        public void Close()
        {
            DeselectAll();

            listViewPanel.SetActive(true);
            if (inspector.gameObject.activeInHierarchy)
                inspector.Close();

            gameObject.SetActive(false);
            selectionMenu.OpenMenu();
        }

        public void EscapeAction()
        {
            if (inspector.gameObject.activeInHierarchy)
            {
                inspector.Close();
                Open();
                return;
            }

            Close();
        }

        private void NonSelectedButtons()
        {
            DisableAllSideButtons();

            openInFolderButton.gameObject.SetActive(true);
            openInFolderButton.SetClickAction(() =>
            {
                Application.OpenURL($"file://{Paths.VoxelSavesFolder}");
            });

            returnButton.gameObject.SetActive(true);
            returnButton.SetClickAction(() =>
            {
                listViewPanel.SetActive(true);
                gameObject.SetActive(false);
                selectionMenu.OpenMenu();
            });

            newButton.gameObject.SetActive(true);
            newButton.SetClickAction(() =>
            {
                NewFile();
            });

            discordButton.gameObject.SetActive(true);
        }

        public void OpenWithFile(VoxelWorldFileHeader header)
        {
            this.SelectedFile = header;
            inspector.OpenWithFile(SelectedFile);
        }

        public class FileSelecitonButton : IDisposable
        {
            public Button Button { get; }
            public VoxelWorldFileHeader Header { get; }
            public Image Frame { get; }
            public Text WorldNameText { get; }
            public Text FileNameText { get; }
            public Text VoxelCountText { get; }
            public GameObject GameObject { get; }

            private Color defaultFrameColor;
            private static readonly Color orange = new Color(1, 0.8f, 0);

            public bool IsSelected;

            private bool IsLoaded()
            {
                return VoxelWorld.CurrentFile != null && VoxelWorld.CurrentFile.Header.FilePath == Header.FilePath;
            }

            public FileSelecitonButton(VoxelWorldFileHeader header, GameObject gameObject)
            {
                Header = header;
                GameObject = gameObject;
                Button = gameObject.GetComponent<Button>();
                Frame = gameObject.GetComponentsInChildren<Image>(true).Where(x => x.name == "Border").FirstOrDefault();
                defaultFrameColor = Frame.color;
                WorldNameText = gameObject.GetComponentsInChildren<Text>(true).Where(x => x.name == "Text_WorldName").FirstOrDefault();
                FileNameText = gameObject.GetComponentsInChildren<Text>(true).Where(x => x.name == "Text_FileName").FirstOrDefault();
                VoxelCountText = gameObject.GetComponentsInChildren<Text>(true).Where(x => x.name == "Text_VoxelCount").FirstOrDefault();
            }

            public void RefreshValues()
            {
                WorldNameText.text = Header.DisplayName;
                FileNameText.text = Path.GetFileNameWithoutExtension(Header.FilePath);
                VoxelCountText.text = Header.TotalVoxelCount.ToString("N0") + " VOXELS";

                Color frameColor = defaultFrameColor;

                if (IsLoaded())
                    frameColor = Color.green;

                if (IsSelected)
                    frameColor = orange;

                Frame.color = frameColor;
            }

            public void Dispose()
            {
                GameObject.Destroy(GameObject);
            }
        }
    }
}
