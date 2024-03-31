using Microsoft.Extensions.DependencyInjection;
using SteamDataViewer.Data.Configuration;
using SteamDataViewer.UI;

namespace SteamDataViewer.Host;

internal class Program
{
    private static IServiceProvider serviceProvider;
    
    [STAThread]
    private static void Main(string[] args)
    {
        ConfigureServices();
        RunApplication();
    }

    private static void RunApplication()
    {
        var app = new SteamDataViewerApp(serviceProvider);
        app.Run();
    }

    private static void ConfigureServices()
    {
        var services = new ServiceCollection();

        services.AddAppsService()
            .AddSingleton<AppsViewerWindow>();
        
        serviceProvider = services.BuildServiceProvider();
    }
}