namespace SteamDataViewer.Data.Apps.Models;

public record Game(string AppId, string Name, bool IsInstalled, List<Dlc> Dlcs, IEnumerable<string> Depots) : App(AppId, Name, IsInstalled);