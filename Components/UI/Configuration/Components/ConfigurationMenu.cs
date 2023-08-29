using HydraDynamics;
using HydraDynamics.Keybinds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UltraFunGuns.Configuration;
using UnityEngine;

namespace UltraFunGuns.UI
{
    public class ConfigurationMenu : MonoBehaviour
    {
        [SerializeField] private RectTransform contentbody;

        private ConfigurationPage rootPage;
        private Dictionary<string,ConfigurationPage> pageManifest = new Dictionary<string,ConfigurationPage>();

        private bool menuBuilt = false;

        private static Keybinding cfgKey = Hydynamics.GetKeybinding("Open Configuration Menu", KeyCode.Keypad6);

        private GameObject[] menus;

        ConfigurationPage lastOpenPage;

        private bool menuOpen = false;

        private void Awake()
        {
            if (!menuBuilt)
                BuildMenus(ConfigurationManager.GetMenus());

            ConfigurationManager.OnMenusChanged += BuildMenus;
        }

        private void Update()
        {
            if (menuOpen)
            {
                CheckMenuOpen();
                return;
            }

            if (!cfgKey.WasPerformedThisFrame)
                return;

            OpenMenu();
        }

        private void CheckMenuOpen()
        {
            if (menus == null)
                return;

            for(int i=0;i< menus.Length;i++)
            {
                if (menus[i] == null)
                    continue;

                if (menus[i].activeInHierarchy)
                    return;
            }

            //No menus are open so unpause.
            Unpause();
        }

        private void DestroyPages()
        {
            ConfigurationPage[] pages = pageManifest.Values.ToArray();

            for (int i=0;i<pages.Length;i++)
            {
                ConfigurationPage page = pages[i];
                Destroy(page.gameObject);
            }

            pageManifest.Clear();
            menuBuilt = false;
        }

        private void BuildMenus(ConfiggableMenu[] menus)
        {
            DestroyPages();

            NewPage((mainPage) =>
            {
                rootPage = mainPage;
                mainPage.SetHeader("Configuration");
                mainPage.Close();

                pageManifest.Add("", rootPage);
            });

            for (int i=0;i<menus.Length;i++)
            {
                BuildMenu(menus[i].GetConfigElements());
            }
        }

        private void BuildMenu(IConfigElement[] configElements)
        {
            if(pageManifest.ContainsKey(""))
            {
                Debug.LogWarning("Empty key is there");
            }else
            {
                Debug.LogWarning("Empty key is NOT FUCKING THERE!!!!!!!!!!");
            }

            foreach (IConfigElement configElement in configElements.OrderBy(x=>x.GetDescriptor().Path))
            {
                BuildElement(configElement);
            }

            foreach(KeyValuePair<string, ConfigurationPage> page in pageManifest)
            {
                page.Value.RebuildPage();
            }

            menuBuilt = true;
        }

        private void BuildMenuTreeFromPath(string fullPath)
        {
            string[] path = fullPath.Split('/');
            string currentPathAddress = "";

            ConfigurationPage previousPage = null;

            for (int i = 0; i < path.Length; i++)
            {
                currentPathAddress += path[i];

                if (!pageManifest.ContainsKey(currentPathAddress))
                {
                    if (previousPage == null)
                        previousPage = rootPage;

                    NewPage((page) =>
                    {
                        //Have to create a new reference because previousPage changes and it causes issues when the button is pressed.
                        ConfigurationPage closablePage = previousPage;

                        //Add button to parent page to access subpage
                        ConfigButton openSubPageButton = new ConfigButton(() =>
                        {
                            closablePage.Close();
                            page.Open();
                        }, path[i]);

                        closablePage.AddElement(openSubPageButton);

                        //Configure page
                        page.SetHeader(path[i]);
                        page.SetParent(closablePage);
                        page.gameObject.name = currentPathAddress;
                        page.SetFooter(currentPathAddress);
                        page.Close();

                        previousPage = page;
                        pageManifest.Add(currentPathAddress, page);
                    });
                }else
                {
                    previousPage = pageManifest[currentPathAddress];
                }
                
                currentPathAddress += "/";
            }
        }
        
        private void BuildElement(IConfigElement element)
        {
            Configgable descriptor = element.GetDescriptor();
            
            string path = "";

            if (descriptor != null && descriptor.Owner != null)
            {
                path = descriptor.Path;
                path = $"{descriptor.Owner.OwnerDisplayName}/"+path;
            }else
            {
                path = "/Other";
            }

            BuildMenuTreeFromPath(path);

            pageManifest[path].AddElement(element);
        }

        private void NewPage(Action<ConfigurationPage> onInstance)
        {
            GameObject newPage = GameObject.Instantiate(ConfigurationAssets.ConfigurationPage, contentbody);
            ConfigurationPage page = newPage.GetComponent<ConfigurationPage>();
            newPage.AddComponent<BehaviourRelay>().OnOnEnable += (g) => lastOpenPage = page;
            onInstance?.Invoke(page);
        }

        public void OpenMenu()
        {
            menus = transform.GetChildren().Select(x=>x.gameObject).ToArray();
         
            GameState ufgInvState = new GameState("cfg_menu", menus);
            
            ufgInvState.cursorLock = LockMode.Unlock;
            ufgInvState.playerInputLock = LockMode.Lock;
            ufgInvState.cameraInputLock = LockMode.Lock;
            ufgInvState.priority = 2;

            OptionsManager.Instance.paused = true;
            GameStateManager.Instance.RegisterState(ufgInvState);

            if (lastOpenPage != null)
                lastOpenPage.Open();
            else
                rootPage.Open();
            
            menuOpen = true;
        }

        private void Unpause()
        {
            GameStateManager.Instance.PopState("cfg_menu");
            OptionsManager.Instance.paused = false;
            menuOpen = false;
        }

        private void OnDestroy()
        {
            ConfigurationManager.OnMenusChanged -= BuildMenus;
        }
    }
}
