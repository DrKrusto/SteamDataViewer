using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace FindMySteamDLC.Models
{
    public class Dlc : Media
    {
        public Dlc(Game game)
        {
            this.Name = "null";
            this.IsInstalled = false;
            this.AppID = -1;
            this.FromTheGame = game;
        }

        public Game FromTheGame { get; set; }
    }
}
