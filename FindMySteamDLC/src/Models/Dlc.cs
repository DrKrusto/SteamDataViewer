using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace FindMySteamDLC.Models
{
    public class Dlc : Media
    {
        public Game FromTheGame { get; set; }
    }
}
