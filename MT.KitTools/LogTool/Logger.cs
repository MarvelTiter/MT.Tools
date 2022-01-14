using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace MT.KitTools.LogTool
{
    public class Logger
    {
        private static Dictionary<string, ILogger> LoggerDict { get; } = new Dictionary<string, ILogger>();
        private static LogConfig logConfig;
        public static void Config(Action<LogConfig> action)
        {
            logConfig = new LogConfig();
            action?.Invoke(logConfig);
        }
        public static void Enable(LogType logType)
        {
            if (logType.HasFlag(LogType.Console))
            {
                if (!LoggerDict.ContainsKey("Console"))
                    LoggerDict.Add("Console", new ConsoleLogger());
            }
            if (logType.HasFlag(LogType.File))
            {
                if (!LoggerDict.ContainsKey("File"))
                    LoggerDict.Add("File", new FileLogger());
            }
        }
        public static void Enable(string key, ILogger logger)
        {
            if (LoggerDict.ContainsKey(key))
                throw new Exception($"key {key} already exit");
            LoggerDict.Add(key, logger);
        }
        public static void Disable(LogType logType)
        {
            if (logType.HasFlag(LogType.Console))
            {
                LoggerDict.Remove("Console");
            }
            if (logType.HasFlag(LogType.File))
            {
                var logger = LoggerDict["File"];
                logger.Dispose();
                LoggerDict.Remove("File");
            }
        }
        public static void Disable(string key)
        {
            var logger = LoggerDict[key];
            logger.Dispose();
            LoggerDict.Remove(key);
        }

        public static event Action<LogInfo> OnLog;

        /// <summary>
        /// 写入Info级别的日志
        /// </summary>
        public static void Info(string msg,
            [CallerFilePath] string callerPath = null,
            [CallerLineNumber] int callerLine = 0,
            [CallerMemberName] string callerMethod = null) => Write(LogLevel.Info, msg, callerPath, callerLine, callerMethod);

        /// <summary>
        /// 写入debug级别日志
        /// </summary>
        public static void Debug(string msg,
            [CallerFilePath] string callerPath = null,
            [CallerLineNumber] int callerLine = 0,
            [CallerMemberName] string callerMethod = null) => Write(LogLevel.Debug, msg, callerPath, callerLine, callerMethod);

        /// <summary>
        /// 写入warn级别日志
        /// </summary>
        public static void Warn(string msg,
            [CallerFilePath] string callerPath = null,
            [CallerLineNumber] int callerLine = 0,
            [CallerMemberName] string callerMethod = null) => Write(LogLevel.Warn, msg, callerPath, callerLine, callerMethod);

        /// <summary>
        /// 写入error级别日志
        /// </summary>
        public static void Error(Exception error,
            [CallerFilePath] string callerPath = null,
            [CallerLineNumber] int callerLine = 0,
            [CallerMemberName] string callerMethod = null) => Write(LogLevel.Error, error.Message, callerPath, callerLine, callerMethod, error);

        /// <summary>
        /// 写入error级别日志
        /// </summary>
        public static void Error(string msg,
            [CallerFilePath] string callerPath = null,
            [CallerLineNumber] int callerLine = 0,
            [CallerMemberName] string callerMethod = null) => Write(LogLevel.Error, msg, callerPath, callerLine, callerMethod);

        /// <summary>
        /// 写入fatal级别日志
        /// </summary>
        public static void Fatal(Exception fatal,
            [CallerFilePath] string callerPath = null,
            [CallerLineNumber] int callerLine = 0,
            [CallerMemberName] string callerMethod = null) => Write(LogLevel.Fatal, fatal.Message, callerPath, callerLine, callerMethod, fatal);

        /// <summary>
        /// 写入fatal级别日志
        /// </summary>
        public static void Fatal(string msg,
            [CallerFilePath] string callerPath = null,
            [CallerLineNumber] int callerLine = 0,
            [CallerMemberName] string callerMethod = null) => Write(LogLevel.Fatal, msg, callerPath, callerLine, callerMethod);

        static object locker = new object();
        public static void Write(LogLevel level, string msg, string source, int line, string member, Exception ex = null)
        {
            lock (locker)
            {
                LogInfo logInfo = new LogInfo()
                {
                    LogLevel = level,
                    Message = msg,
                    ThreadId = Thread.CurrentThread.ManagedThreadId,
                    Source = source,
                    LogLine = line,
                    LogMember = member,
                    Exception = new LogException(ex)
                };
                OnLog?.Invoke(logInfo);

                if (logInfo.Exception.Handled)
                {
                    return;
                }

                foreach (var item in LoggerDict.Values)
                {
                    item.LogConfig = logConfig;
                    item.WriteLog(logInfo);
                }
            }
        }
    }
}
