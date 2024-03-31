using FluentResults;
using SteamDataViewer.Data.Apps.GetApps;
using SteamDataViewer.Data.Apps.Models;

namespace SteamDataViewer.Data.Apps;

public interface IAppsService
{
    Task<Result<IEnumerable<Game>>> GetGames();
}

public class AppsService(ILocalAppsService localAppsService, IOnlineAppsService onlineAppsService) : IAppsService
{
    private readonly ILocalAppsService localAppsService = localAppsService ?? throw new ArgumentNullException(nameof(localAppsService));
    private readonly IOnlineAppsService onlineAppsService = onlineAppsService ?? throw new ArgumentNullException(nameof(onlineAppsService));

    public async Task<Result<IEnumerable<Game>>> GetGames()
    {
        var steamPath = localAppsService.GetSteamPathFromRegistry();
        if (string.IsNullOrEmpty(steamPath))
        {
            return Result.Fail("Steam path not found.");
        }
        
        var games = await localAppsService.GetGamesFromFiles(steamPath);
        foreach (var game in games)
        {
            var dlcsResult = await onlineAppsService.GetDlcs(game.AppId);
            if (dlcsResult.IsSuccess)
            {
                game.AddDlcs(dlcsResult.Value);
            }
        }
        
        return Result.Ok(games);
    }
}