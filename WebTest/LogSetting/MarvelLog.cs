using Mine = MT.KitTools.LogTool;
namespace WebTest.LogSetting
{
    public static partial class LogEx
    {
        public class MarvelLog : ILogger
        {
            private readonly Mine.LoggerManage logger;

            public MarvelLog(Mine.LoggerManage logger)
            {
                this.logger = logger;
            }
            public IDisposable BeginScope<TState>(TState state)
            {
                return null;
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return true;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                if (logLevel == LogLevel.Information)
                {
                    var result = formatter?.Invoke(state, exception);
                    logger.Log(ConvertLogLevel(logLevel),result,exception);
                }
            }

           
            private Mine.LogLevel ConvertLogLevel(LogLevel level)
            {
                return level switch
                {
                    LogLevel.Debug => Mine.LogLevel.Debug,
                    LogLevel.Information => Mine.LogLevel.Info,
                    LogLevel.Warning => Mine.LogLevel.Warn,
                    LogLevel.Error => Mine.LogLevel.Error,
                    LogLevel.Critical => Mine.LogLevel.Fatal,
                    _ => Mine.LogLevel.Debug
                };
            }
        }
    }
}
