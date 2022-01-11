using System;

namespace MT.KitTools.LogTool
{
    [Flags]
    public enum LogType
    {
        Console = 1,
        File = 1 << 1,
    }
    /// <summary>
    /// 日志级别
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// 信息级别
        /// </summary>
        Info,
        /// <summary>
        /// debug级别
        /// </summary>
        Debug,
        /// <summary>
        /// 警告级别
        /// </summary>
        Warn,
        /// <summary>
        /// 错误级别
        /// </summary>
        Error,
        /// <summary>
        /// 致命级别
        /// </summary>
        Fatal
    }
}
