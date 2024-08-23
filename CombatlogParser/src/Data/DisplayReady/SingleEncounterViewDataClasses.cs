using System.Windows.Media;

namespace CombatlogParser.Data.DisplayReady;

public class PlayerDeathDataRow
{
	public string Name { get; set; }
	public Brush Color { get; set; }
	public string FormattedTimestamp { get; set; }
	public string AbilityName { get; set; }
	public string LastHits { get; set; }
	public string DeathTime { get; set; }

	public PlayerDeathDataRow(string name, Brush color, string formattedTimestamp, string abilityName, string[] lastHits, string deathTime)
	{
		Name = name;
		Color = color;
		FormattedTimestamp = formattedTimestamp;
		AbilityName = abilityName;
		LastHits = string.Join(", ", lastHits);
		DeathTime = deathTime;
	}
}
