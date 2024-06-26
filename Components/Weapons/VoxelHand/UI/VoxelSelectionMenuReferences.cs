﻿using UnityEngine;
using UnityEngine.UI;

namespace UltraFunGuns
{
    public class VoxelSelectionMenuReferences : MonoBehaviour
    {
        //I HATE UNITY

        [SerializeField] private GameObject container;
        public GameObject Container => container;
        
        [SerializeField] private RectTransform contentBody;
        public RectTransform ContentBody => contentBody;

        [SerializeField] private GameObject selectionButtonPrefab;
        public GameObject SelectionButtonPrefab => selectionButtonPrefab;

        [SerializeField] private Button importButton;
        public Button ImportButton => importButton;

        [SerializeField] private Button worldsButton;
        public Button WorldsButton => worldsButton;

        [SerializeField] private Button saveButton;
        public Button SaveButton => saveButton;

        [SerializeField] private Button openVoxelFolderButton;
        public Button OpenVoxelFolderButton => openVoxelFolderButton;

        [SerializeField] private Button clearAllVoxelsButton;
        public Button ClearAllVoxelsButton => clearAllVoxelsButton;

        [SerializeField] private Button refreshVoxels;
        public Button RefreshVoxels => refreshVoxels;


        [SerializeField] private Text importButtonText;
        public Text ImportButtonText => importButtonText;
        
        [SerializeField] private Text pageNumberLabel;
        public Text PageNumberLabel => pageNumberLabel;
    }
}
