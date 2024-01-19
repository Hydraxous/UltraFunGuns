using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.UI;

namespace UltraFunGuns
{
    public static class ButtonExtensions
    {

        public static void SetClickAction(this Button b, Action action)
        {
            b.onClick.RemoveAllListeners();
            b.onClick.AddListener(() => action.Invoke());
        }
    }
}
