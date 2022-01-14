using System;

namespace MT.KitTools.LogTool
{
    /// <summary>
    /// 日志信息
    /// </summary>
    public class LogInfo
    {
        /// <summary>
        /// 时间
        /// </summary>
        public DateTime Time { get; set; } = DateTime.Now;
        /// <summary>
        /// 线程id
        /// </summary>
        public int ThreadId { get; set; }
        /// <summary>
        /// 日志级别
        /// </summary>
        public LogLevel LogLevel { get; set; }
        /// <summary>
        /// 异常信息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 异常对象
        /// </summary>
        public LogException Exception { get; set; }
        /// <summary>
        /// 触发日志的源文件
        /// </summary>
        public string Source { get; set; }
        /// <summary>
        /// 触发日志的行数
        /// </summary>
        public int LogLine { get; set; }
        /// <summary>
        /// 触发日志的方法
        /// </summary>
        public string LogMember { get; set; }
    }

    public static class LogInfoExtensions
    {
        public static string FormatLogMessage(this LogInfo self)
        {
            return $"[TId:{self.ThreadId}] {DateTime.Now:yyyy-MM-dd HH:mm:ss} [{self.LogLevel.ToString().ToUpper()}] line:{self.LogLine} {self.Source}{Environment.NewLine}{self.Message}{(self.Exception == null ? "" : Environment.NewLine)}{self.Exception?.StackTrace}";
        }
    }
}
