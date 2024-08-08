using static CombatlogParser.ParsingUtil;

namespace CombatlogParser.Data.Events.EventData;

public class SpellData
{
    private readonly static Dictionary<int, SpellData> knownSpells = new();

    public readonly static SpellData MeleeHit = new(1, "Melee", SpellSchool.Physical);

    public int id;
    public string name;
    public SpellSchool school;

    private SpellData(int id, string name, SpellSchool school)
    {
        this.id = id;
        this.name = name;
        this.school = school;
    }

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