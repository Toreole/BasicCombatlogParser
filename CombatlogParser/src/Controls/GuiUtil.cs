namespace CombatlogParser;
public static class GuiUtil
{
	public static string ToPrettyString(this Enum value)
	{
		return value.ToString().Replace('_', ' ');
	}
}