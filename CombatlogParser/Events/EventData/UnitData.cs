using static CombatlogParser.Parsing.ParsingUtil;

namespace CombatlogParser.Events.EventData;

public class UnitData
{
	private static readonly Dictionary<string, UnitData> knownUnits = [];

	public readonly string unitGUID;
	public readonly string unitName;
	//UnitFlags and RaidFlags not included because those can change dynamically.
	//TargetMarkers (RaidFlags) are set and changed all the time
	//and debuffs like corruption on Echo of Neltharion Heroic can change a players affiliation to be hostile to other players.

	private UnitData(string unitGUID, string unitName)
	{
		this.unitGUID = unitGUID;
		this.unitName = unitName;
	}

	public static UnitData GetOrParse(string entry, ref int dataIndex)
	{
		string guid = NextSubstring(entry, ref dataIndex);
		if (knownUnits.TryGetValue(guid, out UnitData? value))
		{
			//skip name
			MovePastNextDivisor(entry, ref dataIndex);
			return value;
		}
		var name = NextSubstring(entry, ref dataIndex);
		var unit = new UnitData(guid, name);
		knownUnits[guid] = unit;
		return unit;
	}

	public static void ResetStoredUnits()
	{
		knownUnits.Clear();
	}
}