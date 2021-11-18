using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Text;
using System.Windows;
using System.Threading.Tasks;
using System.Windows.Controls;
using FindMySteamDLC.Models;
using System.Text.Json;
using HtmlAgilityPack;

namespace FindMySteamDLC.Handlers
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
            foreach (Game g in await Task.Run(()=> SteamInfo.FetchGamesFromSteamFiles(SteamInfo.PathToSteam)))
            {
                SteamInfo.Games.Add(g);
            }
            loadingGrid.IsEnabled = false;
        }

        async static public Task<ICollection<Game>> FetchGamesFromSteamFiles(string pathToSteam)
        {
            List<Game> games = new List<Game>(SteamInfo.Games);
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
                                            Dlc dlc = new Dlc(game)
                                            {
                                                AppID = dlcAppID,
                                                IsInstalled = true,
                                            };
                                            game.Dlcs.Add(dlcAppID, dlc);
                                        }
                                    }
                                }
                            }
                            await SteamInfo.FetchDlcs(game);
                            foundGames.Add(game);
                            //await Task.Run(() => SteamInfo.FetchNonInstalledDlc(game, foundGames));
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

        async static public Task<bool> FetchDlcs(Game game)
        {
            string json = String.Empty, html = String.Empty, fixedName = String.Empty, url = String.Format("https://store.steampowered.com/dlc/{0}/", game.AppID);
            HtmlWeb htmlWeb = new HtmlWeb();
            try
            {
                fixedName = htmlWeb.Load(url)
                    .DocumentNode
                    .SelectSingleNode("//div[contains(concat(' ', normalize-space(@class), ' '), ' curator_avatar_image ')]/a")
                    .Attributes["href"].Value
                    .Split('/')[5];
            }
            catch(Exception e)
            {
                Console.WriteLine("This game is not found in the steam site. Exception: " + e);
                return await Task.FromResult(false);
            }
            url += String.Format("{0}/{1}", fixedName, "ajaxgetfilteredrecommendations");
            using (WebClient client = new WebClient())
            {
                json = client.DownloadString(url);
                using (JsonDocument document = JsonDocument.Parse(json))
                {
                    html = document.RootElement.GetProperty("results_html").GetString();
                }
            }
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);
            HtmlNodeCollection dlcNodes = htmlDoc.DocumentNode
                .SelectNodes("//div[contains(concat(' ', normalize-space(@class), ' '), ' recommendation ')]");
            if (dlcNodes != null)
            {
                foreach (HtmlNode node in dlcNodes)
                {
                    int appid = Convert.ToInt32(node
                        .SelectSingleNode("div/a")
                        .Attributes["data-ds-appid"].Value);
                    string dlcName = node
                        .SelectSingleNode("a/div/div[1]/div/span[1]")
                        .InnerText;
                    if (game.Dlcs.ContainsKey(appid))
                    {
                        game.Dlcs[appid].Name = dlcName;
                    }
                    else
                    {
                        Dlc dlc = new Dlc(game)
                        {
                            Name = dlcName,
                            AppID = appid,
                            IsInstalled = false
                        };
                        game.Dlcs.Add(appid, dlc);
                    }
                    if (!File.Exists(String.Format(@"{0}\appcache\librarycache\{1}_header.jpg", SteamInfo.PathToSteam, appid)))
                    {
                        using (WebClient client = new WebClient())
                        {
                            try 
                            { 
                                client.DownloadFile(String.Format("http://cdn.akamai.steamstatic.com/steam/apps/{0}/header.jpg", appid), String.Format(@"{0}\appcache\librarycache\{1}_header.jpg", SteamInfo.PathToSteam, appid)); 
                            }
                            catch (Exception e) 
                            {
                                Console.WriteLine("Couldn't download the dlc image. Exception: " + e.Message);
                            }
                        }
                        
                    }
                }
            }
            return await Task.FromResult(true);
        }

        async static public Task AddGame(Game game)
        {
            if (game.PathToImage == "pack://application:,,,/Resources/unknownimg.png")
            {
                using (WebClient client = new WebClient())
                {
                    try
                    {
                        client.DownloadFile(String.Format("http://cdn.akamai.steamstatic.com/steam/apps/{0}/header.jpg", game.AppID), String.Format(@"{0}\appcache\librarycache\{1}_header.jpg", SteamInfo.PathToSteam, game.AppID));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Couldn't download the game image. Exception: " + e.Message);
                    }
                }
            }
            await Task.Run(() => SteamInfo.FetchDlcs(game));
            SteamInfo.Games.Add(game);
            SQLiteHandler.InsertGame(game);
        }

        async static public void FetchAllNonInstalledDlc()
        {
            foreach (Game game in SteamInfo.games)
            {
                await SteamInfo.FetchDlcs(game);
            }
            SQLiteHandler.InsertGames(SteamInfo.games);
        }
    }
}
