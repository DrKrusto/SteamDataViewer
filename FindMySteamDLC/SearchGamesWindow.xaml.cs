using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using FindMySteamDLC.Handlers;
using FindMySteamDLC.Models;
using HtmlAgilityPack;

namespace FindMySteamDLC
{
    /// <summary>
    /// Logique d'interaction pour SearchGamesWindow.xaml
    /// </summary>
    public partial class SearchGamesWindow : Window
    {
        private List<Game> games;

        public SearchGamesWindow()
        {
            InitializeComponent();
        }

        async private void Button_Click(object sender, RoutedEventArgs e)
        {
            string text = this.tb_gameName.Text;
            this.img_loadingBuffering.Opacity = 100;
            try
            {
                this.lb_foundGames.ItemsSource = await Task.Run(() => FetchGames(text));
            }
            catch (Exception ex)
            {
                this.lbl_errorMessage.Content = "The application couldn't fetch any games.\nThe error is most likely due to an internet connection problem.";
                Console.WriteLine(ex);
            }
            this.img_loadingBuffering.Opacity = 0;
        }
        
        private void LoadingGifAnimation()
        {

        }

        async private Task<List<Game>> FetchGames(string query)
        {
            List<Game> inDbGames = new List<Game>();
            foreach (Game g in SteamInfo.Games)
            {
                inDbGames.Add(g);
            }
            this.games = new List<Game>();
            string url = @"https://store.steampowered.com/search/results?count=50&category1=998&term=" + query;
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(url);
            HtmlNodeCollection nodes = doc.DocumentNode
                .SelectNodes("//a[contains(concat(' ', normalize-space(@class), ' '), ' search_result_row ')]");
            if (nodes != null)
            {
                foreach (HtmlNode node in nodes)
                {
                    string appid = node.Attributes["data-ds-appid"].Value;
                    if (!appid.Contains(',')) // Some nodes are not games but collections of games hence it contains multiple appids.
                    {
                        if (!inDbGames.Exists(i => i.AppID == Convert.ToInt32(appid)))
                        {
                            string name = node.SelectSingleNode("div[2]/div[1]/span").InnerText;
                            Game game = new Game()
                            {
                                AppID = Convert.ToInt32(appid),
                                Name = name,
                                IsInstalled = false
                            };
                            this.games.Add(game);
                        }
                    }
                }
            }
            return await Task.FromResult(this.games);
        }

        private void tb_gameName_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Button_Click(sender, null);
            }
        }

        async private void AddGame(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            Game game = (Game)button.DataContext;
            await SteamInfo.AddGame(game);
            this.games.Remove(game);
        }
    }
}
