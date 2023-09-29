using System.Windows.Media;

namespace CombatlogParser.Data.DisplayReady;

#pragma warning disable CS8618
public class PlayerDeathDataRow
{
    public string Name { get; set; }
    public Brush Color { get; set; }
    public string FormattedTimestamp { get; set; }
    public string AbilityName { get; set; }

    public PlayerDeathDataRow(string name, Brush color, string formattedTimestamp, string abilityName)
    {
        Name = name;
        Color = color;
        FormattedTimestamp = formattedTimestamp;
        AbilityName = abilityName;
    }
}
#pragma warning restore CS8618
