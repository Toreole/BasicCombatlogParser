namespace CombatlogParser
{
    public enum PowerType
    {
        HealthCost = -2,
        None = -1,
        Mana = 0,
        Rage = 1,
        Focus = 2,
        Energy = 3,
        ComboPoints = 4,
        Runes = 5,
        RunicPower = 6,
        SoulShards = 7,
        LunarPower = 8,
        HolyPower = 9,
        Alternate = 10,
        Maelstrom = 11,
        Chi = 12,
        Insanity = 13,
        Obsolete = 14,
        Obsolete2 = 15,
        ArcaneCharges = 16,
        Fury = 17,
        Pain = 18,
        NumPowerTypes = 19, //this is a value in the original PowerType enum, but it should never be used in a combat log.
    }
}
