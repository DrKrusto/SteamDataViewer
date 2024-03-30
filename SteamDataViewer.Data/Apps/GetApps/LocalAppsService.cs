using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace SteamDataViewer.Data.Apps.GetApps;

public record Game(string AppID, string Name, bool IsInstalled, Dictionary<int, Dlc> Dlcs);

public record Dlc(Game Game, int AppID, bool IsInstalled);

public interface ILocalAppsService
{
    string GetSteamPathFromRegistry();
    
    Task<IEnumerable<Game>> GetGamesFromFiles(string pathToSteam);
}

public class LocalAppsService : ILocalAppsService
{
    private static readonly Regex AppIdRegex = new(@"appmanifest_(?'appId'\d*).acf", RegexOptions.Compiled);
    private static readonly Regex AcfFileRegex = new(@"""appid"".*""(?'appid'.*)""|""name"".*""(?'name'.*)"".*", RegexOptions.Compiled);
    
    public string GetSteamPathFromRegistry()
    #pragma warning disable CA1416
    => Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Valve\Steam")?.GetValue("InstallPath")?.ToString()
       ?? Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Valve\Steam")?.GetValue("InstallPath")?.ToString()
       ?? string.Empty;
    #pragma warning restore CA1416
    
    public async Task<IEnumerable<Game>> GetGamesFromFiles(string pathToSteam)
    {
        var games = new List<Game>();
        foreach (var file in Directory.EnumerateFiles($"{pathToSteam}\\steamapps", "*.acf"))
        {
            var appid = AppIdRegex.Matches(file).FirstOrDefault()?.Groups["appId"].Value;
            if (appid == null) continue;
            
            using var reader = new StreamReader(file);
            var lines = await reader.ReadToEndAsync();
            
            var fileRegex = AcfFileRegex.Matches(lines);

            var foundGame = new Game(
                fileRegex.First(x => x.Groups["appid"].Success).Groups["appid"].Value,
                fileRegex.First(x => x.Groups["name"].Success).Groups["name"].Value,
                true,
                new Dictionary<int, Dlc>()
            );
            
            games.Add(foundGame);
        }
        return games;
    }
}