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
    private DBViewMode viewMode = DBViewMode.Combatlogs;
    private List<EntityBase?[]> bufferedPages = new();
    private int pageIndex = 0;
    private int totalEntriesForMode = 0;

    public RawDatabaseView()
    {
        InitializeComponent();

        CreateBinding(CombatlogListView, combatlogs);
        //var cls = Queries.GetCombatlogMetadata(0, 30);
        //ReplaceEntries(combatlogs, cls);

        CreateBinding(EncounterInfoListView, encounters);
        //var ens = Queries.GetEncounterInfoMetadata(0, 30);
        //ReplaceEntries(encounters, ens);

        CreateBinding(PerformancesListView, performances);
        //var perfs = Queries.GetPerformanceMetadata(0, 30);
        //ReplaceEntries(performances, perfs);

        CreateBinding(PlayersListView, players);
        //var pls = Queries.GetPlayerMetadata(0, 30);
        //ReplaceEntries(players, pls);
    }

    private static void CreateBinding(ListView listView, object dataSource)
    {
        var binding = new Binding
        {
            Source = dataSource
        };
        listView.SetBinding(ListView.ItemsSourceProperty, binding);
    }

    private static void ReplaceEntries(System.Collections.IList list, EntityBase?[] items)
    {
        list.Clear();
        foreach(var item in items)
        {
            if(item != null)
            {
                list.Add(item);
            }
        }
    }

    private void PlayerSearch_TextChanged(object sender, TextChangedEventArgs e)
    {
        if(string.IsNullOrEmpty(PlayerSearch.Text) || string.IsNullOrWhiteSpace(PlayerSearch.Text))
        {
            players.Clear();
            foreach (var p in Queries.GetPlayerMetadata(0, 30))
            {
                if (p != null)
                {
                    players.Add(p);
                }
            }
            return;
        }
        var results = Queries.FindPlayersWithNameLike(PlayerSearch.Text);
        players.Clear();
        foreach (var p in results)
            players.Add(p);
    }

    private void EncounterInfoListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        e.Handled = true;
        if (MainWindow is null)
            return;
        MainWindow.ChangeContent(new SingleEncounterView())
            .EncounterMetadata = (EncounterInfoMetadata)EncounterInfoListView.SelectedItem;
    }

    private void NextPageButton_Click(object sender, RoutedEventArgs eargs)
    {
        if(pageIndex == bufferedPages.Count - 1)
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
        PaginationTextBlock.Text = $"{items.FirstOrDefault()?.Id} - {items.LastOrDefault()?.Id} of {totalEntriesForMode}";
    }
    int counter = 0;
    private void TabSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        counter++;
        e.Handled = true;
        lastId = 0;
        pageIndex = -1;
        bufferedPages.Clear();
        var header = ((sender as TabControl)?.SelectedItem as TabItem)?.Header ?? string.Empty;
        
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
        PaginationTextBlock.Text = $"{items.FirstOrDefault()?.Id} - {items.LastOrDefault()?.Id} of {totalEntriesForMode} .. {counter}";
    }

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
        Combatlogs, Encounters, Performances, Players
    }
}
