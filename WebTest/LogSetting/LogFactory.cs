namespace WebTest.LogSetting
{
    public static partial class LogEx
    {
        public class LogFactory : ILoggerFactory
        {
            private readonly Dictionary<string, Microsoft.Extensions.Logging.ILogger> _loggers = new Dictionary<string, Microsoft.Extensions.Logging.ILogger>(StringComparer.Ordinal);
            MarvelLogProvider _provider;
            public LogFactory(MarvelLogProvider provider)
            {
                _provider = provider;
            }
            public void AddProvider(ILoggerProvider provider)
            {

            }

            public ILogger CreateLogger(string categoryName)
            {
                lock (_loggers)
                {
                    if (!_loggers.TryGetValue(categoryName, out var logger))
                    {
                        logger = _provider.CreateLogger(categoryName);
                        _loggers[categoryName] = logger;
                    }
                    return logger;
                }
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
            protected virtual void Dispose(bool disposing)
            {
                if (disposing)
                {
                    _provider.Dispose();
                }
            }
        }
    }
}
