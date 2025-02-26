using BepInEx.Logging;
using System;
using UnityEngine;

namespace UltraFunGuns.Logging
{
    internal class BepInExLogger : Ilogger
    {
        private ManualLogSource _logger;
        public BepInExLogger(ManualLogSource logger)
        {
            _logger = logger;
        }

        public void Log(object obj)
        {
            _logger.LogInfo(obj);
        }

        public void LogError(object obj)
        {
            _logger.LogError(obj);
        }

        public void LogException(Exception ex)
        {
            _logger.LogError($"EXCEPTION: {ex.Message}");
            Debug.LogException(ex);
        }

        public void LogWarning(object obj)
        {
            _logger.LogWarning(obj);
        }
    }
}
