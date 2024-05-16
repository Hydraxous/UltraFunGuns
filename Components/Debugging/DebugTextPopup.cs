using UnityEngine;
using UnityEngine.UI;

namespace UltraFunGuns
{
    public class DebugTextPopup : MonoBehaviour
    {
        [SerializeField] private Text text;
        [SerializeField] private DestroyAfterTime destroyAfterTime;

        private void Awake()
        {
            if(text == null)
            {
                text = GetComponentInChildren<Text>();
            }
        }

        public void SetText(string newText, Color color)
        {
            if(text != null)
            {
                text.text = newText;
                text.color = color;
            }
        }

        public void SetKillTime(float newTime)
        {
            if(destroyAfterTime != null)
            {
                destroyAfterTime.TimeLeft = newTime;
            }
        }

        public void OnDestroy()
        {
            Visualizer.ClearDebugText(this);
        }
    }
}
