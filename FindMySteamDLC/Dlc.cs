using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace FindMySteamDLC
{
    public class Dlc : Game
    {
        public Game FromTheGame { get; set; }
        public bool IsInstalled { get; set; }

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
