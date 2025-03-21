﻿using CombatlogParser.Data.WowEnums;
using CombatlogParser.Events.EventData;
using CombatlogParser.Parsing;
using System.Text.RegularExpressions;

namespace CombatlogParser.Events
{
	public partial class DamageEvent : AdvancedParamEvent, ISpellEvent
	{
		//the leading bits of data.
		private readonly SpellData spellData;

		public SpellData SpellData => spellData;

		//what follows
		private readonly DamageEventParams damageParams;

		public DamageEventParams DamageParams => damageParams;

		public DamageEvent(CombatlogEventPrefix prefix, string entry, int dataIndex)
			: base(entry, ref dataIndex, EventType.DAMAGE, prefix, CombatlogEventSuffix._DAMAGE)
		{
			spellData = SpellData.ParseOrGet(prefix, entry, ref dataIndex);
			AdvancedParams = new(entry, ref dataIndex);
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
					spellData.name = nextString;
				}
			}
			damageParams = new(entry, ref dataIndex);
		}

		[GeneratedRegex("([0-9])")]
		private static partial Regex NumericInteger();
	}
}
