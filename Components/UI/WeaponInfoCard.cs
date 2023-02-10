using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace UltraFunGuns
{
    public class WeaponInfoCard : MonoBehaviour
    {
        public RectTransform RectTransform { get; private set; }

        public float manualScalar = 0.3333f;

        private Text header, body;

        private Image background;
        private Shadow headerShadow;

        private WeaponInfo info;

        public void Awake()
        {
            RectTransform = GetComponent<RectTransform>();
            background = transform.Find("Container/HeaderBackground").GetComponent<Image>();

            header = background.transform.Find("HeaderText").GetComponent<Text>();
            body = transform.Find("Container/BodyBackground/BodyText").GetComponent<Text>();
            headerShadow = header.GetComponent<Shadow>();

            transform.localScale *= 0.5f;
            gameObject.SetActive(false);
        }

        public void SetWeaponInfo(WeaponInfo info)
        {
            this.info = info;
            Refresh();
        }

        private void Refresh()
        {
            header.text = info.DisplayName;
            body.text = info.GetCodexText();
            //body.text = $"{mousePos}\n\n\n{Screen.dpi}\n\n{Screen.dpi/referenceDpi}";

            Color color = WeaponManager.GetColor(info.IconColor);
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
                //body.text = $"{mousePos}\n\n\n{Screen.dpi}\n\n{Screen.dpi / referenceDpi}\n\n{RectTransform.anchoredPosition}";
            }
        }

        const int referenceDpi = 96;

        private void UpdatePosition(Vector2 mousePos)
        {
            float scalingFactor = Screen.dpi/referenceDpi;

            float screenHeight = scalingFactor*Screen.height;
            float screenWidth = scalingFactor*Screen.width;

            float pivotX = Mathf.InverseLerp(0.0f, screenWidth, mousePos.x);
            float pivotY = Mathf.InverseLerp(0.0f, screenHeight, mousePos.y);

            RectTransform.pivot = new Vector2(pivotX, pivotY);
            RectTransform.anchoredPosition = mousePos*manualScalar;
            //RectTransform.position = CameraController.Instance.cam.ScreenToWorldPoint(mousePos);
        }
    }


    
}