using System;
using System.Collections.Generic;
using System.Text;
using UltraFunGuns.UI.Template;
using UnityEngine;
using UnityEngine.UI;

namespace UltraFunGuns.UI
{
    public static class DynUI
    {

        [UFGAsset("UI_Frame")]
        private static GameObject FramePrefab;

        [UFGAsset("UI_Button")]
        private static GameObject ButtonPrefab;

        //[UFGAsset("UI_Button_Image")]
        private static GameObject ImageButtonPrefab;

        //[UFGAsset("UI_Slider")]
        private static GameObject SliderPrefab;

        //[UFGAsset("UI_ScrollRect")]
        private static GameObject ScrollRectPrefab;

        [UFGAsset("UI_Text")]
        private static GameObject TextPrefab;

        public static void Frame(RectTransform rect, Action<Frame> onInstance)
        {
            Instantiate<Frame>(rect, FramePrefab, onInstance);
        }

        public static void Button(RectTransform rect, Action<Button> onInstance)
        {
            Instantiate<Button>(rect, ButtonPrefab, onInstance);
        }

        public static void ImageButton(RectTransform rect, Action<Button> onInstance)
        {
            Instantiate<Button>(rect, ImageButtonPrefab, onInstance);
        }

        public static void ScrollRect(RectTransform rect, Action<ScrollRect> onInstance)
        {
            Instantiate<ScrollRect>(rect, ScrollRectPrefab, onInstance);
        }

        public static void Label(RectTransform rect, Action<Text> onInstance)
        {
            Instantiate<Text>(rect, TextPrefab, onInstance);
        }

        private static void Instantiate<T>(RectTransform rect, GameObject prefab, Action<T> onInstance) where T : Component
        {
            onInstance?.Invoke(GameObject.Instantiate(prefab, rect).GetComponent<T>());
        }


        public static class Layout
        {
            public static void HalfTop(RectTransform rt)
            {
                rt.anchorMin = new Vector2(0f, 0.5f);
                rt.anchorMax = new Vector2(1, 1);
            }

            public static void HalfBottom(RectTransform rt)
            {
                rt.anchorMin = new Vector2(0, 0);
                rt.anchorMax = new Vector2(1, 0.5f);
            }

            public static void HalfLeft(RectTransform rt)
            {
                rt.anchorMin = new Vector2(0, 0);
                rt.anchorMax = new Vector2(0.5f, 1);
            }

            public static void HalfRight(RectTransform rt)
            {
                rt.anchorMin = new Vector2(0,0);
                rt.anchorMax = new Vector2(0.5f,1);
            }

            public static void FillParent(RectTransform rt)
            {
                rt.anchorMin = new Vector2(0,0);
                rt.anchorMax = new Vector2(1, 1);
                Fill(rt);
            }

            public static void Fill(RectTransform rt)
            {
                rt.sizeDelta = new Vector2(0f,0f);
            }

            public static void Padding(RectTransform rt, float padding)
            {
                rt.anchoredPosition = new Vector2(-(padding*2f),-(padding*2f));
            }
        }

    }
}
