namespace CombatlogParser
{
    public enum ClassId
    {
        UNKOWN = 0,
        Warrior = 1,
        Paladin = 2,
        Hunter = 3,
        Rogue = 4,
        Priest,
        Death_Knight,
        Shaman,
        Mage,
        Warlock,
        Monk,
        Druid,
        Demon_Hunter,
        Evoker
    }
    public enum SpecId
    {
        UNKNOWN = 0,

        DK_Blood = 250,
        DK_Frost = 251,
        DK_Unholy = 252,

        DH_Havoc = 577,
        DH_Vengeance = 581,

        Druid_Balance = 102,
        Druid_Feral = 103,
        Druid_Guardian = 104,
        Druid_Restoration = 105,

        Evoker_Devastation = 1467,
        Evoker_Preservation = 1468,

        Hunter_BeastMastery = 253,
        Hunter_Marksman = 254,
        Hunter_Survival = 255,

        Mage_Arcane = 62,
        Mage_Fire = 63,
        Mage_Frost = 64,

        Monk_Brewmaster = 268,
        Monk_Mistweaver = 270,
        Monk_Windwalker = 269,

        Paladin_Holy = 65,
        Paladin_Protection = 66,
        Paladin_Retribution = 70,

        Priest_Discipline = 256,
        Priest_Holy = 257,
        Priest_Shadow = 258,

        Rogue_Assassination = 259,
        Rogue_Outlaw = 260,
        Rogue_Subtlety = 261,

        Shaman_Elemental = 262,
        Shaman_Enhancement = 263,
        Shaman_Restoration = 264,

        Warlock_Affliction = 265,
        Warlock_Demonology = 266,
        Warlock_Destruction = 267,

        Warrior_Arms = 71, //the best spec.
        Warrior_Fury = 72,
        Warrior_Protection = 73
    }

    public static class ClassSpecUtil
    {
        public static ClassId GetClassId(this SpecId spec)
        {
            return spec switch
            {
                SpecId.Mage_Arcane or SpecId.Mage_Fire or SpecId.Mage_Frost => ClassId.Mage,
                SpecId.DK_Blood or SpecId.DK_Frost or SpecId.DK_Unholy => ClassId.Death_Knight,
                SpecId.DH_Havoc or SpecId.DH_Vengeance => ClassId.Demon_Hunter,
                SpecId.Druid_Balance or SpecId.Druid_Feral or SpecId.Druid_Guardian or SpecId.Druid_Restoration => ClassId.Druid,
                SpecId.Evoker_Devastation or SpecId.Evoker_Preservation => ClassId.Evoker,
                SpecId.Hunter_BeastMastery or SpecId.Hunter_Marksman or SpecId.Hunter_Survival => ClassId.Hunter,
                SpecId.Monk_Brewmaster or SpecId.Monk_Mistweaver or SpecId.Monk_Windwalker => ClassId.Monk,
                SpecId.Paladin_Protection or SpecId.Paladin_Retribution or SpecId.Paladin_Holy => ClassId.Paladin,
                SpecId.Priest_Discipline or SpecId.Priest_Holy or SpecId.Priest_Shadow => ClassId.Priest,
                SpecId.Rogue_Assassination or SpecId.Rogue_Outlaw or SpecId.Rogue_Subtlety => ClassId.Rogue,
                SpecId.Shaman_Elemental or SpecId.Shaman_Enhancement or SpecId.Shaman_Restoration => ClassId.Shaman,
                SpecId.Warlock_Affliction or SpecId.Warlock_Demonology or SpecId.Warlock_Destruction => ClassId.Warlock,
                SpecId.Warrior_Arms or SpecId.Warrior_Fury or SpecId.Warrior_Protection => ClassId.Warrior,
                _ => ClassId.UNKOWN,
            };
        }

        public static SpecId[] GetSpecs(this ClassId @class)
        {
            return @class switch
            {
                ClassId.Death_Knight => new[] { SpecId.DK_Frost, SpecId.DK_Blood, SpecId.DK_Unholy },
                ClassId.Warrior => new[] { SpecId.Warrior_Arms, SpecId.Warrior_Fury, SpecId.Warrior_Protection },
                ClassId.Paladin => new[] { SpecId.Paladin_Holy, SpecId.Paladin_Protection, SpecId.Paladin_Retribution },
                ClassId.Hunter => new[] { SpecId.Hunter_Marksman, SpecId.Hunter_BeastMastery, SpecId.Hunter_Survival },
                ClassId.Rogue => new[] { SpecId.Rogue_Assassination, SpecId.Rogue_Outlaw, SpecId.Rogue_Subtlety },
                ClassId.Priest => new[] { SpecId.Priest_Discipline, SpecId.Priest_Holy, SpecId.Priest_Shadow },
                ClassId.Shaman => new[] { SpecId.Shaman_Enhancement, SpecId.Shaman_Elemental, SpecId.Shaman_Restoration },
                ClassId.Mage => new[] { SpecId.DK_Frost, SpecId.DK_Blood, SpecId.DK_Unholy },
                ClassId.Warlock => new[] { SpecId.Warlock_Affliction, SpecId.Warlock_Demonology, SpecId.Warlock_Destruction },
                ClassId.Monk => new[] { SpecId.Monk_Brewmaster, SpecId.Monk_Windwalker, SpecId.Monk_Mistweaver },
                ClassId.Druid => new[] { SpecId.Druid_Balance, SpecId.Druid_Feral, SpecId.Druid_Guardian, SpecId.Druid_Restoration },
                ClassId.Demon_Hunter => new[] { SpecId.DH_Havoc, SpecId.DH_Vengeance },
                ClassId.Evoker => new[] { SpecId.Evoker_Devastation, SpecId.Evoker_Devastation },
                _ => new[] { SpecId.UNKNOWN },
            };
        }

        public static RoleId GetRole(this SpecId spec)
        {
            return spec switch
            {
                SpecId.DK_Blood or
                SpecId.DH_Vengeance or
                SpecId.Druid_Guardian or
                SpecId.Monk_Brewmaster or
                SpecId.Paladin_Protection or
                SpecId.Warrior_Protection => RoleId.Tank,

                SpecId.Druid_Restoration or
                SpecId.Evoker_Preservation or
                SpecId.Monk_Mistweaver or
                SpecId.Paladin_Holy or
                SpecId.Priest_Discipline or
                SpecId.Priest_Holy or
                SpecId.Shaman_Restoration => RoleId.Heal,

                SpecId.DK_Frost or
                SpecId.DK_Unholy or
                SpecId.DH_Havoc or
                SpecId.Druid_Balance or
                SpecId.Druid_Feral or
                SpecId.Evoker_Devastation or
                SpecId.Hunter_BeastMastery or
                SpecId.Hunter_Marksman or
                SpecId.Hunter_Survival or
                SpecId.Mage_Arcane or
                SpecId.Mage_Fire or
                SpecId.Mage_Frost or
                SpecId.Monk_Windwalker or
                SpecId.Paladin_Retribution or
                SpecId.Priest_Shadow or
                SpecId.Rogue_Assassination or
                SpecId.Rogue_Outlaw or
                SpecId.Rogue_Subtlety or
                SpecId.Shaman_Elemental or
                SpecId.Shaman_Enhancement or
                SpecId.Warlock_Affliction or
                SpecId.Warlock_Demonology or
                SpecId.Warlock_Destruction or
                SpecId.Warrior_Arms or
                SpecId.Warrior_Fury => RoleId.DPS,

                _ => RoleId.UNKNOWN
            };
        }
    }
}
