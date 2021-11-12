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
using FindMySteamDLC.Models;
using HtmlAgilityPack;

namespace FindMySteamDLC
{
    /// <summary>
    /// Logique d'interaction pour SearchGamesWindow.xaml
    /// </summary>
    public partial class SearchGamesWindow : Window
    {
        public SearchGamesWindow()
        {
            InitializeComponent();
        }

        async private void Button_Click(object sender, RoutedEventArgs e)
        {
            string text = this.tb_gameName.Text;
            this.lb_foundGames.ItemsSource = await Task.Run(() => FetchGames(text));
        }

        async private Task<ObservableCollection<Game>> FetchGames(string query)
        {
            ObservableCollection<Game> foundGames = new ObservableCollection<Game>();
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
                    if (!appid.Contains(','))
                    {
                        string name = node.SelectSingleNode("div[2]/div[1]/span").InnerText;
                        Game game = new Game()
                        {
                            AppID = Convert.ToInt32(appid),
                            Name = name,
                            IsInstalled = false
                        };
                        foundGames.Add(game);
                    }
                }
            }
            return await Task.FromResult(foundGames);
        }
    }
}
