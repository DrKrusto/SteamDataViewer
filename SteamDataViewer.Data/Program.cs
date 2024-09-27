using SteamDataViewer.Data.Apps;
using SteamDataViewer.Data.Apps.GetApps;

namespace SteamDataViewer.Data;

public class Program
{
    private static async Task Main(string[] args)
    {
        var localAppsService = new LocalAppsService();
        var onlineAppsService = new OnlineAppsService();
        var appsService = new AppsService(localAppsService, onlineAppsService);
        
        var test = await appsService.GetGames();
        if (test.IsFailed)
        {
            throw new Exception(test.Errors.First().Message);
        }

        Console.WriteLine("Games:");
        foreach (var game in test.Value)
        {
            Console.WriteLine($"AppId: {game.AppId}, Name: {game.Name}, IsInstalled: {game.IsInstalled}");
            Console.WriteLine("DLCs:");
            foreach (var dlc in game.Dlcs)
            {
                Console.WriteLine($"AppId: {dlc.AppId}, Name: {dlc.Name}, ParentAppId: {dlc.ParentAppId}, IsInstalled: {dlc.IsInstalled}");
            }
        }

        Console.ReadLine();
    }
}