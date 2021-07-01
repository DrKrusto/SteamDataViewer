using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Windows;
using Ookii.Dialogs.Wpf;
using System.Threading.Tasks;
using System.Windows.Controls;

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

        async static public void InitializeSteamLibrary(Grid loadingGrid, List<string> directories) //loadingGrid is to show or hide the loading screen!
        {
            SteamInfo.Games = new ObservableCollection<Game>();
            loadingGrid.IsEnabled = true;
            foreach (Game g in SQLiteHandler.FetchGames())
            {
                SteamInfo.Games.Add(g);
            }
            ICollection<Game> allGames = await Task.Run(()=> SteamInfo.FetchGamesFromSteam(SteamInfo.PathToSteam));
            foreach (Game g in allGames)
            {
                SteamInfo.Games.Add(g);
            }
            loadingGrid.IsEnabled = false;
        }

        async static public Task<ICollection<Game>> FetchGamesFromSteam(string pathToSteam)
        {
            List<Game> games = new List<Game>();
            games.AddRange(SteamInfo.Games);
            List<Game> foundGames = new List<Game>();
            foreach (string s in Directory.EnumerateFiles(pathToSteam + "\\steamapps"))
            {
                if (s.Contains(".acf"))
                {
                    string fileName = s.Split(@"\")[s.Split(@"\").Length-1];
                    int appid = Convert.ToInt32(fileName.Split('_')[1].Split('.')[0]);
                    if (!games.Exists(i => i.AppID == appid))   // Si le jeu existe déjà on cherche pas les informations
                    {
                        using (StreamReader reader = new StreamReader(s))
                        {
                            Game game = new Game() { IsInstalled = true };
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
                                            int dlcAppID = Convert.ToInt32(line.Split('"')[3].Trim());
                                            string json;
                                            using (WebClient wc = new WebClient())
                                            {
                                                json = wc.DownloadString("https://steamspy.com/api.php?request=appdetails&appid=" + dlcAppID).Trim(new char[] { '{', '}' });
                                                json = json.Split(',')[1];
                                                json = json.Split(':')[1];
                                                json = json.Trim('\"');
                                            }
                                            game.Dlcs.Add(new Dlc(game) { AppID = dlcAppID, IsInstalled = true, Name = json });
                                        }
                                    }
                                }
                            }
                            foundGames.Add(game);
                            game.Dlcs.AddRange(await Task.Run(() => SteamInfo.FetchNonInstalledDlc(game, foundGames)));
                        }
                    }
                    else
                    {
                        foreach (Game game in SteamInfo.Games)
                        {
                            if (game.AppID == appid)
                            {
                                game.IsInstalled = true;
                            }
                        }
                    }
                }
            }
            SQLiteHandler.InsertGames(foundGames);
            return await Task.FromResult(foundGames);
        }

        async static public Task<List<Dlc>> FetchNonInstalledDlc(Game game)
        {
            int indexOfGame = SteamInfo.Games.IndexOf(game);
            List<Dlc> dlcs = new List<Dlc>();
            string json;
            using (WebClient wc = new WebClient())
            {
                json = wc.DownloadString("http://store.steampowered.com/api/appdetails/?appids=" + game.AppID);
                if (json.Contains("\"dlc\""))
                {
                    json = json.Remove(0, json.IndexOf("\"dlc\"")).Split('[', ']')[1];
                    foreach (string appid in json.Split(','))
                    {
                        if (!SteamInfo.Games[indexOfGame].Dlcs.Exists(i => i.AppID == Convert.ToInt32(appid)))
                        {
                            string dlcJson = wc.DownloadString("https://steamspy.com/api.php?request=appdetails&appid=" + appid);
                            dlcJson = dlcJson.Split(',')[1];
                            try
                            {
                                dlcJson = dlcJson.Split('\"')[3];
                            }
                            catch
                            {
                                dlcJson = "Unknown name: " + appid;
                            }
                            dlcJson = dlcJson.Trim('\"');
                            dlcs.Add(new Dlc(game) { AppID = Convert.ToInt32(appid), IsInstalled = false, Name = dlcJson });
                            if (!File.Exists(String.Format(@"{0}\appcache\librarycache\{1}_header.jpg", SteamInfo.PathToSteam, appid)))
                            {
                                try { wc.DownloadFile(String.Format("http://cdn.akamai.steamstatic.com/steam/apps/{0}/header.jpg", appid), String.Format(@"{0}\appcache\librarycache\{1}_header.jpg", SteamInfo.PathToSteam, appid)); }
                                catch (Exception e) { }
                            }
                        }
                    }
                }
            }
            return await Task.FromResult(dlcs);
        }

        async static public Task<List<Dlc>> FetchNonInstalledDlc(Game game, List<Game> theGames)
        {
            int indexOfGame = theGames.FindIndex(i => i.AppID == game.AppID);
            List<Dlc> dlcs = new List<Dlc>();
            string json;
            using (WebClient wc = new WebClient())
            {
                json = wc.DownloadString("http://store.steampowered.com/api/appdetails/?appids=" + game.AppID);
                if (json.Contains("\"dlc\""))
                {
                    json = json.Remove(0, json.IndexOf("\"dlc\"")).Split('[', ']')[1];
                    foreach (string appid in json.Split(','))
                    {
                        if (!theGames[indexOfGame].Dlcs.Exists(i => i.AppID == Convert.ToInt32(appid)))
                        {
                            string dlcJson = wc.DownloadString("https://steamspy.com/api.php?request=appdetails&appid=" + appid);
                            dlcJson = dlcJson.Split(',')[1];
                            try
                            {
                                dlcJson = dlcJson.Split('\"')[3];
                            }
                            catch
                            {
                                dlcJson = "Unknown name: " + appid;
                            }
                            dlcJson = dlcJson.Trim('\"');
                            dlcs.Add(new Dlc(game) { AppID = Convert.ToInt32(appid), IsInstalled = false, Name = dlcJson });
                            if (!File.Exists(String.Format(@"{0}\appcache\librarycache\{1}_header.jpg", SteamInfo.PathToSteam, appid)))
                            {
                                try { wc.DownloadFile(String.Format("http://cdn.akamai.steamstatic.com/steam/apps/{0}/header.jpg", appid), String.Format(@"{0}\appcache\librarycache\{1}_header.jpg", SteamInfo.PathToSteam, appid)); }
                                catch (Exception e) { }
                            }
                        }
                    }
                }
            }
            return await Task.FromResult(dlcs);
        }

        async static public Task FetchAllNonInstalledDlc()
        {
            foreach (Game game in SteamInfo.games)
            {
                List<Dlc> dlcs = await Task.Run(()=> SteamInfo.FetchNonInstalledDlc(game));
                game.Dlcs.AddRange(dlcs);
            }
            List<Game> games = new List<Game>();
            games.AddRange(SteamInfo.games);
            SQLiteHandler.InsertGames(games);
        }
    }
}
