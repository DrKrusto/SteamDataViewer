using System.ComponentModel;
using SteamDataViewer.Data.Apps.Models;

namespace SteamDataViewer.UI.DTO;

public class GameViewModel(string AppId, string Name, bool IsInstalled)
{
    public static GameViewModel From(Game game) => new(game.AppId, game.Name, game.IsInstalled);
};