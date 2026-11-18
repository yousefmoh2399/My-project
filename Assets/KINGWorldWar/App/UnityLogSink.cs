using System;
using KingWorldWar.Core.Logging;
using UnityEngine;

namespace KingWorldWar.App
{
    /// <summary>
    /// Log sink that routes sanitized messages to UnityEngine.Debug.
    /// </summary>
    public class UnityLogSink : ILogSink
    {
        public void Emit(LogLevel level, string category, string message, Exception exception)
        {
            string formatted = $"[{level.ToString().ToUpperInvariant()}] [{category}] {message}";
            if (exception != null)
            {
                formatted += $"\nException: {exception}";
            }

            switch (level)
            {
                case LogLevel.Trace:
                case LogLevel.Debug:
                case LogLevel.Info:
                    Debug.Log(formatted);
                    break;
                case LogLevel.Warning:
                    Debug.LogWarning(formatted);
                    break;
                case LogLevel.Error:
                    Debug.LogError(formatted);
                    break;
            }
        }
    }
}
