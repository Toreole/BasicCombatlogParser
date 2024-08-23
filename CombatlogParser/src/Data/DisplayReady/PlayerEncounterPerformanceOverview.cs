namespace CombatlogParser.Data.DisplayReady;
public class PlayerEncounterPerformanceOverview
{
	public string EncounterName { get; init; } = "Unknown Boss";
	public string HighestMetricValue { get; init; } = "-";
	public string MedianMetricValue { get; init; } = "-";
	public string KillCount { get; init; } = "0";
	public string FastestTime { get; init; } = "-";
}