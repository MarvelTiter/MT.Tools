namespace MT.KitTools.LogTool
{
    public interface ILogger
    {
        LogConfig LogConfig { get; set; }
        void WriteLog(LogInfo logInfo);
    }
}
