using CombatlogParser.Data.WowEnums;
using static CombatlogParser.Parsing.ParsingUtil;

namespace CombatlogParser.Events.EventData;

public class DamageEventParams
{
	public long amount;
	public long baseAmount;
	public long overkill;
	public SpellSchool damageSchool;
	public long resisted;
	public long blocked;
	public long absorbed;
	public bool critical;
	public bool glancing;
	public bool isOffHand;

	public long TotalAmount => amount + absorbed;

	public DamageEventParams(string line, ref int dataIndex)
	{
		amount = long.Parse(NextSubstring(line, ref dataIndex));
		baseAmount = long.Parse(NextSubstring(line, ref dataIndex));
		overkill = long.Parse(NextSubstring(line, ref dataIndex));
		var schoolRaw = NextSubstring(line, ref dataIndex);
		damageSchool = (SpellSchool)(schoolRaw.StartsWith("0x") ? Convert.ToInt32(schoolRaw, 16) : int.Parse(schoolRaw));
		resisted = long.Parse(NextSubstring(line, ref dataIndex));
		blocked = long.Parse(NextSubstring(line, ref dataIndex));
		absorbed = long.Parse(NextSubstring(line, ref dataIndex));
		critical = NextSubstring(line, ref dataIndex) == "1";
		glancing = NextSubstring(line, ref dataIndex) == "1";
		isOffHand = NextSubstring(line, ref dataIndex) == "1";
	}
}