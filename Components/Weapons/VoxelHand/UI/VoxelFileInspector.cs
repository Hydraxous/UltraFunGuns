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
        private InputField saveNameInputField;
        private GameObject textWarningPopupObject;
        private Text textWarningPopupText;

        private VoxelSavesMenu menu;

        private VoxelWorldFile target;

        public void OpenWithFile(VoxelWorldFile file)
        {
            this.target = file;
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
            menu = GetComponentInParent<VoxelSavesMenu>();
            saveNameInputField = LocateComponent<InputField>("InputField_FileName");


            if (saveNameInputField != null)
            {
                saveNameInputField.onValueChanged.AddListener(OnInputFieldChanged);
                saveNameInputField.onEndEdit.AddListener(OnEndEdit);
                saveNameInputField.characterLimit = MAX_SAVE_NAME_CHARACTERS;
                saveNameInputField.SetTextWithoutNotify("");
            }

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

            saveNameInputField.SetTextWithoutNotify(currentSaveName);
        }

        private bool CheckName(string name)
        {
            bool nameValid = TryValidateSaveName(name, out string errorMessage);
            menu.saveButton.interactable = nameValid;
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
                errorMessage = $"Save name is too long. Max {MAX_SAVE_NAME_CHARACTERS} chars";
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
