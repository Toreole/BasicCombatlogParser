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
        if (currentEncounter == null) return;
        Dictionary<string, long> data;
		switch (currentViewMode)
		{
			case SingleEncounterViewMode.DamageDone:
                SetupDataGridForDamageDone();
				data = currentEncounter!.CalculateBreakdown(EncounterInfo.BreakdownMode.DamageDone, true, selectedEntityGUID);
				CalculateFinalMetricsAndDisplay(data);
				break;
			case SingleEncounterViewMode.Healing:
				SetupDataGridForHealing();
				data = currentEncounter!.CalculateBreakdown(EncounterInfo.BreakdownMode.HealingDone, true, selectedEntityGUID);
                CalculateFinalMetricsAndDisplay(data);
				break;
			case SingleEncounterViewMode.DamageTaken:
				SetupDataGridForDamageTaken();
				data = currentEncounter.CalculateBreakdown(EncounterInfo.BreakdownMode.DamageTaken, true, selectedEntityGUID);
                CalculateFinalMetricsAndDisplay(data);
				break;
			case SingleEncounterViewMode.Deaths:
				GenerateDeathsBreakdown();
				break;
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
        SetupSourceSelection();
        UpdateViewForCurrentMode();
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
        //TODO: Support this for enemies aswell.
        var deathData = currentEncounter.GetPlayerDeathInfo();
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
    /// Calculate X-per-second, the overall total across all entries, make it display ready, and fill the DataGrid.Items collection.
    /// </summary>
    /// <param name="amountBySource"></param>
    private void CalculateFinalMetricsAndDisplay(Dictionary<string, long> amountBySource)
    {
        bool keyIsPlayerGUID = selectedEntityGUID == null;
        var results = GetOrderedTotals(amountBySource, out long total);

        var encounterLength = currentEncounter!.LengthInSeconds;
        NamedValueBarData[] displayData = new NamedValueBarData[results.Length];
        long maximum = results[0].Value;
        for (int i = 0; i < displayData.Length; i++)
        {
            if (keyIsPlayerGUID)
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
            else
            {
                displayData[i] = new()
                {
                    Maximum = maximum,
                    Value = results[i].Value,
                    Label = results[i].Value.ToShortFormString(),
                    ValueString = (results[i].Value / encounterLength).ToString("N1"),
                    Name = results[i].Guid,
                    Color = Brushes.White
                };
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
        currentEncounter.ExportPlayerMovementAsImage();
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
