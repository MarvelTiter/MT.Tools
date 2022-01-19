using System.Diagnostics;

namespace MT.KitTools.LogTool
{
    public class DebugLogger : ILogger
    {
        public LogConfig LogConfig { get; set; }

        public void WriteLog(LogInfo logInfo)
        {
            Debug.WriteLine(logInfo.FormatLogMessage());
        }
    }
}
