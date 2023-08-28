using System;
using System.Collections.Generic;
using System.Text;

namespace UltraFunGuns
{
    public static class ConfigValidators
    {

        public static bool Clamp(float value, float min, float max)
        {
            return value > min && value < max;
        }

        public static bool Min(float value, float min)
        {
            return value > min;
        }

        public static bool Max(float value, float max)
        {
            return value < max;
        }

        public static bool Clamp(int value, int min, int max)
        {
            return value > min && value < max;
        }

        public static bool Min(int value, int min)
        {
            return value > min;
        }

        public static bool Max(int value, int max)
        {
            return value < max;
        }
    }
}
