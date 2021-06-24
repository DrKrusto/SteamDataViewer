using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows;

namespace FindMySteamDLC
{
    static public class SteamInfo
    {
        static private string pathToSteam = SteamInfo.FetchSteamRepository();
        static private ObservableCollection<Game> games;

        static public ObservableCollection<Game> Games
        {
            get { return SteamInfo.games; }
            set { SteamInfo.games = value; }
        }

        static public string PathToSteam
        {
            get { return SteamInfo.pathToSteam; }
        }

        static public string FetchSteamRepository()
        {
            RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Valve\Steam");
            if (key == null)
            {
                key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Valve\Steam");
                if (key == null)
                {
                    MessageBox.Show("There is no Steam directory found on this computer. The program will close.");
                    Environment.Exit(0);
                    return null;
                }
                else
                    return key.GetValue("InstallPath").ToString();
            }
            else
                return key.GetValue("InstallPath").ToString();
        }

        static public void InitializeSteamLibrary()
        {
            SteamInfo.Games = new ObservableCollection<Game>();
            foreach (Game g in SteamInfo.FetchGamesFromSteam(SteamInfo.PathToSteam))
            {
                SteamInfo.Games.Add(g);
            }
        }

        static public ICollection<Game> FetchGamesFromSteam(string pathToSteam)
        {
            ICollection<Game> games = new List<Game>();
            foreach (string s in Directory.EnumerateFiles(pathToSteam + "\\steamapps"))
            {
                if (s.Contains(".acf"))
                {
                    using (StreamReader reader = new StreamReader(s))
                    {
                        Game game = new Game() { AppID = -1, Dlcs = new List<Dlc>() };
                        while (!reader.EndOfStream)
                        {
                            string line = reader.ReadLine();
                            if (line.Contains("\"appid\""))
                            {
                                game.AppID = Convert.ToInt32(line.Split('"')[3].Trim());
                            }
                            else
                            {
                                if (line.Contains("\"name\""))
                                {
                                    game.Name = line.Split('"')[3].Trim();
                                }
                                else
                                {
                                    if (line.Contains("\"dlcappid\""))
                                    {
                                        game.Dlcs.Add(new Dlc() { AppID = Convert.ToInt32(line.Split('"')[3].Trim()) });
                                    }
                                }
                            }
                        }
                        games.Add(game);
                    }
                }
            }
            return games;
        }
    }
}
