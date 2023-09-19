using CombatlogParser.Data;
using CombatlogParser.Data.DisplayReady;
using CombatlogParser.Data.Events;
using CombatlogParser.Data.Metadata;
using CombatlogParser.Formatting;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CombatlogParser.Controls;

/// <summary>
/// Interaction logic for SingleEncounterView.xaml
/// </summary>
public partial class SingleEncounterView : ContentView
{
    private const string menuBandButtonDefault = "selection-button-spaced";
    private const string menuBandButtonHighlighted = "selection-button-spaced-highlighted";

    private Button highlightedButton;

    private EncounterInfoMetadata? currentEncounterMetadata = null;
    private EncounterInfo? currentEncounter = null;
    private SingleEncounterViewMode currentViewMode = SingleEncounterViewMode.DamageDone;

    public uint EncounterMetadataId
    {
        get => currentEncounterMetadata?.Id ?? 0;
        set
        {
            if (currentEncounterMetadata == null || currentEncounterMetadata.Id != value)
            {
                currentEncounterMetadata = Queries.FindEncounterById(value);
                GetData(currentEncounterMetadata);
            }
        }
    }

    public EncounterInfoMetadata? EncounterMetadata
    {
        get => currentEncounterMetadata;
        set
        {
            if (value == null)
                return;
            if (currentEncounterMetadata == null || currentEncounterMetadata.Id != value.Id)
            {
                currentEncounterMetadata = value;
                GetData(currentEncounterMetadata);
            }
        }
    }

    public SingleEncounterView()
    {
        InitializeComponent();
        highlightedButton = DamageButton;
        highlightedButton.Style = this.Resources[menuBandButtonHighlighted] as Style;
    }

    private void TabButtonClick(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
        if (sender is Button button)
        {
            highlightedButton.Style = this.Resources[menuBandButtonDefault] as Style;
            highlightedButton = button;
            highlightedButton.Style = this.Resources[menuBandButtonHighlighted] as Style;

            var updatedViewMode = (SingleEncounterViewMode)button.Tag;
            if (updatedViewMode == currentViewMode)
                return;

            currentViewMode = (SingleEncounterViewMode)button.Tag;

            if (currentEncounter is null)
                return;

            switch (currentViewMode)
            {
                case SingleEncounterViewMode.DamageDone:
                    GenerateDamageDoneBreakdown();
                    break;
                case SingleEncounterViewMode.Healing:
                    GenerateHealingBreakdown();
                    break;
                case SingleEncounterViewMode.DamageTaken:
                    GenerateDamageTakenBreakdown();
                    break;
            }
        }
    }

    private void GetData(EncounterInfoMetadata encounterInfoMetadata)
    {
        DataGrid.Items.Clear();

        //surprised it just works like this.
        LoadDataAsync(encounterInfoMetadata).WaitAsync(CancellationToken.None);
    }

    private async Task LoadDataAsync(EncounterInfoMetadata encounterInfoMetadata)
    {
        var progress = MainWindow!.ShowProgressBar();
        progress.DescriptionText = "Reading Encounter...";
        progress.ProgressPercent = 50; //somehow have this progress percent be set by ParseEncounterAsync
        try
        {
            currentEncounter = await CombatLogParser.ParseEncounterAsync(encounterInfoMetadata);
        }
        catch (FormatException ex)
        {
            var x = 1;
        }
        MainWindow.HideProgressBar(progress);
        //Maybe not do this immediately but do a check before for the DamageButton being selected.
        GenerateDamageDoneBreakdown();
    }

    private void GenerateDamageDoneBreakdown()
    {
        if (currentEncounter is null)
            return;

        SetupDataGridForDamageDone();

        Dictionary<string, long> damageBySource = new();
        var damageEvents = currentEncounter.CombatlogEventDictionary.GetEvents<DamageEvent>();
        var filter = EventFilters.AllySourceEnemyTargetFilter;
        foreach (var dmgEvent in damageEvents.Where(filter.Match))
        {
            string? actualSource;
            if (!currentEncounter.SourceToOwnerGuidLookup.TryGetValue(dmgEvent.SourceGUID, out actualSource))
                actualSource = dmgEvent.SourceGUID;

            var fullAmount = dmgEvent.damageParams.amount + dmgEvent.damageParams.absorbed;

            if (damageBySource.ContainsKey(actualSource))
                damageBySource[actualSource] += fullAmount;
            else
                damageBySource[actualSource] = fullAmount;
        }
        var damageSupportEvents = currentEncounter.CombatlogEventDictionary.GetEvents<DamageSupportEvent>();
        foreach (var dmgEvent in damageSupportEvents)
        {
            var effectiveDamage = dmgEvent.damageParams.amount + dmgEvent.damageParams.absorbed;
            if (damageBySource.ContainsKey(dmgEvent.SourceGUID))
            {
                damageBySource[dmgEvent.SourceGUID] -= effectiveDamage;
            }
            if (damageBySource.ContainsKey(dmgEvent.supporterGUID))
            {
                damageBySource[dmgEvent.supporterGUID] += effectiveDamage;
            }
            else
            {
                damageBySource[dmgEvent.supporterGUID] = effectiveDamage;
            }
        }

        CalculateFinalMetricsAndDisplay(damageBySource);
    }

