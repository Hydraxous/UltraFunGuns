using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UltraFunGuns
{
    public static class UKAPIP
    {
        private static bool initialized;
        public static void Init()
        {
            if(!initialized)
            {
                SceneManager.sceneLoaded += OnSceneLoad;
            }
        }

        /// <summary>
        /// Enumerated version of the Ultrakill scene types
        /// </summary>
        public enum UKLevelType { Intro, MainMenu, Level, Endless, Sandbox, Custom, Intermission, Unknown }

        /// <summary>
        /// Returns the current level type
        /// </summary>
        public static UKLevelType CurrentLevelType = UKLevelType.Intro;

        public delegate void OnLevelChangedHandler(UKLevelType uKLevelType);

        /// <summary>
        /// Invoked whenever the current level type is changed.
        /// </summary>
        public static OnLevelChangedHandler OnLevelTypeChanged;

        /// <summary>
        /// Invoked whenever the scene is changed.
        /// </summary>
        public static OnLevelChangedHandler OnLevelChanged;

        //Perhaps there is a better way to do this.
        private static void OnSceneLoad(Scene scene, LoadSceneMode loadSceneMode)
        {
            string sceneName = scene.name;
            UKLevelType newScene = GetUKLevelType(sceneName);

            if (newScene != CurrentLevelType)
            {
                CurrentLevelType = newScene;
                OnLevelTypeChanged?.Invoke(newScene);
            }

            OnLevelChanged?.Invoke(CurrentLevelType);
        }

        //Perhaps there is a better way to do this. Also this will most definitely cause problems in the future if PITR or Hakita rename any scenes.

        /// <summary>
        /// Gets enumerated level type from the name of a scene.
        /// </summary>
        /// <param name="sceneName">Name of the scene</param>
        /// <returns></returns>
        public static UKLevelType GetUKLevelType(string sceneName)
        {
            sceneName = (sceneName.Contains("Level")) ? "Level" : (sceneName.Contains("Intermission")) ? "Intermission" : sceneName;

            switch (sceneName)
            {
                case "Main Menu":
                    return UKLevelType.MainMenu;
                case "Custom Content":
                    return UKLevelType.Custom;
                case "Intro":
                    return UKLevelType.Intro;
                case "Endless":
                    return UKLevelType.Endless;
                case "uk_construct":
                    return UKLevelType.Sandbox;
                case "Intermission":
                    return UKLevelType.Intermission;
                case "Level":
                    return UKLevelType.Level;
                default:
                    return UKLevelType.Unknown;
            }
        }

        /// <summary>
        /// Returns true if the current scene is playable
        /// </summary>
        /// <returns></returns>
        public static bool InLevel()
        {
            bool inNonPlayable = (CurrentLevelType == UKLevelType.MainMenu || CurrentLevelType == UKLevelType.Intro || CurrentLevelType == UKLevelType.Intermission || CurrentLevelType == UKLevelType.Unknown);
            return !inNonPlayable;
        }
    }
}
