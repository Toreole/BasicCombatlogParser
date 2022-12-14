namespace CombatlogParser.Data
{
    /// <summary>
    /// Holds the Info about a player based on the COMBATANT_INFO log event.
    /// </summary>
    public class PlayerInfo
    {
        public string Name { get; private set; } = string.Empty;
        public string Realm { get; private set; } = string.Empty;
        public string GUID { get; set; } = string.Empty;
        public int Strength { get; set; }
        public int Agility { get; set; }
        public int Stamina { get; set; }
        public int Intelligence { get; set; }

        public void SetNameAndRealm(string sourceName)
        {
            int seperator = sourceName.IndexOf('-');
            if(seperator != -1)
            {
                Name = sourceName[..seperator];
                Realm = sourceName[(seperator+1)..];
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
        //    CurrentSpecID;

        //public UndefinedType
        //    Talents,
        //    PvPTalents,
        //    ArtifactTraits,
        //    EquippedItems,
        //    BonusItems,
        //    Gems,
        //    Auras;

    }
}