    private void GenerateDamageTakenBreakdown()
    {
        SetupDataGridForDamageTaken();

        Dictionary<string, long> dmgTakenByTarget = new();
        var damageEvents = currentEncounter!.CombatlogEventDictionary.GetEvents<DamageEvent>();
        var filter = EventFilters.GroupMemberTargetFilter;
        foreach (var dmgEvent in damageEvents.Where(filter.Match))
        {
            var target = dmgEvent.TargetGUID;
            if (dmgTakenByTarget.ContainsKey(target))
                dmgTakenByTarget[target] += dmgEvent.damageParams.amount + dmgEvent.damageParams.absorbed;
            else
                dmgTakenByTarget[target] = dmgEvent.damageParams.amount + dmgEvent.damageParams.absorbed;
        }

        CalculateFinalMetricsAndDisplay(dmgTakenByTarget);
    }

    private string GetUnitNameOrFallback(string guid)
    {
        return currentEncounter?.CombatlogEvents.FirstOrDefault(x => x.TargetGUID == guid)?.TargetName
            ?? currentEncounter?.CombatlogEvents.FirstOrDefault(x => x.SourceGUID == guid)?.SourceName
            ?? "Unknown";
    }

    private static NamedTotal<TNumber>[] GetOrderedTotals<TNumber>(Dictionary<string, TNumber> lookup, out long total) 
        where TNumber : INumber<TNumber>, IBinaryInteger<TNumber>, IAdditionOperators<TNumber, long, long>
    {
        var results = new NamedTotal<TNumber>[lookup.Count];
        int i = 0;
        total = 0;
        foreach (var pair in lookup.OrderByDescending(x => x.Value))
        {
            results[i] = new(
                string.Empty, //was currentEncounter.CombatlogEvents.First(x => x.TargetGUID == pair.Key).TargetName,
                pair.Key,
                pair.Value
            );
            total = pair.Value + total;
            i++;
        }
        return results;
    }

    private void SetupDataGridForDamageTaken()
    {
        //for now, just use the same columns as damage done.
        SetupDataGridForDamageDone();
    }

    private void SetupDataGridForDamageDone()
    {
        DataGrid.Items.Clear();

        //Correct headers.
        MetricSumColumn.Header = "Amount";
        MetricPerSecondColumn.Header = "DPS";

        //Setup for Damage by default.
        var mainGridColumns = DataGrid.Columns;
        mainGridColumns.Clear();
        mainGridColumns.Add(NameColumn);
        mainGridColumns.Add(MetricSumColumn);
        mainGridColumns.Add(MetricPerSecondColumn);
    }

