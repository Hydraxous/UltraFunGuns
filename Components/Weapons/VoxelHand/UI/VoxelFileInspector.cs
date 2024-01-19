using Configgy;
using Newtonsoft.Json.Linq;
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
            VoxelWorldFileHeader header = new VoxelWorldFileHeader();
            this.target = header;
            string newName = "NewWorld";
            int nameIndex = 0;

            while (File.Exists(VoxelSaveManager.NameToFilePath(newName + ((nameIndex > 0) ? $" ({nameIndex})" : ""))))
                ++nameIndex;

            header.FilePath = VoxelSaveManager.NameToFilePath(newName + ((nameIndex > 0) ? $" ({nameIndex})" : ""));
            header.DisplayName = "New World";
            header.Description = "A new world";
            header.SceneName = SceneHelper.CurrentScene;
            header.TotalVoxelCount = 0;
            header.WorldScale = VoxelWorld.WorldScale;
            header.ModVersion = ConstInfo.VERSION;
            header.GameVersion = Application.version;

            if(menu == null)
                menu = GetComponentInParent<VoxelSavesMenu>();

            SelectFile(header);
            menu.DisableAllSideButtons();

            menu.confirmButton.gameObject.SetActive(true);
            menu.confirmButton.SetClickAction(() =>
            {
                VoxelWorldFile file = null;

                //use existing or something
                if (VoxelWorld.CurrentFile != null)
                {
                    file = VoxelWorld.CurrentFile;
                }
                else
                {
                    file = new VoxelWorldFile();
                    VoxelWorld.SetCurrentFile(file);
                }

                file.VoxelData = VoxelWorld.SerializeCurrentVoxels();
                file.Header = target;
                
                VoxelSaveManager.SaveWorldData(file.Header.FilePath, file);
                Close();
                menu.Open();
            });

            menu.cancelButton.gameObject.SetActive(true);
            menu.cancelButton.SetClickAction(() =>
            {
                Close();
                menu.Open();
            });

            //guh this is still broken TODO
            fileNameInputField.SetTextWithoutNotify(Path.GetFileNameWithoutExtension(header.FilePath));
            fileNameInputField.onEndEdit.RemoveAllListeners();
            fileNameInputField.onValueChanged.RemoveAllListeners();
            fileNameInputField.onValueChanged.AddListener((v) =>
            {
                bool validPath = CheckName(v, Path.GetFileNameWithoutExtension(header.FilePath));

            });

            fileNameInputField.onEndEdit.AddListener((v) =>
            {
                bool nameValid = CheckName(v, Path.GetFileNameWithoutExtension(header.FilePath));
                if (nameValid)
                {
                    header.FilePath = VoxelSaveManager.NameToFilePath(v);
                }

                menu.confirmButton.interactable = nameValid;
            });


            worldDisplayNameInputField.text = header.DisplayName;
            worldDisplayNameInputField.onEndEdit.RemoveAllListeners();
            worldDisplayNameInputField.onEndEdit.AddListener((v) =>
            {
                header.DisplayName = v;
            });


            descriptionInputField.text = header.Description;
            descriptionInputField.onEndEdit.RemoveAllListeners();
            descriptionInputField.onEndEdit.AddListener((v) =>
            {
                header.Description = v;
            });
        }

        private void SelectFile(VoxelWorldFileHeader header)
        {
            if (header == null)
                return;

            string newDisplayName = header.DisplayName;
            string newDescription = header.Description;
            string newFileName = Path.GetFileNameWithoutExtension(header.FilePath);

            Func<bool> checkDirty = () =>
            {
                //New file creation.
                if(!File.Exists(header.FilePath))
                {
                    return false;
                }

                return newDisplayName != header.DisplayName ||
                newDescription != header.Description ||
                newFileName != Path.GetFileNameWithoutExtension(header.FilePath);
            };

            this.target = header; 
            fileNameInputField.SetTextWithoutNotify(Path.GetFileNameWithoutExtension(header.FilePath));
            fileNameInputField.onEndEdit.RemoveAllListeners();
            fileNameInputField.onValueChanged.RemoveAllListeners();
            fileNameInputField.onValueChanged.AddListener((v) =>
            {
                CheckName(v, Path.GetFileNameWithoutExtension(header.FilePath));
            });

            fileNameInputField.onEndEdit.AddListener((v) =>
            {
                bool nameValid = CheckName(v, Path.GetFileNameWithoutExtension(header.FilePath));

                if (nameValid)
                {
                    newFileName = v;
                }

                menu.saveButton.interactable = nameValid;
                fileNameInputField.SetTextWithoutNotify(v);
                menu.saveButton.gameObject.SetActive(checkDirty());
            });


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

            menu.saveButton.SetClickAction(() =>
            {

                if(newDescription.Length > MAX_DESCRIPTION_CHARACTERS)
                    newDescription = newDescription.Substring(0, MAX_DESCRIPTION_CHARACTERS);

                header.Description = newDescription;

                if(newDisplayName.Length > MAX_WORLD_NAME_CHARACTERS)
                    newDisplayName = newDisplayName.Substring(0, MAX_SAVE_NAME_CHARACTERS);

                header.DisplayName = newDisplayName;

                VoxelSaveManager.UpdateHeaderFile(header);

                if (newFileName != Path.GetFileNameWithoutExtension(header.FilePath))
                {
                    VoxelSaveManager.RenameFile(header, VoxelSaveManager.NameToFilePath(newFileName));
                }

                menu.saveButton.gameObject.SetActive(checkDirty());
            });

            voxelCountText.text = header.TotalVoxelCount.ToString("N0") + " VOXELS";
        }

        public void Open()
        {
            gameObject.SetActive(true);
            Pauser.Pause(gameObject);
        }

        public void Close()
        {
            gameObject.SetActive(false);
            target = null;
        }

        const int MAX_SAVE_NAME_CHARACTERS = 32;
        const int MAX_WORLD_NAME_CHARACTERS = 48;
        const int MAX_DESCRIPTION_CHARACTERS = 2048;

        private void Awake()
        {
            InitializeReferences();
        }

        public void SetMenu(VoxelSavesMenu menu)
        {
            this.menu = menu;
        }

        private bool initialized = false;
        private void InitializeReferences()
        {
            if (initialized)
                return;

            if(menu == null)
                menu = GetComponentInParent<VoxelSavesMenu>();

            fileNameInputField = LocateComponent<InputField>("InputField_FileName");
            

            descriptionInputField = LocateComponent<InputField>("InputField_WorldDescription");
            descriptionInputField.characterLimit = MAX_DESCRIPTION_CHARACTERS;

            voxelCountText = LocateComponent<Text>("Text_VoxelCount");

            worldDisplayNameInputField = LocateComponent<InputField>("InputField_WorldName");
            worldDisplayNameInputField.characterLimit = MAX_SAVE_NAME_CHARACTERS;

            fileNameWarningPanel = LocateComponent<RectTransform>("Panel_FileNameWarning").gameObject;
            fileNameWarningText = LocateComponent<Text>("Text_FileNameWarning");
            fileNameWarningPanel.SetActive(false);

            if (fileNameInputField != null)
            {
                fileNameInputField.characterLimit = MAX_SAVE_NAME_CHARACTERS;
                fileNameInputField.SetTextWithoutNotify("");
            }

            initialized = true;
        }

        private T LocateComponent<T>(string name) where T : Component
        {
            return transform.GetComponentsInChildren<T>().Where(x => x.name == name).FirstOrDefault();
        }

        private bool CheckName(string name, string oldName)
        {
            bool nameValid = TryValidateSaveName(name, out string errorMessage);

            if (name == oldName)
                nameValid = true;

            
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
            
            if (saveName == ":3")
            {
                errorMessage = "no :3";
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
