using SteamDataViewer.Data.Apps.GetApps;
using SteamDataViewer.Data.Apps.Models;

namespace SteamDataViewer.Data;

public class Program
{
    private static async Task Main(string[] args)
    {
        var localAppsService = new LocalAppsService();
        var onlineAppsService = new OnlineAppsService();

        var hello = localAppsService.GetSteamPathFromRegistry();
        if (!string.IsNullOrEmpty(hello))
        {
            var games = await localAppsService.GetGamesFromFiles(hello);
            
            foreach (var game in games)
            {
                var dlcsResult = await onlineAppsService.GetDlcs(game.AppId);
                if (dlcsResult.IsSuccess)
                {
                    var slkd = dlcsResult.Value;
                }
            }
        }


        Console.ReadLine();
    }
}