using System;

namespace MT.KitTools.LogTool
{
    public class ConsoleLogger : ILogger
    {
        public LogConfig LogConfig { get; set; }

        public void Dispose()
        {

        }

        public void WriteLog(LogInfo logInfo)
        {

            switch (logInfo.LogLevel)
            {
                case LogLevel.Info:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case LogLevel.Debug:
                    Console.ForegroundColor = ConsoleColor.DarkBlue;
                    break;
                case LogLevel.Warn:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogLevel.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogLevel.Fatal:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    break;
                default:
                    break;
            }
            Console.WriteLine(logInfo.FormatLogMessage());
        }
    }
}
