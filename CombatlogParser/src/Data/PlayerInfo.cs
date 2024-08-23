namespace CombatlogParser.Data;

/// <summary>
/// Holds the Info about a player based on the COMBATANT_INFO log event.
/// </summary>
public class PlayerInfo : CombatlogUnit
{
	public string Realm { get; set; } = string.Empty;
	public int Strength { get; set; }
	public int Agility { get; set; }
	public int Stamina { get; set; }
	public int Intelligence { get; set; }
	public ClassId Class { get; set; }
	public SpecId SpecId { get; set; }

	public int ItemLevel { get; set; }

	public void SetNameAndRealm(string sourceName)
	{
		int seperator = sourceName.IndexOf('-');
		if (seperator != -1)
		{
			this.Name = sourceName[..seperator];
			Realm = sourceName[(seperator + 1)..];
		}
		else
		{
			Name = sourceName;
		}
	}


	//public UndefinedType 
	//    Dodge,
	//    Parry,
	//    Block,
	//    CritMelee,
	//    CritRanged, 
	//    CritSpell,
	//    Speed,
	//    Lifesteal,
	//    HasteMelee,
	//    HasteRanged,
	//    HasteSpell,
	//    Avoidance,
	//    Mastery,
	//    VersatilityDamageDone,
	//    VersatilityHealingDone,
	//    VersatilityDamageTaken,
	//    Armor,

	//public UndefinedType
	//    Talents,
	//    PvPTalents,
	//    ArtifactTraits,
	//    EquippedItems,
	//    BonusItems,
	//    Gems,
	//    Auras;
}