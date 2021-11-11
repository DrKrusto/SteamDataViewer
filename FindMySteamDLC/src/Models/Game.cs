using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using FindMySteamDLC.Handlers;

namespace FindMySteamDLC.Models
{
    public class Game : INotifyPropertyChanged
    {
        private bool isInstalled;

        public string Name { get; set; }
        public int AppID { get; set; }
        public List<Dlc> Dlcs { get; set; }
        public bool HasBeenFetchForDlcs { get; set; }
        public bool IsInstalled 
        {
            get { return this.isInstalled; }
            set {
                this.isInstalled = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public Game()
        {
            this.Name = "null";
            this.AppID = -1;
            this.Dlcs = new List<Dlc>();
            this.HasBeenFetchForDlcs = false;
            this.IsInstalled = false;
        }

        public string PathToIcon
        {
            get { return String.Format(@"{0}\appcache\librarycache\{1}_icon.jpg", SteamInfo.PathToSteam, this.AppID); }
        }

        public virtual string PathToImage
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

        protected void OnPropertyChanged([CallerMemberName] string isInstalled = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(isInstalled));
        }
    }
}
