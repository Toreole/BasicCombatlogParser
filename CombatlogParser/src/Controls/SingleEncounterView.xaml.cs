using CombatlogParser.Data;
using CombatlogParser.Data.DisplayReady;
using CombatlogParser.Data.Events;
using CombatlogParser.Data.Events.EventData;
using CombatlogParser.Data.Metadata;
using CombatlogParser.Formatting;
using CombatlogParser.Parsing;
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

    private EncounterInfoMetadata? currentEncounterMetadata = null;
    private EncounterInfo? currentEncounter = null;
    private SingleEncounterViewMode currentViewMode = SingleEncounterViewMode.DamageDone;

	private bool ignoreSourceSelectionChanged = false;
	private string? selectedEntityGUID = null;

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

        if (selectedEntityGUID == null || currentViewMode == SingleEncounterViewMode.Deaths)
        {
		    switch (currentViewMode)
		    {
			    case SingleEncounterViewMode.DamageDone:
                    SetupDataGridForDamageDone();
				    _GroupOverview(EncounterInfo.BreakdownMode.DamageDone);
				    break;
			    case SingleEncounterViewMode.Healing:
				    SetupDataGridForHealing();
					_GroupOverview(EncounterInfo.BreakdownMode.HealingDone);
				    break;
			    case SingleEncounterViewMode.DamageTaken:
				    SetupDataGridForDamageTaken();
					_GroupOverview(EncounterInfo.BreakdownMode.DamageTaken);
				    break;
			    case SingleEncounterViewMode.Deaths:
				    GenerateDeathsBreakdown();
				    break;
                case SingleEncounterViewMode.Casts:
                    SetupDataGridForCasts();
                    _GroupOverview(EncounterInfo.BreakdownMode.Casts);

					break;
			}
			void _GroupOverview(EncounterInfo.BreakdownMode mode)
			{
				var data = currentEncounter!.CalculateBreakdown(mode, true);
				CalculateFinalMetricsAndDisplay(data);
			}
		} 
        else //single player.
        {
            switch (currentViewMode)
            {
                case SingleEncounterViewMode.DamageDone:
                    SetupDataGridForDamageDone();
                    _PlayerOverview(EncounterInfo.BreakdownMode.DamageDone);
                    break;
                case SingleEncounterViewMode.Healing:
                    SetupDataGridForHealing();
                    _PlayerOverview(EncounterInfo.BreakdownMode.HealingDone);
                    break;
				case SingleEncounterViewMode.DamageTaken:
					SetupDataGridForDamageTaken();
					_PlayerOverview(EncounterInfo.BreakdownMode.DamageTaken);
					break;
				case SingleEncounterViewMode.Casts:
					SetupDataGridForCasts();
					_PlayerOverview(EncounterInfo.BreakdownMode.Casts);
					break;
                //case Deaths TODO.
			}
            void _PlayerOverview(EncounterInfo.BreakdownMode mode)
            {
                var data = currentEncounter!.CalculateBreakdownForEntity(mode, selectedEntityGUID);
                CalculateFinalMetricsAndDisplay(data);
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
		//reset source selection
		ResetSourceSelection();
		//surprised it just works like this.
		LoadDataAsync(encounterInfoMetadata).WaitAsync(CancellationToken.None);
	}

	private void ResetSourceSelection()
	{
		ignoreSourceSelectionChanged = true;
		SourceSelectionComboBox.SelectedIndex = 0;
		ignoreSourceSelectionChanged = false;
		selectedEntityGUID = null;
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
            //this should be in the try, not after.
			SetupSourceSelection();
			UpdateViewForCurrentMode();
            currentEncounter.PlotGraph(MetricGraph);
		}
        catch (FormatException)
        {

        }
        MainWindow.HideProgressBar(progress);
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

	private void SetupDataGridForCasts()
	{
		DataGrid.Items.Clear();

		MetricSumColumn.Header = "Amount";
		MetricPerSecondColumn.Header = "CPS";

		var mainGridColumns = DataGrid.Columns;
		mainGridColumns.Clear();
		mainGridColumns.Add(NameColumn);
		mainGridColumns.Add(MetricSumColumn);
		mainGridColumns.Add(MetricPerSecondColumn);
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

	/// <summary>
	/// Calculate X-per-second, the overall total across all entries, make it display ready, and fill the DataGrid.Items collection.
	/// </summary>
	/// <param name="amountBySource"></param>
	private void CalculateFinalMetricsAndDisplay(Dictionary<string, long> amountBySource)
    {
        var results = amountBySource.SortDescendingAndSumTotal(out long total);

        var encounterLength = currentEncounter!.LengthInSeconds;
        NamedValueBarData[] displayData = new NamedValueBarData[results.Length];
        long maximum = results[0].Value;
        for (int i = 0; i < displayData.Length; i++)
        {
            NamedValueBarData current = new()
            {
                Maximum = maximum,
                Value = results[i].Value,
                Label = results[i].Value.ToShortFormString(),
                ValueString = (results[i].Value / encounterLength).ToString("N1")
            };
            PlayerInfo? player = currentEncounter.FindPlayerInfoByGUID(results[i].Key);
            if (player != null)
            {
				current.Name = player.Name;
				current.Color = player.Class.GetClassBrush();
            }
            else
            {
				current.Name = currentEncounter.GetUnitNameOrFallback(results[i].Key);
				current.Color = Brushes.Red;
            }
			displayData[i] = current;
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
	private void CalculateFinalMetricsAndDisplay(Dictionary<SpellData, long> amountBySpell)
	{
		var results = amountBySpell.SortDescendingAndSumTotal(out long total);

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
                Name = results[i].Key.name,
                Color = results[i].Key.school.GetSchoolBrush()
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
