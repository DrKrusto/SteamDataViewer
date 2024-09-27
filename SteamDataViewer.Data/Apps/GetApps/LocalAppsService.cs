using System.Text.RegularExpressions;
using Microsoft.Win32;
using SteamDataViewer.Data.Apps.Models;

namespace SteamDataViewer.Data.Apps.GetApps;

public interface ILocalAppsService
{
    string GetSteamPathFromRegistry();
    
    Task<IEnumerable<Game>> GetGamesFromFiles(string pathToSteam);
}

public class LocalAppsService : ILocalAppsService
{
    private string steamPathCache = string.Empty;
    
    private static readonly Regex AppIdRegex = new(@"appmanifest_(?'appId'\d*).acf", RegexOptions.Compiled);
    private static readonly Regex AcfFileRegex = new(
        @"""appid"".*""(?'appid'.*)""|""name"".*""(?'name'.*)"".*|""InstalledDepots""\s*\{(?:\s*""(?'depotid'\d+)""\s*\{[^{}]*\}\s*)+}",
        RegexOptions.Multiline
    );

    public string GetSteamPathFromRegistry()
    {
        if (!string.IsNullOrEmpty(steamPathCache))
            return steamPathCache;
        
        return steamPathCache = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Valve\Steam")?.GetValue("InstallPath")?.ToString()
               ?? Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Valve\Steam")?.GetValue("InstallPath")?.ToString()
               ?? string.Empty;
    }
    
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
            
            var depots = fileRegex
                .FirstOrDefault(x => x.Groups["depotid"].Success)?
                .Groups["depotid"]
                .Captures.Select(x => x.Value)
                .ToList();

            var game = new Game(
                fileRegex.First(x => x.Groups["appid"].Success).Groups["appid"].Value,
                fileRegex.First(x => x.Groups["name"].Success).Groups["name"].Value,
                true,
                [],
                depots ?? []
            );
            
            games.Add(game);
        }
        return games;
    }
}