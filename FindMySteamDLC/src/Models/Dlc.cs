using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using FindMySteamDLC.Handlers;

namespace FindMySteamDLC.Models
{
    public class Dlc : Game
    {
        public Game FromTheGame { get; set; }

        public Dlc(Game fromTheGame)
        {
            this.Name = "null";
            this.AppID = -1;
            this.Dlcs = new List<Dlc>();
            this.HasBeenFetchForDlcs = false;
            this.FromTheGame = fromTheGame;
        }

        public override string PathToImage
        {
            get
            {
                string pathToImage = String.Format(@"{0}\appcache\librarycache\{1}_header.jpg", SteamInfo.PathToSteam, this.AppID);
                if (File.Exists(pathToImage))
                    return pathToImage;
                else
                {
                    if (this.IsInstalled)
                        return "pack://application:,,,/Resources/unknownimg.png";
                    else
                        return "pack://application:,,,/Resources/dlcnotdownloaded.png";
                }

            }
        }
    }
}
