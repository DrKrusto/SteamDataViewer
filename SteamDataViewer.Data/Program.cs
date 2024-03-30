using SteamDataViewer.Data.Apps.GetApps;

namespace SteamDataViewer.Data;

public class Program
{
    private static async Task Main(string[] args)
    {
        var localAppsService = new LocalAppsService();

        var hello = localAppsService.GetSteamPathFromRegistry();
        if (!string.IsNullOrEmpty(hello))
        {
            await localAppsService.GetGamesFromFiles(hello);
        }

        Console.ReadLine();
    }
}