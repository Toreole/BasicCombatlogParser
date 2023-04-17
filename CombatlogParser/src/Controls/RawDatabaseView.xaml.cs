using CombatlogParser.Data.Metadata;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace CombatlogParser.Controls;

/// <summary>
/// Interaction logic for RawDatabaseView.xaml
/// </summary>
public partial class RawDatabaseView : UserControl
{

    private ObservableCollection<CombatlogMetadata> combatlogs = new();
    //private uint lastLogId;
    private ObservableCollection<EncounterInfoMetadata> encounters = new();
    //private uint lastEncounterId;
    private ObservableCollection<PerformanceMetadata> performances = new();
    private uint lastPerformanceId;
    private ObservableCollection<PlayerMetadata> players = new();
    //private uint lastPlayerId;

    public RawDatabaseView()
    {
        InitializeComponent();

        var combatlogsBinding = new Binding()
        {
            Source = combatlogs
        };
        CombatlogListView.SetBinding(ListView.ItemsSourceProperty, combatlogsBinding);

        var cls = Queries.GetCombatlogMetadata(0, 10);
        foreach (var c in cls)
            if (c != null)
                combatlogs.Add(c);

        var encountersBinding = new Binding()
        {
            Source = encounters
        };
        EncounterInfoListView.SetBinding(ListView.ItemsSourceProperty, encountersBinding);

        var ens = Queries.GetEncounterInfoMetadata(0, 30);
        foreach (var e in ens)
            if (e != null)
                encounters.Add(e);

        var performancesBinding = new Binding()
        {
            Source = performances
        };
        PerformancesListView.SetBinding(ListView.ItemsSourceProperty, performancesBinding);

        var perfs = Queries.GetPerformanceMetadata(0, 30);
        foreach (var p in perfs)
            if (p != null)
                performances.Add(p);

        var playersBinding = new Binding()
        {
            Source = players
        };
        PlayersListView.SetBinding(ListView.ItemsSourceProperty, playersBinding);
        var pls = Queries.GetPlayerMetadata(0, 30);
        foreach (var p in pls)
            if (p != null)
                players.Add(p);
    }

    private void NextPageButton_Click(object sender, RoutedEventArgs eargs)
    {
        //encounters.Clear();
        //var encounterInfos = Queries.GetEncounterInfoMetadata(lastEncounterId, 30);
        //foreach (var e in encounterInfos)
        //    if (e != null)
        //    {
        //        encounters.Add(e);
        //        lastEncounterId = e.Id;
        //    }

        performances.Clear();
        var performanceInfos = Queries.GetPerformanceMetadata(lastPerformanceId, 30);
        foreach (var e in performanceInfos)
            if (e != null)
            {
                performances.Add(e);
                lastPerformanceId = e.Id;
            }

        PerformancesListView.UpdateLayout();
    }

    private void PlayerSearch_TextChanged(object sender, TextChangedEventArgs e)
    {
        if(string.IsNullOrEmpty(PlayerSearch.Text) || string.IsNullOrWhiteSpace(PlayerSearch.Text))
        {
            players.Clear();
            var pls = Queries.GetPlayerMetadata(0, 30);
            foreach (var p in pls)
                if(p != null)
                    players.Add(p);
            return;
        }
        var results = Queries.FindPlayersWithNameLike(PlayerSearch.Text);
        players.Clear();
        foreach (var p in results)
            players.Add(p);
    }

    private void EncounterInfoListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var testList = new CombatlogParser.src.Controls.TestList();
        (this.Parent as ContentControl).Content = testList;
        testList.GetData(EncounterInfoListView.SelectedItem as EncounterInfoMetadata);
    }
}
