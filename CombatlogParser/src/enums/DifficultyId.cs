namespace CombatlogParser
{
    public enum DifficultyId
    {
        //There are many more difficultyIds: https://wowpedia.fandom.com/wiki/DifficultyID 
        //but not all of them are required for this to work.
        UNKNOWN = 0,

        Normal_Group = 1,
        Heroic_Group = 2,
        Mythic_Group = 23,
        Mythic_Keystone = 8,

        Timewalking_Group = 24,
        Timewalking_Raid = 33,

        Normal_Raid = 14,
        Heroic_Raid = 15,
        Mythic_Raid = 16,

        Legacy_LFR = 7,
        LFR = 17,

        World_Boss = 172
    }
}
