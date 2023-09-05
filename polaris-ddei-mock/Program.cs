using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WireMock.Settings;

namespace WireMock.Net.WebApplication;

public static class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    private static IHostBuilder CreateHostBuilder(string[] args)
        => Host.CreateDefaultBuilder(args)
            .ConfigureServices((host, services) => ConfigureServices(services, host.Configuration));

    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddLogging(logging => logging.AddConsole().AddDebug());

        services.AddTransient<IWireMockService, WireMockService>();
#if DEBUG
        services.Configure<WireMockServerSettings>(configuration.GetSection("WireMockServerSettingsLocal"));
#else
        services.Configure<WireMockServerSettings>(configuration.GetSection("WireMockServerSettings"));
#endif

        services.AddHostedService<App>();
    }
}