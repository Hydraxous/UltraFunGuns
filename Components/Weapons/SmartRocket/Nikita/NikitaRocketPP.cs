using UnityEngine;

namespace UltraFunGuns
{
    public class NikitaRocketPP : MonoBehaviour
    {
        [SerializeField] private Material mat;

        private void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            Graphics.Blit(src, dest, mat);
        }
    }
}
