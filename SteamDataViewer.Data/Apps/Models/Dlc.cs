namespace SteamDataViewer.Data.Apps.Models;

public record Dlc(string AppId, string Name, string ParentAppId, bool IsInstalled) : App(AppId, Name, IsInstalled);