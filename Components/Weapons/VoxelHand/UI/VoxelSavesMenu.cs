using Configgy;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace UltraFunGuns
{
    //TODO make the asset for this.
    public class VoxelSavesMenu : MonoBehaviour
    {
        [SerializeField] private InputField saveNameInputField;
        [SerializeField] private GameObject textWarningPopupObject;
        [SerializeField] private Text textWarningPopupText;
        [SerializeField] private Button saveButton;

        const int MAX_SAVE_NAME_CHARACTERS = 32;

        private void Awake()
        {
            if (saveNameInputField != null)
            {
                saveNameInputField.onValueChanged.AddListener(OnInputFieldChanged);
                saveNameInputField.onEndEdit.AddListener(OnEndEdit);
                saveNameInputField.characterLimit = MAX_SAVE_NAME_CHARACTERS;
                saveNameInputField.SetTextWithoutNotify("");
            }

        }

        string currentSaveName = "";

        private void OnInputFieldChanged(string value)
        {
            CheckName(value);
        }

        private void OnEndEdit(string value)
        {
            if(CheckName(value))
                currentSaveName = value;
            else
                currentSaveName = "";

            saveNameInputField.SetTextWithoutNotify(currentSaveName);
        }

        private bool CheckName(string name)
        {
            bool nameValid = TryValidateSaveName(name, out string errorMessage);
            saveButton.interactable = nameValid;
            textWarningPopupObject.SetActive(!nameValid);
            if (!nameValid)
            {
                textWarningPopupText.text = errorMessage;
            }

            return nameValid;
        }

        private bool TryValidateSaveName(string saveName, out string errorMessage)
        {
            errorMessage = "";

            if(string.IsNullOrEmpty(saveName) || string.IsNullOrWhiteSpace(saveName))
            {
                errorMessage = "Save name cannot be empty or whitespace";
                return false;
            }

            if(saveName.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
            {
                errorMessage = "Save name contains invalid characters";
                return false;
            }

            //How?? Hacking!!!!
            if (saveName.Length > MAX_SAVE_NAME_CHARACTERS)
            {
                errorMessage = $"Save name is too long. Max {MAX_SAVE_NAME_CHARACTERS} chars";
                return false;
            }

            string endFilePath = VoxelSaveManager.NameToFilePath(saveName);
            if (File.Exists(endFilePath))
            {
                errorMessage = "A save with that name already exists.";
                return false;
            }

            return true;
        }

        public void Save()
        {

        }

        public void CreateNewAndSave()
        {
            VoxelWorld.SaveWorld(currentSaveName);
        }

        public void Load(VoxelWorldFile data)
        {
            bool worldDirty = VoxelWorld.IsWorldDirty();

            Action loadConfirmation = () =>
            {
                ModalDialogue.ShowSimple($"Load {data.Header.DisplayName}?", $"Loading {data.Header.DisplayName} will clear all blocks in the current environment. Are you sure?", (confirm) =>
                {
                    if (confirm)
                        VoxelWorld.LoadWorld(data);

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
                            VoxelWorld.SaveWorld(saveNameInputField.text);
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
    }
}
