using CombatlogParser.Data.Metadata;
using CombatlogParser.Controls;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace CombatlogParser;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private ObservableCollection<string> searchedPlayerNames = new();
    private PlayerMetadata[] searchedPlayers = Array.Empty<PlayerMetadata>();

    public MainWindow()
    {
        InitializeComponent();

        PlayerSearchBox.SetBinding(ComboBox.ItemsSourceProperty,
            new Binding()
            {
                Source = searchedPlayerNames
            }
        );
    }

    //the actual dropdown.
    private void PlayerSearchSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        int index = PlayerSearchBox.SelectedIndex;
        if (index < 0 || index >= searchedPlayers.Length)
            return;
        var targetPlayer = searchedPlayers[index];
        var activeContent = MainContentControl.Content;
        if(activeContent != null && activeContent is PlayerMetadataView pmView)
        {
            pmView.SetPlayer(targetPlayer);
        }
        else MainContentControl.Content = new PlayerMetadataView(targetPlayer);
    }

    private void PlayerSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        PlayerSearchBox.IsDropDownOpen = true;
        searchedPlayerNames.Clear();
        searchedPlayers = Queries.FindPlayersWithNameLike(PlayerSearchBox.Text);
        foreach(var player in searchedPlayers)
        {
            searchedPlayerNames.Add(player!.Name);
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }

    private void TitleBar_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
            if (e.ClickCount == 2)
            {
                AdjustWindowSize();
            }
            else
            {
                this.DragMove();
            }
    }
    private void AdjustWindowSize()
    {
        if (this.WindowState == WindowState.Maximized)
        {
            this.WindowState = WindowState.Normal;
            //MaxButton.Content = "1";
        }
        else
        {
            this.WindowState = WindowState.Maximized;
            //MaxButton.Content = "2";
        }

    }
}
