using Microsoft.Extensions.DependencyInjection.Extensions;
using Mine = MT.KitTools.LogTool;
namespace WebTest.LogSetting
{
    public static partial class LogEx
    {
        public static void UseMarvelLog(this IHostBuilder self, Action<Mine.LogConfig> action)
        {
            Mine.LogConfig config = new Mine.LogConfig();
            action?.Invoke(config);
            self.ConfigureServices((context, services) =>
            {
                AddMLoggerProvider(services, context.Configuration, context.HostingEnvironment, config, (provider, configuration, environment, cfg) => CreateProvider(cfg));
            });
        }

        public static void TryAddLoggingProvider(this IServiceCollection self, Action<IServiceCollection, Action<ILoggingBuilder>> addlogging, IConfiguration configuration, Mine.LogConfig config, Func<IServiceProvider, IConfiguration, Mine.LogConfig, MarvelLogProvider> factory)
        {
            var sharedFactory = factory;
            addlogging?.Invoke(self, (builder) => builder?.ClearProviders());
            MarvelLogProvider singleInstance = null;
            sharedFactory = (provider, cfg, opt) => singleInstance ?? (singleInstance = factory(provider, cfg, opt));

            self.Replace(ServiceDescriptor.Singleton<ILoggerFactory, LogFactory>(serviceProvider => new LogFactory(sharedFactory(serviceProvider, configuration, config))));

            self.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, MarvelLogProvider>(serviceProvider => sharedFactory(serviceProvider, configuration, config)));
        }

        private static void AddMLoggerProvider(IServiceCollection services, IConfiguration configuration, IHostEnvironment hostEnvironment, Mine.LogConfig options, Func<IServiceProvider, IConfiguration, IHostEnvironment, Mine.LogConfig, MarvelLogProvider> factory)
        {
            services.TryAddLoggingProvider((svc, addlogging) => svc.AddLogging(addlogging), configuration, options, (provider, cfg, opt) => factory(provider, cfg, hostEnvironment, opt));
        }



        private static MarvelLogProvider CreateProvider(Mine.LogConfig options)
        {
            MarvelLogProvider provider = new MarvelLogProvider(options);

            return provider;
        }
    }
}
