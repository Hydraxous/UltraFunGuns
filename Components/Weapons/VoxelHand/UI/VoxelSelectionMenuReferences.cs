using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
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

        [SerializeField] private Text importButtonText;
        public Text ImportButtonText => importButtonText;
        
        [SerializeField] private Text pageNumberLabel;
        public Text PageNumberLabel => pageNumberLabel;
    }
}
