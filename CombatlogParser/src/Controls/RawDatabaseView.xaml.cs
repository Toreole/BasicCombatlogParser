using CombatlogParser.Data.Metadata;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace CombatlogParser.Controls;

/// <summary>
/// Interaction logic for RawDatabaseView.xaml
/// </summary>
public partial class RawDatabaseView : ContentView
{
    private readonly ObservableCollection<CombatlogMetadata> combatlogs = new();
    private readonly ObservableCollection<EncounterInfoMetadata> encounters = new();
    private readonly ObservableCollection<PerformanceMetadata> performances = new();
    private readonly ObservableCollection<PlayerMetadata> players = new();
    private uint lastId;
    private DBViewMode viewMode = DBViewMode.EMPTY;
    private readonly List<EntityBase?[]> bufferedPages = new();
    private int pageIndex = 0;
    private int totalEntriesForMode = 0;

    public RawDatabaseView()
    {
        InitializeComponent();

        CreateBinding(CombatlogListView, combatlogs);

        CreateBinding(EncounterInfoListView, encounters);

        CreateBinding(PerformancesListView, performances);

        CreateBinding(PlayersListView, players);
    }

    /// <summary>
    /// Binds a ListViews ItemsSourceProperty to the provided dataSource.
    /// </summary>
    /// <param name="listView">Any <see cref="ListView"/></param>
    /// <param name="dataSource">Should be anything that implements INotifyCollectionChanged and/or INotifyPropertyChanged. Usually <see cref="ObservableCollection{T}"/></param>
    private static void CreateBinding(ListView listView, object dataSource)
    {
        var binding = new Binding
        {
            Source = dataSource
        };
        listView.SetBinding(ListView.ItemsSourceProperty, binding);
    }

    /// <summary>
    /// Clears a list and fills it with the provided items.
    /// List should be of compatible type. Make sure of that before passing both into this method.
    /// </summary>
    /// <param name="list">Any List compatible with the Type of items</param>
    /// <param name="items"></param>
    private static void ReplaceEntries(System.Collections.IList list, EntityBase?[] items)
    {
        list.Clear();
        foreach (var item in items)
        {
            if (item != null)
            {
                list.Add(item);
            }
        }
    }

    /// <summary>
    /// NOT TO BE CALLED MANUALLY
    /// Event for when the PlayerSearch textbox's text has changed (aka when the user has input something)
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void PlayerSearch_TextChanged(object sender, TextChangedEventArgs e)
    {
        e.Handled = true;
        if (string.IsNullOrEmpty(PlayerSearch.Text) || string.IsNullOrWhiteSpace(PlayerSearch.Text))
        {
            ResetPagination();
            FetchData();
            return;
        }
        ReplaceEntries(players, Queries.FindPlayersWithNameLike(PlayerSearch.Text));
    }

    private void EncounterInfoListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        e.Handled = true;
        if (MainWindow is null)
            return;
        MainWindow.ChangeContent(new SingleEncounterView())
            .EncounterMetadata = (EncounterInfoMetadata)EncounterInfoListView.SelectedItem;
    }

    private void NextPageButton_Click(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
        if (pageIndex == bufferedPages.Count - 1)
        {
            FetchData();
        }
        else
        {
            pageIndex++;
            ShowBufferedPage();
        }
    }

    private void PreviousPageButton_Click(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
        if (pageIndex == 0)
            return;
        pageIndex--;
        ShowBufferedPage();
    }

    private void ShowBufferedPage()
    {
        var list = RelevantList;
        list.Clear();
        var items = bufferedPages[pageIndex];
        ReplaceEntries(list, items);
        UpdatePaginationText(items);
    }

    //for some silly reason, TabSelectionChanged is being called when you click on an item in any of the lists.
    //even weirder, its being called multiple times.
    private void TabSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        e.Handled = true;
        if (sender is not TabControl)
            return;
        var header = ((sender as TabControl)?.SelectedItem as TabItem)?.Header ?? string.Empty;
        DBViewMode currentMode = viewMode;

        switch (header)
        {
            case "Combatlogs":
                viewMode = DBViewMode.Combatlogs;
                totalEntriesForMode = Queries.CountCombatlogMetadata();
                break;
            case "Encounters":
                viewMode = DBViewMode.Encounters;
                totalEntriesForMode = Queries.CountEncounterMetadata();
                break;
            case "Performances":
                viewMode = DBViewMode.Performances;
                totalEntriesForMode = Queries.CountPerformanceMetadata();
                break;
            case "Players":
                viewMode = DBViewMode.Players;
                totalEntriesForMode = Queries.CountPlayerMetadata();
                break;
        }
        if(viewMode == currentMode) //no change, dont add new data
        {
            return;
        }
        ResetPagination();
        FetchData();
    }

    private void FetchData()
    {
        System.Collections.IList list = RelevantList;
        var items = GetData();
        bufferedPages.Add(items);
        pageIndex++;
        ReplaceEntries(list, items);
        lastId = items.LastOrDefault()?.Id ?? lastId;
        UpdatePaginationText(items);
    }

    private void ResetPagination()
    {
        lastId = 0;
        pageIndex = -1;
        bufferedPages.Clear();
    }

    private void UpdatePaginationText(EntityBase?[] items)
    {
        PaginationTextBlock.Text = $"{items.FirstOrDefault()?.Id} - {items.LastOrDefault()?.Id} of {totalEntriesForMode}";
    }

    // This may not be great, but its the only way i could think of at the time
    // to provide a as-generic-as-possible type return for any of the ObservableCollections
    private System.Collections.IList RelevantList
        => viewMode switch
        {
            DBViewMode.Combatlogs => combatlogs,
            DBViewMode.Encounters => encounters,
            DBViewMode.Players => players,
            DBViewMode.Performances => performances,
            _ => throw new NotImplementedException()
        };

    private EntityBase?[] GetData()
    {
        return viewMode switch
        {
            DBViewMode.Combatlogs => Queries.GetCombatlogMetadata(lastId, 30),
            DBViewMode.Encounters => Queries.GetEncounterInfoMetadata(lastId, 30),
            DBViewMode.Performances => Queries.GetPerformanceMetadata(lastId, 30),
            DBViewMode.Players => Queries.GetPlayerMetadata(lastId, 30),
            _ => Array.Empty<EntityBase?>()
        };
    }

    private enum DBViewMode
    {
        EMPTY, Combatlogs, Encounters, Performances, Players
    }
}
