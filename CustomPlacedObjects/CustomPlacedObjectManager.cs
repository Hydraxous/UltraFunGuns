using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace UltraFunGuns.CustomPlacedObjects
{
    public static class CustomPlacedObjectManager
    {
        private static Dictionary<string, List<ICustomPlacedObject>> objectsForScenes = new Dictionary<string, List<ICustomPlacedObject>>();

        public static void Init()
        {
            RegisterAllCustomPlacementObjects();
            UKAPIP.OnLevelChanged += (_) => OnLevelChanged();
        }


        private static void RegisterAllCustomPlacementObjects()
        {
            Type intf = typeof(ICustomPlacedObject);
            List<Type> customPlaceables = Assembly.GetExecutingAssembly().GetTypes().Where(p => intf.IsAssignableFrom(p) && !p.IsInterface).ToList();

            foreach(Type customPlaceableType in customPlaceables)
            {
                object newObject = Activator.CreateInstance(customPlaceableType);
                ICustomPlacedObject customPlaceable = newObject as ICustomPlacedObject;
                RegisterPlacable(customPlaceable);
            }
        }

        private static void OnLevelChanged()
        {
            string sceneName = UKAPIP.CurrentSceneName;

            if (!objectsForScenes.ContainsKey(sceneName))
                return;

            foreach (ICustomPlacedObject customPlacedObject in objectsForScenes[sceneName])
            {
                customPlacedObject.Place(sceneName);
            }
        }

        public static void RegisterPlacable(ICustomPlacedObject customPlacedObject)
        {
            string[] scenes = customPlacedObject.GetScenePlacementNames();

            foreach(string scene in scenes)
            {
                if (!objectsForScenes.ContainsKey(scene))
                {
                    objectsForScenes.Add(scene, new List<ICustomPlacedObject>());
                }

                if (!objectsForScenes[scene].Contains(customPlacedObject))
                {
                    objectsForScenes[scene].Add(customPlacedObject);
                }
            }
        }
    }

    public interface ICustomPlacedObject
    {
        public string[] GetScenePlacementNames();
        public void Place(string sceneName);
    }
}
