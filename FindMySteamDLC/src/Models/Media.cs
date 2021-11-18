using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.IO;
using System.ComponentModel.DataAnnotations;
using FindMySteamDLC.Handlers;

namespace FindMySteamDLC.Models
{
    public abstract class Media : INotifyPropertyChanged
    {
        [Key]
        public int AppID { get; set; }
        public string Name { get; set; } 
        private bool isInstalled;
        public bool IsInstalled
        {
            get { return this.isInstalled; }
            set
            {
                this.isInstalled = value;
                this.OnPropertyChanged();
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        public string PathToIcon
        {
            get 
            {
                string pathToIcon = String.Format(@"{0}\appcache\librarycache\{1}_icon.jpg", SteamInfo.PathToSteam, this.AppID);
                return File.Exists(pathToIcon) ? pathToIcon : "pack://application:,,,/Resources/unknownimg.png";
            }
        }

        public virtual string PathToImage
        {
            get
            {
                string pathToImage = String.Format(@"{0}\appcache\librarycache\{1}_header.jpg", SteamInfo.PathToSteam, this.AppID);
                if (File.Exists(pathToImage))
                    return pathToImage;
                else
                    return this.IsInstalled ? "pack://application:,,,/Resources/unknownimg.png" : "pack://application:,,,/Resources/dlcnotdownloaded.png";
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string isInstalled = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(isInstalled));
        }
    }
}
