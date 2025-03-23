using CombatlogParser.Data.WowEnums;
using CombatlogParser.Events.EventData;
using CombatlogParser.Parsing;
using System.Text.RegularExpressions;

namespace CombatlogParser.Events;

/// <summary>
/// Contains any
/// </summary>
[CombatlogEvent(CombatlogEventSuffix._DAMAGE, 
	allowedPrefixes: [
		CombatlogEventPrefix.SPELL, 
		CombatlogEventPrefix.SWING,
		CombatlogEventPrefix.SPELL_PERIODIC,
		CombatlogEventPrefix.RANGE,
		CombatlogEventPrefix.ENVIRONMENTAL
	])]
public partial class DamageEvent : AdvancedParamEvent, ISpellEvent
{
	public SpellData SpellData { get; private set; } = null!;

	public DamageEventParams DamageParams { get; private set; } = null!;

	public override void SetDataFrom(string entry, int dataIndex, CombatlogEventPrefix prefix)
	{
		SetBasicCombatlogData(entry, ref dataIndex);
		// prefix data. SpellData.ParseOrGet handles prefix variance.
		SpellData = SpellData.ParseOrGet(prefix, entry, ref dataIndex);
		SetAdvancedParams(entry, ref dataIndex);
		// suffix data
		if (prefix is CombatlogEventPrefix.ENVIRONMENTAL)
		{
			int x_index = dataIndex;
			var nextString = ParsingUtil.NextSubstring(entry, ref x_index);
			if (NumericInteger().Match(nextString).Success is false)
			{
				//if this isnt a number for damage, this is the "spell name"
				//for example: "Falling"
				//because for some reason, ENVIRONMENTAL_DAMAGE puts a name *after*
				//the advancedParams, but before the _DAMAGE payload.
				dataIndex = x_index;
				SpellData.Name = nextString;
			}
		}
		DamageParams = new(entry, ref dataIndex);
	}

	[GeneratedRegex("([0-9])")]
	private static partial Regex NumericInteger();
}
