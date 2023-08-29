using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace UltraFunGuns.UI
{
    public class ConfigurationPage : MonoBehaviour
    {
        public ConfigurationPage Parent { get; private set; }

        [SerializeField] private Text header;
        [SerializeField] private Text footer;
        [SerializeField] private Button backButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private RectTransform contentBody;
        
        private List<IConfigElement> elements = new List<IConfigElement>();

        public void SetParent(ConfigurationPage page)
        { 
            Parent = page; 
        }

        public void SetHeader(string headerText)
        {
            this.header.text = $"-- {headerText} --";
        }

        public void SetFooter(string footerText) 
        {
            this.footer.text = footerText.TrimEnd('/', '\n', '\r');
        }

        public void AddElement(IConfigElement configElement)
        {
            elements.Add(configElement);
        }

        public void ClearElements()
        {
            elements.Clear();
            DestroyAllElements();
        }

        private void DestroyAllElements()
        {
            Transform[] transforms = contentBody.GetChildren();

            for (int i = 0; i < transforms.Length; i++)
            {
                Transform t = transforms[i];
                transforms[i] = null;
                Destroy(t.gameObject);
            }
        }

        public void Open()
        {
            gameObject.SetActive(true);
        }

        public void Back()
        {
            if (Parent != null)
                Parent.Open();

            gameObject.SetActive(false);
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }

        public void RebuildPage()
        {
            DestroyAllElements();

            foreach(IConfigElement configElement in elements.OrderBy(x=>x.GetDescriptor()?.OrderInList))
            {
                configElement.BuildElement(contentBody);
            }
        }

        private void OnEnable()
        {
            backButton.gameObject.SetActive(Parent != null);
            foreach (IConfigElement configElement in elements.OrderBy(x => x.GetDescriptor()?.OrderInList))
            {
                configElement.OnMenuOpen();
            }
        }

        private void OnDisable()
        {
            foreach (IConfigElement configElement in elements.OrderBy(x => x.GetDescriptor()?.OrderInList))
            {
                configElement.OnMenuClose();
            }
        }
    }
}
