using CombatlogParser.Data.WowEnums;
using static CombatlogParser.Parsing.ParsingUtil;

namespace CombatlogParser.Events.EventData;

public class SpellData
{
	private readonly static Dictionary<int, SpellData> knownSpells = [];

	public readonly static SpellData MeleeHit = new(1, "Melee", SpellSchool.Physical);

	public int Id { get; internal set; }
	public string Name { get; internal set; }
	public SpellSchool School { get; internal set; }

	private SpellData(int id, string name, SpellSchool school)
	{
		Id = id;
		Name = name;
		School = school;
	}

	/// <summary>
	/// Attempts to obtain SpellData from a string at a given index.
	/// Depending on the known prefix, SpellData might not exist at the expected location
	/// and is then defaulted to "Melee". (Ranged auto attacks for hunters have SpellData)
	/// </summary>
	/// <param name="prefix">The prefix used</param>
	/// <param name="line">The full line of the combatlog</param>
	/// <param name="index">Index where to expect SpellData in the log.</param>
	/// <returns></returns>
	public static SpellData ParseOrGet(CombatlogEventPrefix prefix, string line, ref int index)
	{
		if (prefix is CombatlogEventPrefix.SWING or CombatlogEventPrefix.ENVIRONMENTAL)
		{
			return MeleeHit;
		}
		int spellId = int.Parse(NextSubstring(line, ref index));
		if (knownSpells.TryGetValue(spellId, out SpellData? value))
		{
			MovePastNextDivisor(line, ref index);
			MovePastNextDivisor(line, ref index);
			return value;
		}
		var spellName = string.Intern(NextSubstring(line, ref index));
		var spellSchool = (SpellSchool)HexStringToUInt(NextSubstring(line, ref index));
		SpellData data = new(spellId, spellName, spellSchool);
		knownSpells[spellId] = data;
		return data;
	}

	internal static void ResetStoredSpells()
	{
		knownSpells.Clear();
	}
}