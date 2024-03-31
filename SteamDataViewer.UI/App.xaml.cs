using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using SteamDataViewer.Data.Configuration;

namespace SteamDataViewer.UI;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class SteamDataViewerApp(IServiceProvider serviceProvider) : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        var mainWindow = serviceProvider.GetRequiredService<AppsViewerWindow>();
        mainWindow.Show();
    }
}