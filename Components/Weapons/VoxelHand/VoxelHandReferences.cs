using UnityEngine;

namespace UltraFunGuns
{
    public class VoxelHandReferences : MonoBehaviour
    {
        [SerializeField] private GameObject handObject;
        public GameObject HandObject => handObject;


        [SerializeField] private GameObject displayedCube;
        public GameObject DisplayedCube => displayedCube;


        [SerializeField] private Renderer displayCubeRenderer;
        public Renderer DisplayCubeRenderer => displayCubeRenderer;

    }
}
