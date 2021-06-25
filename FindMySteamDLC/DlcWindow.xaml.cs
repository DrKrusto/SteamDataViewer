using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FindMySteamDLC
{
    /// <summary>
    /// Logique d'interaction pour DlcWindow.xaml
    /// </summary>
    public partial class DlcWindow : Window
    {
        private Game theGame;

        public DlcWindow(Game game)
        {
            InitializeComponent();
            this.theGame = game;
            this.lb_dlcs.ItemsSource = this.theGame.Dlcs.FindAll(i => i.IsInstalled == false);
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
    }
}
