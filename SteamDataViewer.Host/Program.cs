using Microsoft.Extensions.DependencyInjection;
using SteamDataViewer.Data.Configuration;

namespace SteamDataViewer.Host;

internal class Program
{
    private static IServiceProvider serviceProvider;
    
    private static void Main(string[] args)
    {
        ConfigureServices();
    }

    private static void ConfigureServices()
    {
        var services = new ServiceCollection();
        services.AddAppsService();
        serviceProvider = services.BuildServiceProvider();
    }
}