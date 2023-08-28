using UltraFunGuns.UI;
using UnityEngine;
using UnityEngine.UI;

namespace UltraFunGuns.Components.UI.Configuration
{
    public static class CfgTestCase
    {
        [Configgable(displayName: "Quit The Game?", path: "Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!/Click Me!")]
        private static ConfigCustomElement superNestedButton = new ConfigCustomElement(Dingus2);

        [Configgable("Exit Game")]
        private static ConfigButton badButton = new ConfigButton(() => Application.Quit(), "Yes");

        [Configgable("Exit Game")]
        private static ConfigCustomElement badButton2 = new ConfigCustomElement((c,r) =>
        {
            DynUI.Frame(r, (f) =>
            {
                DynUI.Button(f.RectTransform, (b) =>
                {
                    DynUI.Layout.FillParent(b.GetComponent<RectTransform>());

                    b.GetComponentInChildren<Text>().text = "No";

                    b.onClick.AddListener(() =>
                    {
                        r.GetComponentInParent<ConfigurationPage>()?.Back();
                    });
                });
            });
        });


        public static void Bruh(string lol)
        {
            Debug.Log(lol);   
        }

        private static void Dingus2(Configgable descriptor, RectTransform rect)
        {
            DynUI.Frame(rect, (f) =>
            {
                f.SetBorderColor(Color.red);
                f.SetBackgroundColor(Color.blue);

                Vector2 size = f.RectTransform.sizeDelta;
                size.y *= 3;
                f.RectTransform.sizeDelta = size;

                DynUI.Label(f.Content, (t) =>
                {
                    DynUI.Layout.FillParent(t.GetComponent<RectTransform>());

                    t.text = $"Do you get to the Lust District very often? Oh what am I saying... Of course you don't.";
                    t.color = Color.red;
                });

            });

        }
    }
}
