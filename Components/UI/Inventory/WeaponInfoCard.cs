using UnityEngine;
using UnityEngine.UI;

namespace UltraFunGuns
{
    public class WeaponInfoCard : MonoBehaviour
    {
        public RectTransform RectTransform { get; private set; }

        private Canvas canvas;

        public bool useCanvasScaler = true;
        public bool scaleWithDisplaySize = false;

        private Text header, body;

        private Image background;
        private Shadow headerShadow;
        private UFGWeapon info;

        public void Awake()
        {
            canvas = GetComponentInParent<Canvas>();
            RectTransform = GetComponent<RectTransform>();
            gameObject.AddComponent<HudOpenEffect>();
            background = transform.Find("Container/HeaderBackground").GetComponent<Image>();

            header = background.transform.Find("HeaderText").GetComponent<Text>();
            body = transform.Find("Container/BodyBackground/BodyText").GetComponent<Text>();
            headerShadow = header.GetComponent<Shadow>();

            //transform.localScale *= referenceDpi/Screen.dpi;
            gameObject.SetActive(false);
        }

        public void SetWeaponInfo(UFGWeapon info)
        {
            this.info = info;
            Refresh();
        }

        private void Refresh()
        {
            header.text = info.DisplayName;
            body.text = info.GetCodexText();
            //body.text = $"{mousePos}\n\n\n{Screen.dpi}\n\n{Screen.dpi/referenceDpi}";

            Color color = info.IconColor.ToColor();
            header.color = color;
            headerShadow.effectColor = new Color(color.r,color.g,color.b, color.a*0.3803f);
            background.color = new Color(color.r, color.g, color.b, color.a * 0.3568f);
        }

        private static Vector2 mousePos;

        private void Update()
        {
            mousePos = Input.mousePosition;

            if (RectTransform != null)
            {
                UpdatePosition(mousePos);
                //body.text = $"Mpos: {mousePos}\nDpi: {Screen.dpi}\nDpiScalar: {Screen.dpi / referenceDpi}\nAnchorpos: {RectTransform.anchoredPosition}\nTFPos: {RectTransform.position}\nPivot: {RectTransform.pivot}\nAnchor3D: {RectTransform.anchoredPosition3D}";
            }
        }

        const int referenceDpi = 96;

        private void UpdatePosition(Vector2 mousePos)
        {
            float scalingFactor = canvas.scaleFactor;

            float screenHeight = Screen.height;
            float screenWidth = Screen.width;

            float pivotX = Mathf.InverseLerp(0.0f, screenWidth, mousePos.x);
            float pivotY = Mathf.InverseLerp(0.0f, screenHeight, mousePos.y);

            RectTransform.pivot = new Vector2(pivotX, pivotY);
            RectTransform.anchoredPosition = (mousePos)/scalingFactor;
        }
        
        private void OnEnable()
        {
            if(RectTransform != null)
            {
                float scale = Data.Config.Data.InventoryInfoCardScale;
                RectTransform.localScale = new Vector3(scale, scale, scale);
            }
        }

    }


    
}