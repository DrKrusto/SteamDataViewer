namespace SteamDataViewer.Data.Apps.Models;

public record Game(string AppId, string Name, bool IsInstalled, List<Dlc> Dlcs, IEnumerable<string> Depots)
    : App(AppId, Name, IsInstalled)
{
    public void AddDlc(Dlc dlc) => Dlcs.Add(new Dlc(dlc.AppId, dlc.Name, dlc.ParentAppId, Depots.Contains(dlc.AppId)));

    public void AddDlcs(IEnumerable<Dlc> dlcs)
    {
        foreach (var dlc in dlcs)
            AddDlc(dlc);
    }
}