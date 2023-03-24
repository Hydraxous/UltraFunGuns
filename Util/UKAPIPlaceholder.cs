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
                initialized = true;
                SceneManager.sceneLoaded += OnSceneLoad;
            }
        }

        private static void PrintLoadedScenes()
        {
            int numScenes = SceneManager.sceneCountInBuildSettings;
            for (int i = 0; i < numScenes; i++)
            {
                PrintSceneDetails(SceneManager.GetSceneByBuildIndex(i));
            }
        }

        private static void PrintSceneDetails(Scene scene, bool force = false)
        {
            if((scene == null || scene.buildIndex == -1) && !force)
            {
                Deboog.Log($"SCENEHELPER: Scene is null!");
                return;
            }

            Deboog.Log($"=============================\n" +
                $"SCENE FOUND: {scene.name}\n" +
                $"VALID: {scene.IsValid()}\n" +
                $"INDEX: {scene.buildIndex}\n" +
                $"PATH: {scene.path}\n" +
                $"OBJS: {scene.rootCount}\n" +
                $"=============================");

        }

        /// <summary>
        /// Enumerated version of the Ultrakill scene types
        /// </summary>
        public enum UKLevelType { Intro, MainMenu, Level, Endless, Sandbox, Credits, Custom, Intermission, Secret, PrimeSanctum, Unknown }

        /// <summary>
        /// Returns the current level type
        /// </summary>
        public static UKLevelType CurrentLevelType = UKLevelType.Intro;

        /// <summary>
        /// Returns the currently active ultrakill scene name.
        /// </summary>
        public static string CurrentSceneName = "";

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

            if (scene != SceneManager.GetActiveScene())
                return;

            UKLevelType newScene = GetUKLevelType(sceneName);

            if (newScene != CurrentLevelType)
            {
                CurrentLevelType = newScene;
                CurrentSceneName = scene.name;
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

            Deboog.Log($"Scene Loaded: [{sceneName}]");

            sceneName = (sceneName.Contains("P-")) ? "Sanctum" : sceneName;
            sceneName = (sceneName.Contains("-S")) ? "Secret" : sceneName;
            sceneName = (sceneName.Contains("Level")) ? "Level" : sceneName;
            sceneName = (sceneName.Contains("Intermission")) ? "Intermission" : sceneName;

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
                case "Secret":
                    return UKLevelType.Secret;
                case "Sanctum":
                    return UKLevelType.PrimeSanctum;
                case "CreditsMuseum2":
                    return UKLevelType.Credits;
                default:
                    return UKLevelType.Unknown;
            }
        }

        /// <summary>
        /// Returns true if the current scene is playable.
        /// This will return false for all secret levels.
        /// </summary>
        /// <returns></returns>
        public static bool InLevel()
        {
            bool inNonPlayable = (CurrentLevelType == UKLevelType.MainMenu || CurrentLevelType == UKLevelType.Intro || CurrentLevelType == UKLevelType.Intermission || CurrentLevelType == UKLevelType.Secret || CurrentLevelType == UKLevelType.Unknown);
            return !inNonPlayable;
        }
    }
}
