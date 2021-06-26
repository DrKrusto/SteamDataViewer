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
        static public Loader Loader { get; set; }

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

        async static public void InitializeSteamLibrary(Grid loadingGrid) //loadingGrid is to show or hide the loading screen!
        {
            SteamInfo.Loader = new Loader(false);
            SteamInfo.Games = new ObservableCollection<Game>();
            foreach (Game g in SteamInfo.FetchGamesFromSteam(SteamInfo.PathToSteam))
            {
                SteamInfo.Games.Add(g);
            }
            loadingGrid.IsEnabled = true;
            await Task.Run(()=> SteamInfo.FetchAllNonInstalledDlc());
            loadingGrid.IsEnabled = false;
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
                                        int dlcAppID = Convert.ToInt32(line.Split('"')[3].Trim());
                                        string json;
                                        using (WebClient wc = new WebClient())
                                        {
                                            json = wc.DownloadString("https://steamspy.com/api.php?request=appdetails&appid=" + dlcAppID).Trim(new char[] { '{', '}' });
                                            json = json.Split(',')[1];
                                            json = json.Split(':')[1];
                                            json = json.Trim('\"');
                                        }
                                        game.Dlcs.Add(new Dlc() { AppID = dlcAppID, IsInstalled = true, Name = json});
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

        static public void FetchNonInstalledDlc(Game game)
        {
            SteamInfo.Loader.IsLoading = true;
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
                            //progressDialog.Description = String.Format("Found {0}", dlcJson);
                            dlcs.Add(new Dlc() { AppID = Convert.ToInt32(appid), IsInstalled = false, Name = dlcJson });
                        }
                    }
                    game.Dlcs.AddRange(dlcs);
                }
            }
            SteamInfo.Loader.IsLoading = false;
        }

        async static public Task FetchAllNonInstalledDlc()
        {
            //ProgressDialog progressDialog = new ProgressDialog() { ProgressBarStyle = ProgressBarStyle.None, ShowCancelButton = false };
            //progressDialog.Show();
            foreach (Game game in SteamInfo.games)
            {
                //progressDialog.Description = "Searching DLC's...";
                if (!game.HasBeenFetchForDlcs)
                {
                    //progressDialog.Text = game.Name;
                    SteamInfo.FetchNonInstalledDlc(game);
                    game.HasBeenFetchForDlcs = true;
                }
            }
            await Task.Delay(50);
        }
    }

    public class Loader
    {
        public Loader(bool isLoading)
        {
            this.IsLoading = isLoading;
        }

        public bool IsLoading { get; set; }
    }
}
