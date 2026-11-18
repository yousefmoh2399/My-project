using System;
using System.Diagnostics;

namespace KingWorldWar.Core.Logging
{
    /// <summary>
    /// Sink contract for receiving dispatched log messages.
    /// </summary>
    public interface ILogSink
    {
        void Emit(LogLevel level, string category, string message, Exception exception);
    }

    /// <summary>
    /// Contract for structured application logging.
    /// </summary>
    public interface ILog
    {
        LogLevel MinimumLevel { get; set; }

        void Trace(string category, string message);
        void Debug(string category, string message);
        void Info(string category, string message);
        void Warning(string category, string message, Exception exception = null);
        void Error(string category, string message, Exception exception = null);
    }

    /// <summary>
    /// Thread-safe, structured application logger with automatic credential redaction.
    /// </summary>
    public class GameLog : ILog
    {
        private static ILog _default = new GameLog(new ConsoleLogSink(), LogLevel.Debug);
        private readonly ILogSink _sink;

        public static ILog Default
        {
            get => _default;
            set => _default = value ?? throw new ArgumentNullException(nameof(value));
        }

        public LogLevel MinimumLevel { get; set; }

        public GameLog(ILogSink sink, LogLevel minimumLevel = LogLevel.Info)
        {
            _sink = sink ?? throw new ArgumentNullException(nameof(sink));
            MinimumLevel = minimumLevel;
        }

        public void Trace(string category, string message)
        {
#if DEBUG || UNITY_EDITOR
            Log(LogLevel.Trace, category, message, null);
#endif
        }

        public void Debug(string category, string message)
        {
#if DEBUG || UNITY_EDITOR
            Log(LogLevel.Debug, category, message, null);
#endif
        }

        public void Info(string category, string message)
        {
            Log(LogLevel.Info, category, message, null);
        }

        public void Warning(string category, string message, Exception exception = null)
        {
            Log(LogLevel.Warning, category, message, exception);
        }

        public void Error(string category, string message, Exception exception = null)
        {
            Log(LogLevel.Error, category, message, exception);
        }

        private void Log(LogLevel level, string category, string message, Exception exception)
        {
            if (level < MinimumLevel || MinimumLevel == LogLevel.None)
            {
                return;
            }

            string sanitized = LogRedactor.Redact(message);
            _sink.Emit(level, category ?? "General", sanitized, exception);
        }

        /// <summary>
        /// Fallback console sink using System.Console when Unity logging is not wired.
        /// </summary>
        private class ConsoleLogSink : ILogSink
        {
            public void Emit(LogLevel level, string category, string message, Exception exception)
            {
                string text = $"[{level.ToString().ToUpperInvariant()}] [{category}] {message}";
                if (exception != null)
                {
                    text += $"\n{exception}";
                }
                Console.WriteLine(text);
            }
        }
    }
}
