using System;

namespace MT.KitTools.LogTool
{
    public interface ILogger : IDisposable
    {
        LogConfig LogConfig { get; set; }
        void WriteLog(LogInfo logInfo);
    }
}
