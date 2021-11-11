using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace FindMySteamDLC.Models
{
    public class Game : Media
    {
        public Game()
        {
            this.Dlcs = new Dictionary<int, Dlc>();
            this.AppID = -1;
            this.IsInstalled = false;
            this.Name = "null";
        }

        public Dictionary<int, Dlc> Dlcs { get; set; }
    }
}
