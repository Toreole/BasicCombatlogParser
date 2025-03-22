using System.Windows.Media;

namespace CombatlogParser.Data.DisplayReady;

public class PlayerDeathDataRow(
	string name, 
	Brush color, 
	string formattedTimestamp, 
	string abilityName, 
	string[] lastHits, 
	string deathTime
	)
{
	public string Name { get; set; } = name;
	public Brush Color { get; set; } = color;
	public string FormattedTimestamp { get; set; } = formattedTimestamp;
	public string AbilityName { get; set; } = abilityName;
	public string LastHits { get; set; } = string.Join(", ", lastHits);
	public string DeathTime { get; set; } = deathTime;
}
