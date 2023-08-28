using System;
using System.Collections.Generic;
using System.Linq;
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

        [UFGAsset("UI_Button_Image")]
        private static GameObject ImageButtonPrefab;

        [UFGAsset("UI_Slider")]
        private static GameObject SliderPrefab;

        //[UFGAsset("UI_ScrollRect")]
        private static GameObject ScrollRectPrefab;

        [UFGAsset("UI_Text")]
        private static GameObject TextPrefab;

        [UFGAsset("UI_InputField")]
        private static GameObject InputFieldPrefab;

        public static void Frame(RectTransform rect, Action<Frame> onInstance)
        {
            Instantiate<Frame>(rect, FramePrefab, onInstance);
        }

        public static void Button(RectTransform rect, Action<Button> onInstance)
        {
            Instantiate<Button>(rect, ButtonPrefab, onInstance);
        }

        public static void ImageButton(RectTransform rect, Action<Button, Image> onInstance)
        {

            Action<Button> onSubInstance = new Action<Button>((b) =>
            {
                Image icon = b.GetComponentsInChildren<Image>().Where(x=>x.name == "Icon").FirstOrDefault();
                onInstance?.Invoke(b,icon);
            });

            Instantiate<Button>(rect, ImageButtonPrefab, onSubInstance);
        }

        public static void ScrollRect(RectTransform rect, Action<ScrollRect> onInstance)
        {
            Instantiate<ScrollRect>(rect, ScrollRectPrefab, onInstance);
        }

        public static void Label(RectTransform rect, Action<Text> onInstance)
        {
            Instantiate<Text>(rect, TextPrefab, onInstance);
        }

        public static void InputField(RectTransform rect, Action<InputField> onInstance)
        {
            Instantiate<InputField>(rect, InputFieldPrefab, onInstance);
        }

        public static void Slider(RectTransform rect, Action<Slider> onInstance)
        {
            Instantiate<Slider>(rect, SliderPrefab, onInstance);
        }

        public static void Div(RectTransform rect, Action<RectTransform> onInstance)
        {
            GameObject div = new GameObject("div");
            div.transform.parent = rect.transform;
            onInstance?.Invoke(div.AddComponent<RectTransform>());
        }

        public static void Component<T>(Transform transform, Action<T> onInstance) where T : Component
        {
            onInstance?.Invoke(transform.gameObject.AddComponent<T>());
        }

        private static void Instantiate<T>(RectTransform rect, GameObject prefab, Action<T> onInstance) where T : Component
        {
            onInstance?.Invoke(GameObject.Instantiate(prefab, rect).GetComponent<T>());
        }

        public static void SetAnchors(this RectTransform rect, float minX, float minY, float maxX, float maxY)
        {
            rect.anchorMax = new Vector2(maxX, maxY);
            rect.anchorMin = new Vector2(minX, minY);
        }

        public static class ConfigUI
        {
            public static void CreateElementSlot(RectTransform rect, string label, Action<RectTransform> onInstance, Action<RectTransform> onButtonSlots = null)
            {
                DynUI.Frame(rect, (f) =>
                {
                    DynUI.Div(f.Content, (operatorsDiv) =>
                    {
                        operatorsDiv.SetAnchors(0, 0, 0.2f, 1f);
                        DynUI.Layout.Fill(operatorsDiv);

                        DynUI.Component<HorizontalLayoutGroup>(operatorsDiv, (hlg) =>
                        {
                            hlg.childForceExpandHeight = true;
                            hlg.childForceExpandWidth = true;

                            hlg.childControlHeight = true;
                            hlg.childControlWidth = true;
                            hlg.childAlignment = TextAnchor.MiddleLeft;
                        });

                        onButtonSlots?.Invoke(operatorsDiv);
                    });


                    DynUI.Div(f.Content, (elementsDiv) =>
                    {
                        elementsDiv.SetAnchors(0.2f, 0, 1f, 1f);
                        DynUI.Layout.Fill(elementsDiv);

                        DynUI.Component<HorizontalLayoutGroup>(elementsDiv, (hlg) =>
                        {
                            hlg.childForceExpandHeight = true;
                            hlg.childForceExpandWidth = true;

                            hlg.childControlHeight = true;
                            hlg.childControlWidth = true;

                            hlg.childAlignment = TextAnchor.MiddleRight;
                        });

                        DynUI.Label(elementsDiv, (t) =>
                        {
                            t.text = label;
                            t.fontSize = 4;
                            t.resizeTextMaxSize = 26;
                        });

                        onInstance?.Invoke(elementsDiv);
                    });
                });
            }
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
                rt.offsetMax= new Vector2(0,0);
                rt.offsetMin= new Vector2(0,0);
                rt.sizeDelta = new Vector2(0f,0f);
            }

            public static void Padding(RectTransform rt, float padding)
            {
                rt.anchoredPosition = new Vector2(-(padding*2f),-(padding*2f));
            }
            
        }

    }
}
