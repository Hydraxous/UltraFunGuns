using System;
using System.Collections.Generic;
using System.Text;

namespace UltraFunGuns
{
    public interface IStatusEffect
    {
        public void OnEffectStart();
        public void OnEffectUpdate();
        public void OnEffectStop();

    }
}
