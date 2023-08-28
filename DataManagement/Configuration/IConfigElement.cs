using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    public interface IConfigElement
    {
        public void BindDescriptor (Configgable configgable);
        public Configgable GetDescriptor();
        public void BuildElement(RectTransform rect);
    }
}
