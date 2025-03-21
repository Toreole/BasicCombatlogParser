using System.Windows.Media;

namespace CombatlogParser.Data.DisplayReady;

public class NamedValueBarData
{
#pragma warning disable CS8618
	public string Name { get; set; }
	public string Label { get; set; }
	public double Value { get; set; }
	public double Maximum { get; set; }
	public string ValueString { get; set; }
	public Brush Color { get; set; }
#pragma warning restore CS8618
}
