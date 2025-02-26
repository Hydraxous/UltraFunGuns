using System;
using System.Collections.Generic;
using System.Text;

namespace UltraFunGuns.Logging
{
    internal interface Ilogger
    {
        public void Log(object obj);
        public void LogWarning(object obj);
        public void LogError(object obj);
        public void LogException(Exception ex);
    }
}
