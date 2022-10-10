namespace CombatlogParser
{
    public enum SpellSchool
    {
        UNDEFINED = 0,
        //the basic ones
        Physical = 1,
        Holy = 2,
        Fire = 4,
        Nature = 8,
        Frost = 16,
        Shadow = 32,
        Arcane = 64,

        //combinations
        Holystrike = 3,
        Flamestrike = 5,
        Radiant = 6,
        Stormstrike = 9,
        Holystorm = 10,
        Firestorm = 12,
        Froststrike = 16,
        Holyfrost = 18,
        Frostfire = 20,
        Froststorm = 24,
        Shadowstrike = 33,
        Twilight = 34,
        Shadowlame = 36,
        Plague = 40,
        Shadowfrost = 48,
        Spellstrike = 65,
        Divine = 66,
        Spellfire = 68,
        Astral = 72,
        Spellfrost = 80,
        Spellshadow = 96,
        //multi
        Elemental = 28,
        Chromatic = 62,
        Cosmic = 106,
        Chaos = 124,
        Magic = 126,
        TrueChaos = 127 //this one is just named Chaos in wowpedia, but cant exactly have multiple enum values with the same name.
    }
}
