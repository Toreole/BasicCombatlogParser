using System.Runtime.InteropServices;

namespace CombatlogParser.Data
{
    /// <summary>
    /// Holds the Info about a player based on the COMBATANT_INFO log event.
    /// </summary>
    public class PlayerInfo
    {
        public string GUID { get; set; } = "";
        public int Strength { get; set; }
        public int Agility { get; set; }
        public int Stamina { get; set; }
        public int Intelligence { get; set; }

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
