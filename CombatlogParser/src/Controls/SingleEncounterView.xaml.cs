using CombatlogParser.Data;
using CombatlogParser.Data.DisplayReady;
using CombatlogParser.Data.Events;
using CombatlogParser.Data.Events.EventData;
using CombatlogParser.Data.Metadata;
using CombatlogParser.Formatting;
using Microsoft.Win32;
using System.ComponentModel;
using System.Drawing;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WinRT;

using Brushes = System.Windows.Media.Brushes;
using Point = System.Drawing.Point;

namespace CombatlogParser.Controls;

/// <summary>
/// Interaction logic for SingleEncounterView.xaml
/// </summary>
public partial class SingleEncounterView : ContentView
{
    private const string menuBandButtonDefault = "selection-button-spaced";
    private const string menuBandButtonHighlighted = "selection-button-spaced-highlighted";

    private Button highlightedButton;

    private string? selectedEntityGUID = null;
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
                //SetupSourceSelection();
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
            UpdateViewForCurrentMode();
		}
    }

    private void UpdateViewForCurrentMode()
    {
        if (selectedEntityGUID == null)
		{
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
        else
        {
            switch (currentViewMode)
            {
                case SingleEncounterViewMode.DamageDone:
                    GenerateDamageDoneBySource();
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
        SetupSourceSelection();

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

	private void GenerateDamageDoneBySource()
    {
        if (currentEncounter is null || selectedEntityGUID is null)
            return;

        SetupDataGridForDamageDone();

        Dictionary<String, long> damageBySpell = new();
        var damageEvents = currentEncounter.CombatlogEventDictionary.GetEvents<DamageEvent>();
        foreach (var dmgEvent in damageEvents.Where(x => GetActualSource(x.SourceGUID) == selectedEntityGUID))
        {
            if (damageBySpell.ContainsKey(dmgEvent.spellData.name))
            {
                damageBySpell[dmgEvent.spellData.name] += dmgEvent.damageParams.TotalAmount;
            }
            else
            {
                damageBySpell.Add(dmgEvent.spellData.name, dmgEvent.damageParams.TotalAmount);
            }
        }
        //once again, subtract any support dmg
        var supportEvents = currentEncounter.CombatlogEventDictionary.GetEvents<DamageSupportEvent>();
        foreach (var supportEvent in supportEvents.Where(x => x.SourceGUID == selectedEntityGUID))
        {
            damageBySpell[supportEvent.spellData.name] -= supportEvent.damageParams.TotalAmount;
        }

        //prepare data for displaying.
        var results = GetOrderedTotals(damageBySpell, out long total);
		var encounterLength = currentEncounter!.LengthInSeconds;
		NamedValueBarData[] displayData = new NamedValueBarData[results.Length];
		long maximum = results[0].Value;
		for (int i = 0; i < displayData.Length; i++)
		{
			displayData[i] = new()
			{
				Maximum = maximum,
				Value = results[i].Value,
				Label = results[i].Value.ToShortFormString(),
				ValueString = (results[i].Value / encounterLength).ToString("N1"),
				Name = results[i].Guid, //unintuitively, guid has the value of the spell name right here.
				Color = Brushes.White
			};

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
            var amount = dmgEvent.damageParams.TotalAmount;
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
        columns.Add(LastHitsColumn);
        columns.Add(AbilityNameColumn);
        columns.Add(SlowDeathTimeColumn);
        columns.Add(TimestampColumn);
    }

    private void GenerateDeathsBreakdown()
    {
        if (currentEncounter == null)
            return;
        SetupDataGridForDeaths();
        List<PlayerDeathDataRow> deathData = new();
        var startTime = currentEncounter.EncounterStartTime;
        //yes, all of this data is required.
        var unitDiedEvents = currentEncounter.CombatlogEventDictionary.GetEvents<UnitDiedEvent>();
        var damageEvents = currentEncounter.CombatlogEventDictionary.GetEvents<DamageEvent>();
        var advancedInfoEvents = currentEncounter.CombatlogEventDictionary.GetEvents<AdvancedParamEvent>();
        //hard-coded to only show death info for players right now. parameterize UnitFlag at a later time.
        foreach (var deathEvent in unitDiedEvents.Where(x => x.TargetFlags.HasFlagf(UnitFlag.COMBATLOG_OBJECT_TYPE_PLAYER)))
        {
            //playerInfo is pretty much only used for coloring via the class brush.
            PlayerInfo? player = currentEncounter.FindPlayerInfoByGUID(deathEvent.TargetGUID);
            var offsetTime = (deathEvent.Timestamp - startTime);
            var formattedTimestamp = offsetTime.ToString(@"m\:ss\.fff");
            if (player is not null)
            {
                var beforeFilter = EventFilters.Before(deathEvent.Timestamp);
                var last3hits = damageEvents.Reverse().Where(x => x.TargetGUID == deathEvent.TargetGUID)
                        .Where(beforeFilter.Match)
                        .Take(3).ToArray();
                // the time since the last time someone was at above 90% hp should give a rough estimate
                // of how quickly they died.
                // WARNING: there is an important edge case here:
                // someone getting resurrected and dying again before they
                // ever reach a high health % again. It would take the last time they were at high health before the first death.
                var lastTimeFullHealth = advancedInfoEvents.Reverse().Where(beforeFilter.Match)
                    .Where(x =>
                    {
                        var aparams = x.AdvancedParams;
                        return aparams.infoGUID == player.GUID && aparams.currentHP >= 0.90 * aparams.maxHP;
                    }
                    ).FirstOrDefault()?.Timestamp ?? deathEvent.Timestamp;
                var slowDeathTimeOffset = deathEvent.Timestamp - lastTimeFullHealth;
                var formattedDeathTime = slowDeathTimeOffset.TotalMilliseconds <= 100 ?
                    "Instant" : $"{slowDeathTimeOffset:ss\\.fff} seconds";


                deathData.Add(
                    new(player.Name,
                    player.Class.GetClassBrush(),
                    formattedTimestamp,
                    abilityName: last3hits[^1].spellData.name,
                    lastHits: last3hits.Select(x => x.spellData.name).ToArray(), // last3hits.LastOrDefault()?.spellData.name ?? "unknown"
                    formattedDeathTime
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
        EventFilter filter = EventFilters.AllySourceFilter; //only the source matters. you cant heal enemies after all
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
        foreach (var ev in events)
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

	private void ExportMovementButton_Click(object sender, RoutedEventArgs e)
	{
        if (currentEncounter == null)
            return;
		const int resolution = 1024;
        Bitmap bitmap = new(resolution, resolution);
        using var graphics = Graphics.FromImage(bitmap);
        //set background
        graphics.FillRectangle(System.Drawing.Brushes.Black, new(0, 0, resolution, resolution));

		var events = currentEncounter.CombatlogEventDictionary.GetEvents<AdvancedParamEvent>().Select(x => x.AdvancedParams);

        const float padding = 5f;
		float minX, maxX, minY, maxY;
        minX = events.Min(x => x.positionX);
        maxX = events.Max(x => x.positionX);
		minY = events.Min(x => x.positionY);
		maxY = events.Max(x => x.positionY);
        { //make it a square.
            var width = maxX - minX;
            var height = maxY - minY;
            var halfExtents = Math.Max(width, height) * 0.5f + padding;
            var centerX = Average(minX, maxX);
            var centerY = Average(minY, maxY);
            minX = centerX - halfExtents;
            maxX = centerX + halfExtents;
            minY = centerY - halfExtents;
            maxY = centerY + halfExtents;
        }

		var lastPixelPosition = new Dictionary<string, Point>();

        foreach (var entry in events)
        {
            var unitGUID = entry.infoGUID;
            var player = currentEncounter.FindPlayerInfoByGUID(unitGUID);
            if (player == null)
                continue;

            int x = (int) (InverseLerp(entry.positionX, minX, maxX) * resolution);
			int y = (int)(InverseLerp(entry.positionY, minY, maxY) * resolution);
            Point position = new(x, y);

            if (lastPixelPosition.TryGetValue(unitGUID, out Point value))
			{
				var color = player.Class.GetClassColor();
                graphics.DrawLine(new System.Drawing.Pen(System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B), 2), value, position);
			}
            lastPixelPosition[unitGUID] = position;
		}

		var fileDialog = new SaveFileDialog
		{
			AddExtension = true,
			DefaultExt = "png",
			Filter = "PNG image|*.png",
			FileName = $"{currentEncounter.EncounterName}_{currentEncounter.DifficultyID}.png"
		};
		if (fileDialog.ShowDialog() == true)
            bitmap.Save(fileDialog.FileName);
	}

    private static float InverseLerp(float value, float a, float b)
    {
        return (value - a) / (b - a);
    }
    private static float Average(params float[] values)
    {
        return values.Sum() / values.Length;
    }

    private void SetupSourceSelection()
    {
        ignoreSourceSelectionChanged = true;
		var items = this.SourceSelectionComboBox.Items;
        items.Clear();
        items.Add("All Sources"); // index 0
		SourceSelectionComboBox.SelectedIndex = 0;

		// if mode == friendlies
		if (currentEncounter == null)
            return;
        foreach (var player in currentEncounter.Players)
        {
            items.Add(player.Name);
		}
		ignoreSourceSelectionChanged = false;
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

    bool ignoreSourceSelectionChanged = false;
	private void SourceSelectionChanged(object sender, SelectionChangedEventArgs e)
	{
        if (ignoreSourceSelectionChanged) return;

        if (SourceSelectionComboBox.SelectedIndex > 0)
        {
            //should do null check but should be guaranteed.
            selectedEntityGUID = currentEncounter!.Players[SourceSelectionComboBox.SelectedIndex-1].GUID;
		} 
        else
        {
            selectedEntityGUID = null;
        }
		UpdateViewForCurrentMode();
	}
}
