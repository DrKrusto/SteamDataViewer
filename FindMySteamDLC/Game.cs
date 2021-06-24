using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FindMySteamDLC
{
    public class Game
    {
        public string Name { get; set; }
        public int AppID { get; set; }
        public List<Dlc> Dlcs { get; set; }

        public string PathToIcon
        {
            get { return String.Format(@"{0}\appcache\librarycache\{1}_icon.jpg", SteamInfo.PathToSteam, this.AppID); }
        }

        public string PathToImage
        {
            get 
            {
                string pathToImage = String.Format(@"{0}\appcache\librarycache\{1}_header.jpg", SteamInfo.PathToSteam, this.AppID);
                if (File.Exists(pathToImage))
                    return pathToImage;
                else
                    return "pack://application:,,,/Resources/unknownimg.png";
            }
        }
    }
}