    //oh my god this is awful code.
    private void GenerateHealingBreakdown()
    {
        if (currentEncounter == null)
            return;

        SetupDataGridForHealing();

        Dictionary<string, long> healingBySource = new();
        IEventFilter filter = EventFilters.AllySourceFilter;
        var healEvents = currentEncounter.CombatlogEventDictionary.GetEvents<HealEvent>();
        foreach (var healEvent in healEvents.Where(filter.Match))
        {
            string? actualSource;
            if (!currentEncounter.SourceToOwnerGuidLookup.TryGetValue(healEvent.SourceGUID, out actualSource))
                actualSource = healEvent.SourceGUID;

            var fullAmount = healEvent.Amount + healEvent.Absorbed - healEvent.Overheal;

            if (healingBySource.ContainsKey(actualSource))
                healingBySource[actualSource] += fullAmount;
            else
                healingBySource[actualSource] = fullAmount;
        }
        var absorbEvents = currentEncounter.CombatlogEventDictionary.GetEvents<SpellAbsorbedEvent>();
        Func<SpellAbsorbedEvent, bool> absorbCasterFilter = new((x) =>
        {
            return x.AbsorbCasterFlags.HasFlagf(UnitFlag.COMBATLOG_OBJECT_REACTION_FRIENDLY)
                || x.AbsorbCasterFlags.HasFlagf(UnitFlag.COMBATLOG_OBJECT_REACTION_NEUTRAL);
        });
        foreach (var absorbEvent in absorbEvents.Where(absorbCasterFilter))
        {
            string? actualSource;
            if (!currentEncounter.SourceToOwnerGuidLookup.TryGetValue(absorbEvent.AbsorbCasterGUID, out actualSource))
                actualSource = absorbEvent.AbsorbCasterGUID;
            if (healingBySource.ContainsKey(actualSource))
                healingBySource[actualSource] += absorbEvent.AbsorbedAmount;
            else
                healingBySource[actualSource] = absorbEvent.AbsorbedAmount;
        }
        var healSupportEvents = currentEncounter.CombatlogEventDictionary.GetEvents<HealSupportEvent>();
        foreach (var supportEvent in healSupportEvents)
        {
            var healParams = supportEvent.healParams;
            var effectiveHealing = healParams.amount + healParams.absorbed - healParams.overheal;
            if (healingBySource.ContainsKey(supportEvent.supporterGUID))
            {
                healingBySource[supportEvent.supporterGUID] += effectiveHealing;
            }
            else
            {
                healingBySource[supportEvent.supporterGUID] = effectiveHealing;
            }
            if (healingBySource.ContainsKey(supportEvent.SourceGUID))
            {
                healingBySource[supportEvent.SourceGUID] -= effectiveHealing;
            }
        }

        CalculateFinalMetricsAndDisplay(healingBySource);
    }

    private void CalculateFinalMetricsAndDisplay(Dictionary<string, long> healingBySource)
    {
        var results = GetOrderedTotals(healingBySource, out long total);

        var encounterLength = currentEncounter!.LengthInSeconds;
        NamedValueBarData[] displayData = new NamedValueBarData[results.Length];
        long maximum = results[0].Value;
        for (int i = 0; i < displayData.Length; i++)
        {
            PlayerInfo? player = currentEncounter.FindPlayerInfoByGUID(results[i].Guid);
            displayData[i] = new()
            {
                Maximum = maximum,
                Value = results[i].Value,
                Label = results[i].Value.ToShortFormString(),
                ValueString = (results[i].Value / encounterLength).ToString("N1")
            };
            if (player != null)
            {
                displayData[i].Name = player.Name;
                displayData[i].Color = player.Class.GetClassBrush();
            }
            else
            {
                displayData[i].Name = GetUnitNameOrFallback(results[i].Guid);
                displayData[i].Color = Brushes.Red;
            }
        }
        foreach (var entry in displayData)
            DataGrid.Items.Add(entry);
        //add the special Total entry
        DataGrid.Items.Add(new NamedValueBarData()
        {
            Name = "Total",
            Color = Brushes.White,
            Maximum = maximum,
            Value = -1,
            Label = total.ToShortFormString(),
            ValueString = (total / encounterLength).ToString("N1")
        });
    }

    private void Test()
    {
        CombatlogEventDictionary dict = new CombatlogEventDictionary();

        GetSums(dict.GetEvents<DamageEvent>(), x => x.damageParams.amount - x.damageParams.overkill);
        GetSums(dict.GetEvents<HealEvent>(), x => x.Amount - x.Overheal);
    }

    private void GetSums<T>(IList<T> list, Func<T, long> valueAccessor)
    {
        long sum = 0;
        foreach (var item in list)
        {
            sum += valueAccessor(item);
        }
    }

    private void SetupDataGridForHealing()
    {
        DataGrid.Items.Clear();

        //Correct headers.
        MetricSumColumn.Header = "Amount";
        MetricPerSecondColumn.Header = "HPS";

        //Setup for Damage by default.
        var mainGridColumns = DataGrid.Columns;
        mainGridColumns.Clear();
        mainGridColumns.Add(NameColumn);
        mainGridColumns.Add(MetricSumColumn);
        mainGridColumns.Add(MetricPerSecondColumn);
    }

    public struct NamedTotal<T> where T : INumber<T>
    {
        public string Name;
        public string Guid;
        public T Value;

        public NamedTotal(string name, string guid, T value)
        {
            Name = name;
            Guid = guid;
            Value = value;
        }
    }
}
