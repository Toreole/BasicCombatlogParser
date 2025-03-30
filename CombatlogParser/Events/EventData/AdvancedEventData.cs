using CombatlogParser.Data.WowEnums;
using CombatlogParser.Parsing;
using static CombatlogParser.Parsing.ParsingUtil;

namespace CombatlogParser.Events.EventData;

public class AdvancedEventData
{
	public readonly string infoGUID;
	public readonly string ownerGUID;
	public readonly long currentHP;
	public readonly long maxHP;
	public readonly long attackPower;
	public readonly long spellPower;
	public readonly long armor;
	public readonly long absorb;
	public readonly long unknown_v22_a;
	public readonly long unknown_v22_b;
	public readonly PowerType[] powerType;
	public readonly int[] currentPower;
	public readonly int[] maxPower;
	public readonly int[] powerCost;
	public readonly float positionX;
	public readonly float positionY;
	public readonly long uiMapID;
	public readonly float facing;
	public readonly int level;

	public AdvancedEventData(string data, ref int dataIndex)
	{
		infoGUID = string.Intern(NextSubstring(data, ref dataIndex));
		ownerGUID = string.Intern(NextSubstring(data, ref dataIndex));
		currentHP = long.Parse(NextSubstring(data, ref dataIndex));
		maxHP = long.Parse(NextSubstring(data, ref dataIndex));
		attackPower = long.Parse(NextSubstring(data, ref dataIndex));
		spellPower = long.Parse(NextSubstring(data, ref dataIndex));
		armor = long.Parse(NextSubstring(data, ref dataIndex));
		absorb = long.Parse(NextSubstring(data, ref dataIndex));
		if (ParserCore.LogVersion == 22)
		{
			unknown_v22_a = long.Parse(NextSubstring(data, ref dataIndex));
			unknown_v22_b = long.Parse(NextSubstring(data, ref dataIndex));
		}
		powerType = AllPowerTypesIn(NextSubstring(data, ref dataIndex));
		currentPower = AllIntsIn(NextSubstring(data, ref dataIndex));
		maxPower = AllIntsIn(NextSubstring(data, ref dataIndex));
		powerCost = AllIntsIn(NextSubstring(data, ref dataIndex));
		positionX = float.Parse(NextSubstring(data, ref dataIndex), FloatNumberFormat);
		positionY = float.Parse(NextSubstring(data, ref dataIndex), FloatNumberFormat);
		uiMapID = long.Parse(NextSubstring(data, ref dataIndex));
		facing = float.Parse(NextSubstring(data, ref dataIndex), FloatNumberFormat);
		level = int.Parse(NextSubstring(data, ref dataIndex));
	}
}