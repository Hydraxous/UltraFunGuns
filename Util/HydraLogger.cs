using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.IO;

namespace UltraFunGuns
{
    public static class HydraLogger
    {
        const string prefix = "UFG";

        private static string sessionLog;
        private static int logCounter = 0;
        private static int logSaveThreshold = 100;

        private static bool initialized = false;
        public static void Init()
        {
            string sysInfo =
                "=======================================\n" +
                $"UFG Version: {UltraFunGuns.Version}\n" +
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
            if(type != LogType.Exception)
            {
                return;
            }

            string exceptionMessage =
                "=================================" +
                $"{condition}\n" +
                $"{stackTrace}" +
                "=================================";

            LogToFile(exceptionMessage, DebugChannel.Fatal);
            
        }

        public static void Log(string message, DebugChannel channel = DebugChannel.Message)
        {
            if(!initialized)
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
                default:
                    if (UltraFunGuns.DebugMode || channel == DebugChannel.User)
                        Debug.Log($"{prefix}: {message}");
                    break;
            }
        }

        private static void LogToFile(string message, DebugChannel channel)
        {
            string formattedLogMessage = $"[{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss tt")}]({channel.ToString()}): {message}\n";
            sessionLog += formattedLogMessage;
            ++logCounter;
            if(logCounter % logSaveThreshold == 0)
            {
                WriteLog();
            }
        }

        public static void WriteLog()
        {
            string logFilePath = UltraFunData.GetDataPath("log.txt");

            File.WriteAllText(logFilePath, sessionLog);
        }
    }

    public enum DebugChannel { User, Message, Warning, Error, Fatal }
}
