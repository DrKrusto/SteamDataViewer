using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Ookii.Dialogs.Wpf;

namespace FindMySteamDLC
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            SteamInfo.InitializeSteamLibrary(this.grid_loading);
            this.lb_games.ItemsSource = SteamInfo.Games;
            this.lb_games.Items.SortDescriptions.Add(new System.ComponentModel.SortDescription("Name", System.ComponentModel.ListSortDirection.Ascending));
            this.grid_loading.DataContext = SteamInfo.Loader;
        }

        private void MoveWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && this.IsActive)
            {
                DragMove();
            }
        }

        private void ExitApplication(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void MinimizeApplication(object sender, MouseButtonEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.lb_games.SelectedIndex != -1)
            {
                Game game = (Game)this.lb_games.SelectedItem;
                if (game.PathToImage == null || !File.Exists(game.PathToImage))
                {
                    this.img_gameImage.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/unknownimg.png"));
                }
                else
                {
                    this.img_gameImage.Source = new BitmapImage(new Uri(game.PathToImage));
                }
                this.lb_dlcs.ItemsSource = game.Dlcs;
            }
        }

        async private void SearchGames(object sender, MouseButtonEventArgs e)
        {
            VistaFolderBrowserDialog openFileDialog = new VistaFolderBrowserDialog() { Description = "Select the Steam directory where your games are located...", SelectedPath = SteamInfo.PathToSteam, UseDescriptionForTitle = true };
            if (openFileDialog.ShowDialog() == true)
            {
                if (Directory.Exists(String.Format(@"{0}\steamapps", openFileDialog.SelectedPath)))
                {
                    foreach (Game g in SteamInfo.FetchGamesFromSteam(openFileDialog.SelectedPath))
                    {
                        if (!SteamInfo.Games.Any(i => i.AppID == g.AppID))
                        {
                            SteamInfo.Games.Add(g);
                        }
                    }
                }
                this.grid_loading.IsEnabled = true;
                await Task.Run(()=> SteamInfo.FetchAllNonInstalledDlc());
                this.grid_loading.IsEnabled = false;
            }
        }
    }
}
