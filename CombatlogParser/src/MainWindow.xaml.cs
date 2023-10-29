using CombatlogParser.Controls;
using CombatlogParser.Data.Metadata;
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
    private readonly ObservableCollection<string> searchedPlayerNames = new();
    private PlayerMetadata[] searchedPlayers = Array.Empty<PlayerMetadata>();
    private LabelledProgressBar progressBar;

    public MainWindow()
    {
        InitializeComponent();
        progressBar = new LabelledProgressBar();

        PlayerSearchBox.SetBinding(ComboBox.ItemsSourceProperty,
            new Binding()
            {
                Source = searchedPlayerNames
            }
        );

        var defaultContent = MainContentControl.Content;
        if (defaultContent is ContentView view)
        {
            view.SetWindow(this);
        }
    }

    internal T ChangeContent<T>(T content) where T : ContentView
    {
        content.SetWindow(this);
        MainContentControl.Content = content;
        return content;
    }

    //the actual dropdown.
    private void PlayerSearchSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        e.Handled = true;
        int index = PlayerSearchBox.SelectedIndex;
        if (index < 0 || index >= searchedPlayers.Length)
            return;
        var targetPlayer = searchedPlayers[index];
        var activeContent = MainContentControl.Content;
        if (activeContent != null && activeContent is PlayerMetadataView pmView)
        {
            pmView.SetPlayer(targetPlayer);
        }
        else this.ChangeContent(new PlayerMetadataView(targetPlayer));
    }

    private void PlayerSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        e.Handled = true;
        //PlayerSearchBox.IsDropDownOpen = true;
        PlayerSearchBox.SelectedIndex = -1;
        searchedPlayerNames.Clear();
        searchedPlayers = Queries.FindPlayersWithNameLike(PlayerSearchBox.Text);
        foreach (var player in searchedPlayers)
        {
            if (player != null)
                searchedPlayerNames.Add(player!.Name);
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
        this.Close();
    }

    private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
        if (e.ChangedButton != MouseButton.Left)
            return;
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

    private async void ImportLogButtonClicked(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            FileName = System.IO.Path.Combine(Config.Default.WoW_Log_Directories, "Combatlog"),
            DefaultExt = ".txt",
            Filter = "Logs (.txt)|*.txt"
        };

        bool? result = dialog.ShowDialog();
        if (result == true)
        {
            //CombatLogParser.ImportCombatlog(dialog.FileName);
            var progressBar = ShowProgressBar();
            await CombatLogParser.ImportCombatlogAsync(dialog.FileName, progressBar);
            HideProgressBar(progressBar);
        }
    }

    private void DBViewButton_Click(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
        if (MainContentControl.Content is not RawDatabaseView)
            this.ChangeContent(new RawDatabaseView());
    }

    private async void TestButton_Click(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
        ChangeContent(new CombatlogOverviewView())
            .FromEncounterMetadata(Queries.GetEncounterInfoMetadata(0, 1).First()!);

        return;
        //LabelledProgressBar labelledProgress = PopupOverlay.Children.OfType<LabelledProgressBar>().First();
        var progressBar = ShowProgressBar();
        TestButton.IsEnabled = false;
        for (int i = 0; i <= 100; i++)
        {
            progressBar.ProgressPercent = i;
            progressBar.DescriptionText = $"Working... {i}%";

            await Task.Delay(70);
        }
        await Task.Delay(200);
        TestButton.IsEnabled = true;
        HideProgressBar(progressBar);
    }

    public LabelledProgressBar ShowProgressBar()
    {
        PopupOverlay.Children.Add(progressBar);
        PopupOverlay.Visibility = Visibility.Visible;
        progressBar.ProgressPercent = 0;
        return progressBar;
    }

    public void HideProgressBar(LabelledProgressBar bar)
    {
        PopupOverlay.Children.Remove(bar);
        PopupOverlay.Visibility = PopupOverlay.Children.Count == 0 ? Visibility.Hidden : Visibility.Visible;
    }
}
