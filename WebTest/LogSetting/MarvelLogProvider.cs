using Mine=MT.KitTools.LogTool;

namespace WebTest.LogSetting
{
    public static partial class LogEx
    {
        public class MarvelLogProvider : ILoggerProvider
        {
            private readonly Mine.LogConfig config;

            public MarvelLogProvider(Mine.LogConfig config)
            {
                this.config = config;
            }
            public ILogger CreateLogger(string categoryName)
            {
                return new MarvelLog(Mine.LoggerFactory.GetLogger(categoryName,config));
            }

            public void Dispose()
            {

            }
        }
    }
}
