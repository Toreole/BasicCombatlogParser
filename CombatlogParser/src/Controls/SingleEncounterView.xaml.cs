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

    /// <summary>
    /// The metadata for the encounter to be viewed.
    /// Set to a different value to update the view.
    /// </summary>
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

    /// <summary>
    /// Triggered when a menu band button was clicked to switch the active view tab
    /// between the different modes.
    /// </summary>
    /// <param name="sender">Should be a Button</param>
    /// <param name="e"></param>
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
                case SingleEncounterViewMode.Deaths:
                    GenerateDeathsBreakdown();
                    break;
            }
        }
    }

    /// <summary>
    /// Clears the DataGrid and starts LoadDataAsync
    /// </summary>
    /// <param name="encounterInfoMetadata"></param>
    private void GetData(EncounterInfoMetadata encounterInfoMetadata)
    {
        DataGrid.Items.Clear();

        //surprised it just works like this.
        LoadDataAsync(encounterInfoMetadata).WaitAsync(CancellationToken.None);
    }

    /// <summary>
    /// Gets Encounter data via CombatLogParser.ParseEncounter(Async) and shows 
    /// the progress bar while waiting for it to be done
    /// </summary>
    /// <param name="encounterInfoMetadata"></param>
    /// <returns></returns>
    private async Task LoadDataAsync(EncounterInfoMetadata encounterInfoMetadata)
    {
        var progress = MainWindow!.ShowProgressBar();
        progress.DescriptionText = "Reading Encounter...";
        progress.ProgressPercent = 50; //somehow have this progress percent be set by ParseEncounterAsync
        try
        {
            currentEncounter = await CombatLogParser.ParseEncounterAsync(encounterInfoMetadata);
        }
        catch (FormatException)
        {

        }
        MainWindow.HideProgressBar(progress);
        //Maybe not do this immediately but do a check before for the DamageButton being selected.
        GenerateDamageDoneBreakdown();
    }

    /// <summary>
    /// Calculates and shows the sums of each players damage done to enemies
    /// </summary>
    private void GenerateDamageDoneBreakdown()
    {
        if (currentEncounter is null)
            return;

        SetupDataGridForDamageDone();

        Dictionary<string, long> damageBySource = new();
        var damageEvents = currentEncounter.CombatlogEventDictionary.GetEvents<DamageEvent>();
        var filter = EventFilters.AllySourceEnemyTargetFilter;
        SumAmountsForSources(
            damageEvents.Where(filter.Match),
            dmgEvent => dmgEvent.damageParams.amount + dmgEvent.damageParams.absorbed,
            damageBySource);
        //damage done via support events (augmentation evoker) needs to be attributed to the evoker
        //and subtracted from the buffed players damage.
        var damageSupportEvents = currentEncounter.CombatlogEventDictionary.GetEvents<DamageSupportEvent>();
        foreach (var dmgEvent in damageSupportEvents)
        {
            var damageSupported = dmgEvent.damageParams.amount + dmgEvent.damageParams.absorbed;
            if (damageBySource.ContainsKey(dmgEvent.SourceGUID))
            {
                damageBySource[dmgEvent.SourceGUID] -= damageSupported;
            }
            AddSum(damageBySource, dmgEvent.supporterGUID, damageSupported);
        }

        CalculateFinalMetricsAndDisplay(damageBySource);
    }

    /// <summary>
    /// Calculate and show the totals of each players damage taken.
    /// </summary>
    private void GenerateDamageTakenBreakdown()
    {
        SetupDataGridForDamageTaken();

        Dictionary<string, long> dmgTakenByTarget = new();
        var damageEvents = currentEncounter!.CombatlogEventDictionary.GetEvents<DamageEvent>();
        var filter = EventFilters.GroupMemberTargetFilter;
        foreach (var dmgEvent in damageEvents.Where(filter.Match))
        {
            var target = dmgEvent.TargetGUID;
            var amount = dmgEvent.damageParams.amount + dmgEvent.damageParams.absorbed;
            AddSum(dmgTakenByTarget, target, amount);
        }

        CalculateFinalMetricsAndDisplay(dmgTakenByTarget);
    }

    /// <summary>
    /// Attempt to get the name of a unit through combatlog events.
    /// </summary>
    /// <param name="guid"></param>
    /// <returns></returns>
    private string GetUnitNameOrFallback(string guid)
    {
        return currentEncounter?.CombatlogEvents.FirstOrDefault(x => x.TargetGUID == guid)?.TargetName
            ?? currentEncounter?.CombatlogEvents.FirstOrDefault(x => x.SourceGUID == guid)?.SourceName
            ?? "Unknown";
    }

    /// <summary>
    /// Sorts the totals in the dictionary in a descending order and outputs them alongside the units GUID in an array.
    /// </summary>
    /// <typeparam name="TNumber"></typeparam>
    /// <param name="lookup"></param>
    /// <param name="total">The total of all values</param>
    /// <returns></returns>
    private static NamedTotal<TNumber>[] GetOrderedTotals<TNumber>(Dictionary<string, TNumber> lookup, out long total) 
        where TNumber : INumber<TNumber>, IBinaryInteger<TNumber>, IAdditionOperators<TNumber, long, long>
    {
        var results = new NamedTotal<TNumber>[lookup.Count];
        int i = 0;
        total = 0;
        foreach (var pair in lookup.OrderByDescending(x => x.Value))
        {
            results[i] = new(
                pair.Key,
                pair.Value
            );
            total = pair.Value + total;
            i++;
        }
        return results;
    }

    private void SetupDataGridForDeaths()
    {
        DataGrid.Items.Clear();
        //Update headers where needed.
        AbilityNameColumn.Header = "Killing Blow";

        //add columns in order
        var columns = DataGrid.Columns;
        columns.Clear();
        columns.Add(NameColumn);
        columns.Add(AbilityNameColumn);
        columns.Add(TimestampColumn);
    }

    private void GenerateDeathsBreakdown()
    {
        if (currentEncounter == null) return;
        SetupDataGridForDeaths();
        List<PlayerDeathDataRow> deathData = new();
        var unitDiedEvents = currentEncounter.CombatlogEventDictionary.GetEvents<UnitDiedEvent>();
        var damageEvents = currentEncounter.CombatlogEventDictionary.GetEvents<DamageEvent>();
        var startTime = currentEncounter.EncounterStartTime;
        foreach(var deathEvent in unitDiedEvents.Where(x => x.TargetFlags.HasFlagf(UnitFlag.COMBATLOG_OBJECT_TYPE_PLAYER)))
        {
            PlayerInfo? player = currentEncounter.FindPlayerInfoByGUID(deathEvent.TargetGUID);
            var offsetTime = (deathEvent.Timestamp - startTime);
            var formattedTimestamp = offsetTime.ToString(@"m\:ss\.fff");
            if (player is not null)
            {
                var last3hits = damageEvents.Reverse().Where(x => x.TargetGUID == deathEvent.TargetGUID)
                        .Where(EventFilters.Before(deathEvent.Timestamp).Match)
                        .Take(3).ToArray();
                deathData.Add(
                    new(player.Name,
                    player.Class.GetClassBrush(),
                    formattedTimestamp,
                    abilityName: string.Join(", ", last3hits.Select(x => x?.spellData.name)) // last3hits.LastOrDefault()?.spellData.name ?? "unknown"
                    )
                ); 
            }
        }

        //now add the data to the grid
        foreach (var entry in deathData)
            DataGrid.Items.Add(entry);
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

    /// <summary>
    /// Looks up the given GUID in the encounters SourceToOwnerGuidLookup Dictionary.
    /// </summary>
    /// <param name="possibleSource"></param>
    /// <returns>The Value in the Lookup if the key exists</returns>
    private string GetActualSource(string possibleSource)
    {
        //watch out that this is only used in methods that already ensure that
        //currentEncounter is not null.
        if (currentEncounter!.SourceToOwnerGuidLookup.ContainsKey(possibleSource))
        {
            return currentEncounter.SourceToOwnerGuidLookup[possibleSource];
        }
        return possibleSource;
    }

    /// <summary>
    /// Calculates and shows the total healing done by each player
    /// </summary>
    private void GenerateHealingBreakdown()
    {
        if (currentEncounter == null)
            return;

        SetupDataGridForHealing();

        Dictionary<string, long> healingBySource = new();
        IEventFilter filter = EventFilters.AllySourceFilter; //only the source matters. you cant heal enemies after all
        var healEvents = currentEncounter.CombatlogEventDictionary.GetEvents<HealEvent>();
        SumAmountsForSources(
            healEvents.Where(filter.Match),
            healEvent => healEvent.Amount + healEvent.Absorbed - healEvent.Overheal,
            healingBySource);
        var absorbEvents = currentEncounter.CombatlogEventDictionary.GetEvents<SpellAbsorbedEvent>();
        //this func should not be in here, but its a specific usecase so it is.
        Func<SpellAbsorbedEvent, bool> absorbCasterFilter = new((x) => 
        {
            return x.AbsorbCasterFlags.HasFlagf(UnitFlag.COMBATLOG_OBJECT_REACTION_FRIENDLY)
                || x.AbsorbCasterFlags.HasFlagf(UnitFlag.COMBATLOG_OBJECT_REACTION_NEUTRAL);
        });
        foreach (var absorbEvent in absorbEvents.Where(absorbCasterFilter))
        {
            string actualSource = GetActualSource(absorbEvent.AbsorbCasterGUID);
            AddSum(healingBySource, actualSource, absorbEvent.AbsorbedAmount);
        }
        //healing done via support needs to be attributed to the evoker, not the buffed player.
        var healSupportEvents = currentEncounter.CombatlogEventDictionary.GetEvents<HealSupportEvent>();
        foreach (var supportEvent in healSupportEvents)
        {
            var healParams = supportEvent.healParams;
            var effectiveHealing = healParams.amount + healParams.absorbed - healParams.overheal;
            AddSum(healingBySource, supportEvent.supporterGUID, effectiveHealing);
            if (healingBySource.ContainsKey(supportEvent.SourceGUID))
            {
                healingBySource[supportEvent.SourceGUID] -= effectiveHealing;
            }
        }

        CalculateFinalMetricsAndDisplay(healingBySource);
    }

    /// <summary>
    /// Sums up the amounts in the given events by their Source
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="events"></param>
    /// <param name="amountGetter">Determines the amount. Different events have different ways of calculating it.</param>
    /// <param name="sumDictionary">The dictionary it outputs into.</param>
    private void SumAmountsForSources<T>(
        IEnumerable<T> events, 
        Func<T, long> amountGetter, 
        Dictionary<string, long> sumDictionary)
        where T : CombatlogEvent
    {
        foreach(var ev in events)
        {
            string sourceGuid = GetActualSource(ev.SourceGUID);
            long amount = amountGetter(ev);
            AddSum(sumDictionary, sourceGuid, amount);
        }
    }

    /// <summary>
    /// Calculate X-per-second, the overall total across all entries, make it display ready, and fill the DataGrid.Items collection.
    /// </summary>
    /// <param name="amountBySource"></param>
    private void CalculateFinalMetricsAndDisplay(Dictionary<string, long> amountBySource)
    {
        var results = GetOrderedTotals(amountBySource, out long total);

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

    /// <summary>
    /// Add an amount to a guid's sum in the dictionary. Small helper to avoid duplicate code.
    /// </summary>
    /// <param name="sums"></param>
    /// <param name="guid"></param>
    /// <param name="amount"></param>
    private static void AddSum(Dictionary<string, long> sums, string guid, long amount)
    {
        if (sums.ContainsKey(guid))
        {
            sums[guid] += amount;
        }
        else
        {
            sums[guid] = amount;
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

    /// <summary>
    /// Really nothing more than a glorified KeyValuePair
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct NamedTotal<T> where T : INumber<T>
    {
        public string Guid;
        public T Value;

        public NamedTotal(string guid, T value)
        {
            Guid = guid;
            Value = value;
        }
    }
}
