using HydraDynamics.CameraTools;
using System;
using System.Collections.Generic;
using System.Text;

namespace UltraFunGuns.Util
{
    public static class FreecamAssist
    {
        private static bool initialized;

        public static void Init()
        {
            if (initialized)
                return;

            initialized = true;
            Freecam.OnFreecamStateChange += SetInput;
        }

        private static void SetInput(bool disabled)
        {
            if (disabled)
            {
                InputManager.Instance.InputSource.Disable();
            }
            else
            {
                InputManager.Instance.InputSource.Enable();
            }
        }
    }
}
