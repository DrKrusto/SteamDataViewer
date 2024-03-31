using System.Text.Json;
using FluentResults;
using HtmlAgilityPack;
using SteamDataViewer.Data.Apps.Models;

namespace SteamDataViewer.Data.Apps.GetApps;

public interface IOnlineAppsService
{
    Task<Result<IEnumerable<Dlc>>> GetDlcs(string gameAppId);
}

public class OnlineAppsService : IOnlineAppsService
{
    public async Task<Result<IEnumerable<Dlc>>> GetDlcs(string gameAppId)
    {
        var dlcs = new List<Dlc>();
        
        var url = $"https://store.steampowered.com/dlc/{gameAppId}/";
        var htmlWeb = new HtmlWeb();
        
        var loaded = await htmlWeb.LoadFromWebAsync(url);
        
        if (loaded == null) 
            return Result.Fail("Failed to load the page");
        
        var urlGameName = loaded
            .DocumentNode
            .SelectSingleNode("//div[contains(concat(' ', normalize-space(@class), ' '), ' curator_avatar_image ')]/a")?
            .Attributes["href"]?.Value?
            .Split('/')?[5];
        
        if (string.IsNullOrEmpty(urlGameName)) 
            return Result.Fail("Failed to get the URL game name");
        
        url += $"{urlGameName}/ajaxgetfilteredrecommendations";

        using var httpClient = new HttpClient();
        var json = await httpClient.GetStringAsync(url);
        
        using var document = JsonDocument.Parse(json);
        var html = document.RootElement.GetProperty("results_html").GetString();
        
        var htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(html);
        
        var dlcNodes = htmlDocument.DocumentNode
            .SelectNodes("//div[contains(concat(' ', normalize-space(@class), ' '), ' recommendation ')]");
        
        if (dlcNodes == null) 
            return Result.Fail("Failed to get the DLC nodes");

        foreach (var node in dlcNodes)
        {
            var appId = node
                .SelectSingleNode("div/a")
                .Attributes["data-ds-appid"].Value;
            
            var dlcName = node
                .SelectSingleNode("a/div/div[1]/div/span[1]")
                .InnerText;
            
            dlcs.Add(new Dlc(appId, dlcName, gameAppId, false));
        }
        
        return Result.Ok<IEnumerable<Dlc>>(dlcs);
    }
}