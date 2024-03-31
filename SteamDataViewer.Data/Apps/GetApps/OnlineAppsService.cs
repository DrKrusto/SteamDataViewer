using System.Net;
using System.Text.Json;
using HtmlAgilityPack;
using SteamDataViewer.Data.Apps.Models;

namespace SteamDataViewer.Data.Apps.GetApps;

public class OnlineAppsService
{
    public async Task<IEnumerable<Dlc>> GetDlcs(string gameAppId)
    {
        var url = $"https://store.steampowered.com/dlc/{gameAppId}/";
        var htmlWeb = new HtmlWeb();
        
        var fixedDlcName = (await htmlWeb.LoadFromWebAsync(url))
            .DocumentNode
            .SelectSingleNode("//div[contains(concat(' ', normalize-space(@class), ' '), ' curator_avatar_image ')]/a")
            .Attributes["href"].Value
            .Split('/')[5];
        
        url += $"{fixedDlcName}/ajaxgetfilteredrecommendations";

        using var httpClient = new HttpClient();
        var json = await httpClient.GetStringAsync(url);
        
        using var document = JsonDocument.Parse(json);
        var html = document.RootElement.GetProperty("results_html").GetString();
        
        var htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(html);
        
        var dlcNodes = htmlDocument.DocumentNode
            .SelectNodes("//div[contains(concat(' ', normalize-space(@class), ' '), ' recommendation ')]");
        
        if (dlcNodes == null) return new List<Dlc>();
        
        var dlcs = new List<Dlc>();
        
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
        
        return dlcs;
    }
}