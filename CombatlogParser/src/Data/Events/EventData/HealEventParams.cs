using static CombatlogParser.ParsingUtil;

namespace CombatlogParser.Data.Events.EventData;

public class HealEventParams
{
	public readonly int amount;
	public readonly int baseAmount;
	public readonly int overheal;
	public readonly int absorbed;
	public readonly bool critical;

	public int TotalAmount => amount + absorbed;

	public HealEventParams(string entry, ref int dataIndex)
	{
		amount = int.Parse(NextSubstring(entry, ref dataIndex));
		baseAmount = int.Parse(NextSubstring(entry, ref dataIndex));
		overheal = int.Parse(NextSubstring(entry, ref dataIndex));
		absorbed = int.Parse(NextSubstring(entry, ref dataIndex));
		critical = NextSubstring(entry, ref dataIndex) == "1";
	}
}