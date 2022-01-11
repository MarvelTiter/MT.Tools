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

        public static event Action<LogInfo> OnLog;

        /// <summary>
        /// 写入Info级别的日志
        /// </summary>
        public static void Info(string msg,
            [CallerFilePath] string callerPath = null,
            [CallerLineNumber] int callerLine = 0,
            [CallerMemberName] string callerMethod = null)
        {
            var log = new LogInfo()
            {
                LogLevel = LogLevel.Info,
                Message = msg,
                ThreadId = Thread.CurrentThread.ManagedThreadId,
                Source = callerPath,
                LogLine = callerLine,
                LogMember = callerMethod,
            };
            Write(log);
        }

        /// <summary>
        /// 写入debug级别日志
        /// </summary>
        public static void Debug(string msg,
            [CallerFilePath] string callerPath = null,
            [CallerLineNumber] int callerLine = 0,
            [CallerMemberName] string callerMethod = null)
        {
            LogInfo log = new LogInfo()
            {
                LogLevel = LogLevel.Debug,
                Message = msg,
                ThreadId = Thread.CurrentThread.ManagedThreadId,
                Source = callerPath,
                LogLine = callerLine,
                LogMember = callerMethod,
            };
            Write(log);
        }

        /// <summary>
        /// 写入warn级别日志
        /// </summary>
        public static void Warn(string msg,
            [CallerFilePath] string callerPath = null,
            [CallerLineNumber] int callerLine = 0,
            [CallerMemberName] string callerMethod = null)
        {
            LogInfo log = new LogInfo()
            {
                LogLevel = LogLevel.Warn,
                Message = msg,
                ThreadId = Thread.CurrentThread.ManagedThreadId,
                Source = callerPath,
                LogLine = callerLine,
                LogMember = callerMethod,
            };
            Write(log);
        }

        /// <summary>
        /// 写入error级别日志
        /// </summary>
        public static void Error(Exception error,
            [CallerFilePath] string callerPath = null,
            [CallerLineNumber] int callerLine = 0,
            [CallerMemberName] string callerMethod = null)
        {
            LogInfo log = new LogInfo()
            {
                LogLevel = LogLevel.Error,
                Message = error.Message,
                ThreadId = Thread.CurrentThread.ManagedThreadId,
                Exception = error,
                Source = callerPath,
                LogLine = callerLine,
                LogMember = callerMethod,
            };
            Write(log);
        }

        /// <summary>
        /// 写入error级别日志
        /// </summary>
        public static void Error(string msg,
            [CallerFilePath] string callerPath = null,
            [CallerLineNumber] int callerLine = 0,
            [CallerMemberName] string callerMethod = null)
        {
            LogInfo log = new LogInfo()
            {
                LogLevel = LogLevel.Error,
                Message = msg,
                ThreadId = Thread.CurrentThread.ManagedThreadId,
                Source = callerPath,
                LogLine = callerLine,
                LogMember = callerMethod,
            };
            Write(log);
        }

        /// <summary>
        /// 写入fatal级别日志
        /// </summary>
        public static void Fatal(Exception fatal,
            [CallerFilePath] string callerPath = null,
            [CallerLineNumber] int callerLine = 0,
            [CallerMemberName] string callerMethod = null)
        {
            LogInfo log = new LogInfo()
            {
                LogLevel = LogLevel.Fatal,
                Message = fatal.Message,
                ThreadId = Thread.CurrentThread.ManagedThreadId,
                Exception = fatal,
                Source = callerPath,
                LogLine = callerLine,
                LogMember = callerMethod,
            };
            Write(log);
        }

        /// <summary>
        /// 写入fatal级别日志
        /// </summary>
        public static void Fatal(string msg,
            [CallerFilePath] string callerPath = null,
            [CallerLineNumber] int callerLine = 0,
            [CallerMemberName] string callerMethod = null)
        {
            LogInfo log = new LogInfo()
            {
                LogLevel = LogLevel.Fatal,
                Message = msg,
                ThreadId = Thread.CurrentThread.ManagedThreadId,
                Source = callerPath,
                LogLine = callerLine,
                LogMember = callerMethod,
            };
            Write(log);
        }
        static object locker = new object();
        public static void Write(LogInfo logInfo)
        {
            lock (locker)
            {
                foreach (var item in LoggerDict.Values)
                {
                    item.LogConfig = logConfig;
                    item.WriteLog(logInfo);
                }
                OnLog?.Invoke(logInfo);
            }
        }
    }
}
