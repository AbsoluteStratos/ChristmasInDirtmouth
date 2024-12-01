using System;

namespace ChristmasInDirtmouth
{
    public static class Logger
    {
        // To change log level modify ModdingApi.GlobalSettings.json in saves folder
        // https://prashantmohta.github.io/ModdingDocs/logging.html#log-levels
        private static string GetPrefix()
        {
            string dt = DateTime.Now.ToString("M/d/yyyyTHH:mm:ss");
            return $"[{ChristmasInDirtmouth.GetName()}][{dt}] - ";
        }

        public static void Info(string message)
        {
            Modding.Logger.Log(GetPrefix() + message);
        }

        public static void Success(string message)
        {
            Modding.Logger.LogFine(GetPrefix() + message);
        }

        public static void Warning(string message)
        {
            Modding.Logger.LogWarn(GetPrefix() + message);
        }

        public static void Error(string message)
        {
            Modding.Logger.LogError(GetPrefix() + message);
        }

        public static void Debug(string message)
        {
            Modding.Logger.LogDebug(GetPrefix() + message);
        }
    }
}
