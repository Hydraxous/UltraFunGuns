using Configgy;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace UltraFunGuns
{
    //TODO make the asset for this.
    public class VoxelSavesMenu : MonoBehaviour
    {
        
        public Button newButton;
        public Button saveButton;
        public Button loadButton;
        public Button editButton;
        public Button openInFolderButton;
        public Button deleteButton;
        public Button discordButton;


        private void Awake()
        {
            newButton = LocateComponent<Button>("Button_New");
            saveButton = LocateComponent<Button>("Button_Save");
            loadButton = LocateComponent<Button>("Button_Load");
            editButton = LocateComponent<Button>("Button_Edit");
            openInFolderButton = LocateComponent<Button>("Button_OpenInFolder");
            deleteButton = LocateComponent<Button>("Button_Delete");
            discordButton = LocateComponent<Button>("Button_Discord");
        }

        private T LocateComponent<T>(string name) where T : Component
        {
            return transform.GetComponentsInChildren<T>().Where(x => x.name == name).FirstOrDefault();
        }

        public void CreateNewAndSave()
        {

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
    }
}
