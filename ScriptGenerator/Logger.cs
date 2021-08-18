using NLog;
using System;

namespace ScriptGenerator
{
    public static class Logger
    {
        private static readonly ILogger _logger = NLog.LogManager.GetCurrentClassLogger();

        public static void Log(string message)
        {
            _logger.Log(LogLevel.Info, message);
        }

        public static void LogException(Exception exception)
        {
            Log(GetExceptionMessages(exception));
        }

        public static void LogHeader(string message)
        {
            _logger.Log(LogLevel.Info, "--------------------------");

            Log(message);

            _logger.Log(LogLevel.Info, "--------------------------");
        }

        public static string GetExceptionMessages(Exception exception, string messages = "")
        {
            if (exception == null) return string.Empty;
            if (messages == "") messages = exception.Message;
            if (exception.InnerException != null)
                messages += "\r\nInnerException: " + GetExceptionMessages(exception.InnerException);
            return messages;
        }
    }
}
