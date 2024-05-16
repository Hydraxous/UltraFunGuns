using UnityEngine;

namespace UltraFunGuns.CustomPlacedObjects.Objects
{
    public class ArmorCubePlacer : ICustomPlacedObject
    {
        public ArmorCubePlacer() { }

        public string[] GetScenePlacementNames()
        {
            return new string[] { "uk_construct" };
        }

        public bool Place(string sceneName)
        {
            if (!UltraFunGuns.DebugMode)
                return false;

            Place(new Vector3(0.0f, 15.5f, -20.0f),Vector3.zero);
            return true;
        }

        private void Place(Vector3 position, Vector3 rotation)
        {
            GameObject newArmorCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            GameObject newArmorCubeRenderer = GameObject.CreatePrimitive(PrimitiveType.Cube);

            newArmorCube.layer = 26;
            newArmorCubeRenderer.layer = 25;

            newArmorCube.transform.position = position;
            newArmorCubeRenderer.transform.position = position;
            newArmorCube.transform.rotation = Quaternion.Euler(rotation);
            newArmorCubeRenderer.transform.rotation = Quaternion.Euler(rotation);

            if (newArmorCubeRenderer.TryGetComponent<BoxCollider>(out BoxCollider boxCol))
            {
                boxCol.enabled = false;
            }

            newArmorCubeRenderer.transform.localScale = Vector3.one * 19.99f;
            newArmorCube.transform.localScale = Vector3.one*20;

            GameObject newArmorSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            newArmorSphere.layer = 26;

            newArmorSphere.transform.position = position+position;

            newArmorSphere.transform.localScale = Vector3.one * 20;

            GameObject newArmorSphereRender = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            newArmorSphereRender.layer = 25;

            if (newArmorSphereRender.TryGetComponent<SphereCollider>(out SphereCollider sphereCol))
            {
                sphereCol.enabled = false;
            }

            newArmorSphereRender.transform.position = position + position;

            newArmorSphereRender.transform.localScale = Vector3.one * 19.99f;
        }
    }
}
