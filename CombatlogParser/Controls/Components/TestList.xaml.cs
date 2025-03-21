﻿using CombatlogParser.Controls.Styles;
using CombatlogParser.Data;
using CombatlogParser.Data.DisplayReady;
using CombatlogParser.Data.Metadata;
using CombatlogParser.Data.WowEnums;
using CombatlogParser.Database;
using CombatlogParser.Events;
using CombatlogParser.Events.Filters;
using CombatlogParser.Parsing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CombatlogParser.Controls.Components;

/// <summary>
/// Interaction logic for TestList.xaml
/// </summary>
public partial class TestList : UserControl
{
	private bool nameColumnActive = false;

	public TestList()
	{
		InitializeComponent();
		//GetData();
		//DataGrid.Columns.Remove(this.NameColumn);
		//AddRandomSampleData();
	}

	public void GetData(EncounterInfoMetadata encounterInfoMetadata)
	{
		DataGrid.Items.Clear();

		EncounterInfo encounterInfo = ParserCore.ParseEncounter(encounterInfoMetadata);

		Dictionary<string, long> damageBySource = [];
		var damageEvents = encounterInfo.CombatlogEventDictionary.GetEvents<DamageEvent>();
		var filter = new TargetFlagFilter(UnitFlag.COMBATLOG_OBJECT_REACTION_HOSTILE);
		foreach (var dmgEvent in damageEvents.Where(filter.Match))
		{
			if (!encounterInfo.SourceToOwnerGuidLookup.TryGetValue(dmgEvent.SourceGUID, out string? actualSource))
				actualSource = dmgEvent.SourceGUID;
			if (damageBySource.ContainsKey(actualSource))
				damageBySource[actualSource] += dmgEvent.DamageParams.amount;
			else
				damageBySource[actualSource] = dmgEvent.DamageParams.amount;
		}
		(string sourceGuid, string sourceName, long damage)[] results = new (string, string, long)[damageBySource.Count];
		int i = 0;
		long totalDamage = 0;
		foreach (var pair in damageBySource.OrderByDescending(x => x.Value))
		{
			results[i] = (
				pair.Key,
				encounterInfo.CombatlogEvents.First(x => x.SourceGUID == pair.Key).SourceName,
				pair.Value
			);
			totalDamage += pair.Value;
			i++;
		}

		var encounterLength = encounterInfo.LengthInSeconds;
		NamedValueBarData[] displayData = new NamedValueBarData[results.Length];
		long maxDamage = results[0].damage;
		for (i = 0; i < displayData.Length; i++)
		{
			PlayerInfo? player = encounterInfo.FindPlayerInfoByGUID(results[i].sourceGuid);
			if (player != null)
			{
				displayData[i] = new()
				{
					Name = player.Name,
					Color = player.Class.GetClassBrush(),
					Maximum = maxDamage,
					Value = results[i].damage,
					ValueString = (results[i].damage / encounterLength).ToString("N1")
				};
			}
			else
			{
				displayData[i] = new()
				{
					Name = results[i].sourceName,
					Color = Brushes.Red,
					Maximum = maxDamage,
					Value = results[i].damage,
					ValueString = (results[i].damage / encounterLength).ToString("N1")
				};
			}
		}
		foreach (var entry in displayData)
			DataGrid.Items.Add(entry);
	}

	public void GetData()
	{
		using CombatlogDBContext dbContext = new();
		EncounterInfoMetadata encounterInfoMetadata = dbContext.Encounters
			.Where(x => x.Success).First();
		GetData(encounterInfoMetadata);
	}

	private void AddRandomSampleData()
	{
		Random random = new();
		//warning: highly illegal code
		byte[] objects = new byte[10];
		var classes = Enum.GetValues<ClassId>();
		var playerPerformances = objects.Select(x =>
		{
			double num = random.NextDouble() * 10000.0d;
			return new NamedValueBarData()
			{
				Name = "Player",
				Value = num,
				Maximum = 10000.0,
				ValueString = num.ToString("0.0"),
				Color = classes[random.Next(1, classes.Length)].GetClassBrush(),
			};
		}).OrderByDescending(x => x.Value).ToArray();
		var max = playerPerformances.Max(x => x.Value);

		foreach (var x in playerPerformances)
		{
			x.Maximum = max;
			DataGrid.Items.Add(x);
		}

	}

	void ButtonClicked(object sender, RoutedEventArgs e)
	{
		e.Handled = true;
		if (sender is Button)
		{
			if (this.nameColumnActive)
				this.DataGrid.Columns.Remove(NameColumn);
			else
				this.DataGrid.Columns.Insert(0, NameColumn);
			this.nameColumnActive ^= true;
		}
	}

}