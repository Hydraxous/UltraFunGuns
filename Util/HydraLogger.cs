using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.IO;

namespace UltraFunGuns
{
    public static class HydraLogger
    {
        const string prefix = "UltraFunGuns";

        private static string sessionLog;
        private static int logCounter = 0;
        private static int logSaveThreshold = 100;

        private static bool initialized = false;
        public static void Init()
        {
            string sysInfo =
                "=======================================\n" +
                $"UFG Version: {UltraFunGuns.RELEASE_VERSION}\n" +
                $"Device Name: {SystemInfo.deviceName}\n" +
                $"Device Model: {SystemInfo.deviceModel}\n" +
                $"OS: {SystemInfo.operatingSystem} [{SystemInfo.operatingSystemFamily}]\n" +
                $"CPU Cores: {SystemInfo.processorCount}\n" +
                $"CPU Freq: {SystemInfo.processorFrequency}\n" +
                $"CPU Type: {SystemInfo.processorType}\n" +
                $"RAM: {SystemInfo.systemMemorySize}\n" +
                $"GPU Name: {SystemInfo.graphicsDeviceName}\n" +
                $"GPU Vendor: {SystemInfo.graphicsDeviceVendor}\n" +
                $"GPU Type: {SystemInfo.graphicsDeviceType}\n" +
                $"GPU Vram: {SystemInfo.graphicsMemorySize}\n" +
                "========================================\n";

            sessionLog += sysInfo;

            Application.logMessageReceived += Application_logMessageReceived;
            initialized = true;
        }

        private static void Application_logMessageReceived(string condition, string stackTrace, LogType type)
        {
            if (type != LogType.Exception)
            {
                return;
            }

            string exceptionMessage =
                "=================================\n" +
                $"{condition}\n" +
                $"{stackTrace}\n" +
                "=================================";

            LogToFile(exceptionMessage, DebugChannel.Fatal);

        }

        

        public static void Log(string message, DebugChannel channel = DebugChannel.Message)
        {
            if (!initialized)
            {
                Init();
            }

            LogToFile(message, channel);

            switch (channel)
            {
                case DebugChannel.Fatal:
                case DebugChannel.Error:
                    Debug.LogError($"{prefix}: {message}");
                    break;
                case DebugChannel.Warning:
                    Debug.LogWarning($"{prefix}: {message}");
                    break;
                case DebugChannel.Spam:
                    SpamLog(message);                    
                    break;
                default:
                    if (UltraFunGuns.DebugMode || channel == DebugChannel.User)
                        Debug.Log($"{prefix}: {message}");
                    break;
            }
        }

        private static string lastSpamLog = "";
        private static int spamCount = 0;

        private static void SpamLog(string message)
        {
            if (!UltraFunGuns.DebugMode)
            {
                return;
            }

            if (lastSpamLog != message)
            {
                spamCount = 0;
                lastSpamLog = message;
            }else
            {
                ++spamCount;
            }

            if(spamCount % 20 == 0)
            {
                Debug.Log($"{prefix}: {message}");
            }
        }

        private static void LogToFile(string message, DebugChannel channel)
        {
            string formattedLogMessage = $"[{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss tt")}]({channel.ToString()}): {message}\n";
            sessionLog += formattedLogMessage;
            ++logCounter;
            if (logCounter % logSaveThreshold == 0)
            {
                WriteLog();
            }
        }

        public static void WriteLog()
        {
            string logFilePath = Data.GetDataPath("log.txt");

            File.WriteAllText(logFilePath, sessionLog);
        }

        const string textHeader = @"
   __  ______             ______            ______                
  / / / / / /__________ _/ ____/_  ______  / ____/_  ______  _____
 / / / / / __/ ___/ __ `/ /_  / / / / __ \/ / __/ / / / __ \/ ___/
/ /_/ / / /_/ /  / /_/ / __/ / /_/ / / / / /_/ / /_/ / / / (__  ) 
\____/_/\__/_/   \__,_/_/    \__,_/_/ /_/\____/\__,_/_/ /_/____/

Created By Hydraxous";

        public static void StartMessage()
        {
            Debug.Log(textHeader);
            Log($"Loading started. Version: {UltraFunGuns.RELEASE_VERSION}", DebugChannel.User);
        }
    }

    public enum DebugChannel { User, Message, Warning, Spam, Error, Fatal }
}
