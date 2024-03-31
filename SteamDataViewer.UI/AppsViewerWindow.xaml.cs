using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using SteamDataViewer.Data.Apps;
using SteamDataViewer.UI.DTO;

namespace SteamDataViewer.UI;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class AppsViewerWindow : Window
{
    private readonly IAppsService appService;

    public ObservableCollection<GameViewModel> Apps { get; set; }
    
    public AppsViewerWindow(IAppsService appService)
    {
        this.appService = appService ?? throw new ArgumentNullException(nameof(appService));
        
        Apps = [new GameViewModel("LDSKmlsDQK", "Test Game 1", true), new GameViewModel("LDSKmlsDQK", "Test Game 1", true)];
        
        InitializeComponent();
        BindData();
    }
    
    private void BindData()
    {
        var binding = new Binding
        {
            Source = Apps,
            Mode = BindingMode.OneWay
        };
        
        AppsListView.SetBinding(ItemsControl.ItemsSourceProperty, binding);
    }

    private async void GetGames()
    {
        var result = await appService.GetGames();
        if (result.IsFailed)
        {
            MessageBox.Show(result.Errors.First().Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        
        foreach (var game in result.Value)
        {
            Apps.Add(GameViewModel.From(game));
        }
    }
}