using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace UltraFunGuns
{
    public class VoxelFileInspector : MonoBehaviour
    {
        private InputField fileNameInputField;
        private InputField worldDisplayNameInputField;
        private InputField descriptionInputField;

        private Text voxelCountText;

        private GameObject fileNameWarningPanel;
        private Text fileNameWarningText;

        private VoxelSavesMenu menu;
        private VoxelWorldFileHeader target;

        public void OpenWithFile(VoxelWorldFileHeader header)
        {
            InitializeReferences();
            SelectFile(header);
            Open();

            menu.DisableAllSideButtons();
            menu.saveButton.onClick.RemoveAllListeners();
            menu.returnButton.gameObject.SetActive(true);

            menu.returnButton.SetClickAction(() =>
            {
                Close();
                menu.Open();
            });

            menu.openInFolderButton.gameObject.SetActive(true);
            menu.openInFolderButton.SetClickAction(() =>
            {
                Application.OpenURL("file://" + Path.GetDirectoryName(header.FilePath));
            });

            menu.deleteButton.gameObject.SetActive(true);
            menu.deleteButton.SetClickAction(() =>
            {
                menu.Delete(header, (r) =>
                {
                    Close();
                    menu.Open();
                });
            });

            menu.loadButton.gameObject.SetActive(true);
            menu.loadButton.SetClickAction(() =>
            {
                menu.Load(header);
            });
        }

        public void CreationMode()
        {
            InitializeReferences();
            VoxelWorldFileHeader newHeader = new VoxelWorldFileHeader();

            string newName = "NewWorld";
            int nameIndex = 0;

            while (File.Exists(VoxelSaveManager.NameToFilePath(newName + ((nameIndex > 0) ? $" ({nameIndex})" : ""))))
                ++nameIndex;

            newHeader.FilePath = newName + ((nameIndex > 0) ? $" ({nameIndex})" : "");
            newHeader.DisplayName = "New World";
            newHeader.Description = "A new world";
            newHeader.SceneName = SceneHelper.CurrentScene;
            newHeader.TotalVoxelCount = 0;
            newHeader.WorldScale = VoxelWorld.WorldScale;
            newHeader.ModVersion = ConstInfo.VERSION;
            newHeader.GameVersion = Application.version;

            SelectFile(newHeader);
            menu.DisableAllSideButtons();

            menu.confirmButton.gameObject.SetActive(true);
            menu.confirmButton.SetClickAction(() =>
            {
                VoxelWorldFile file = new VoxelWorldFile();
                file.VoxelData = new SerializedVoxel[0];

                //use existing or something
                if (VoxelWorld.CurrentFile != null)
                {
                    file = VoxelWorld.CurrentFile;
                }

                file.Header = target;
                VoxelSaveManager.SaveWorldData(file.Header.FilePath, file);
                Close();
                menu.Open();
            });

        }

        private void SelectFile(VoxelWorldFileHeader header)
        {
            if (header == null)
                return;

            string newDisplayName = header.DisplayName;
            string newDescription = header.Description;
            currentSaveName = Path.GetFileNameWithoutExtension(header.FilePath);

            Func<bool> checkDirty = () =>
            {
                return newDisplayName != header.DisplayName ||
                newDescription != header.Description ||
                currentSaveName != Path.GetFileNameWithoutExtension(header.FilePath);
            };

            this.target = header; 
            fileNameInputField.text = Path.GetFileNameWithoutExtension(header.FilePath);
            fileNameInputField.onEndEdit.RemoveAllListeners();
            fileNameInputField.onValueChanged.RemoveAllListeners();
            fileNameInputField.onValueChanged.AddListener(OnInputFieldChanged);
            fileNameInputField.onEndEdit.AddListener(OnEndEdit);

            worldDisplayNameInputField.text = header.DisplayName;
            worldDisplayNameInputField.onEndEdit.RemoveAllListeners();
            worldDisplayNameInputField.onEndEdit.AddListener((v) => 
            {
                newDisplayName = v;
                menu.saveButton.gameObject.SetActive(checkDirty());
            });


            descriptionInputField.text = header.Description;
            descriptionInputField.onEndEdit.RemoveAllListeners();
            descriptionInputField.onEndEdit.AddListener((v) =>
            {
                newDescription = v;
                menu.saveButton.gameObject.SetActive(checkDirty());
            });

            voxelCountText.text = header.TotalVoxelCount.ToString("N0") + " VOXELS";
        }

        public void Open()
        {
            this.gameObject.SetActive(true);
        }

        public void Close()
        {
            this.gameObject.SetActive(false);
            target = null;
        }

        const int MAX_SAVE_NAME_CHARACTERS = 32;
        string currentSaveName = "";

        private void Awake()
        {
            InitializeReferences();
        }

        private bool initialized = false;
        private void InitializeReferences()
        {
            if (initialized)
                return;

            menu = GetComponentInParent<VoxelSavesMenu>();
            fileNameInputField = LocateComponent<InputField>("InputField_FileName");
            descriptionInputField = LocateComponent<InputField>("InputField_WorldDescription");
            voxelCountText = LocateComponent<Text>("Text_VoxelCount");
            worldDisplayNameInputField = LocateComponent<InputField>("InputField_WorldName");

            fileNameWarningPanel = LocateComponent<RectTransform>("Panel_FileNameWarning").gameObject;
            fileNameWarningText = LocateComponent<Text>("Text_FileNameWarning");
            fileNameWarningPanel.SetActive(false);

            if (fileNameInputField != null)
            {
                fileNameInputField.onValueChanged.AddListener(OnInputFieldChanged);
                fileNameInputField.onEndEdit.AddListener(OnEndEdit);
                fileNameInputField.characterLimit = MAX_SAVE_NAME_CHARACTERS;
                fileNameInputField.SetTextWithoutNotify("");
            }

            initialized = true;
        }

        private T LocateComponent<T>(string name) where T : Component
        {
            return transform.GetComponentsInChildren<T>().Where(x => x.name == name).FirstOrDefault();
        }

        private void OnInputFieldChanged(string value)
        {
            CheckName(value);
        }

        private void OnEndEdit(string value)
        {
            if (CheckName(value))
                currentSaveName = value;
            else
                currentSaveName = "";

            fileNameInputField.SetTextWithoutNotify(currentSaveName);
        }

        private bool CheckName(string name)
        {
            bool nameValid = TryValidateSaveName(name, out string errorMessage);
            menu.saveButton.interactable = nameValid;
            fileNameWarningPanel.SetActive(!nameValid);
            if (!nameValid)
            {
                fileNameWarningText.text = errorMessage;
            }

            return nameValid;
        }

        private bool TryValidateSaveName(string saveName, out string errorMessage)
        {
            errorMessage = "";

            if (string.IsNullOrEmpty(saveName) || string.IsNullOrWhiteSpace(saveName))
            {
                errorMessage = "Save name cannot be empty or whitespace";
                return false;
            }

            if (saveName.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
            {
                errorMessage = "Save name contains invalid characters";
                return false;
            }

            //How?? Hacking!!!!
            if (saveName.Length > MAX_SAVE_NAME_CHARACTERS)
            {
                errorMessage = $"Save name is too long. Max {MAX_SAVE_NAME_CHARACTERS} chars. Literally, how did you even manage to do this?";
                return false;
            }

            if (VoxelSaveManager.Exists(saveName))
            {
                errorMessage = "A save with that name already exists.";
                return false;
            }

            return true;
        }


    }
}
