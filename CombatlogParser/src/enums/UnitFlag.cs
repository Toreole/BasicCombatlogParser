﻿namespace CombatlogParser
{
    /// <summary>
    /// UnitFlag gives info about the type of unit, and its relation to the players party
    /// </summary>
    [Flags]
    public enum UnitFlag : uint
    {
        NONE = 0,
        //Affiliation flags
        COMBATLOG_OBJECT_AFFILIATION_MINE     = 0x1,
        COMBATLOG_OBJECT_AFFILIATION_PARTY    = 0x2,
        COMBATLOG_OBJECT_AFFILIATION_RAID     = 0x4,
        COMBATLOG_OBJECT_AFFILIATION_OUTSIDER = 0x8,
        COMBATLOG_OBJECT_AFFILIATION_MASK     = 0xF,
        //Reaction flags
        COMBATLOG_OBJECT_REACTION_FRIENDLY = 0x10,
        COMBATLOG_OBJECT_REACTION_NEUTRAL  = 0x20,
        COMBATLOG_OBJECT_REACTION_HOSTILE  = 0x40,
        COMBATLOG_OBJECT_REACTION_MASK     = 0xF0,
        //Controller flags
        COMBATLOG_OBJECT_CONTROL_PLAYER = 0x100,
        COMBATLOG_OBJECT_CONTROL_NPC    = 0x200,
        COMBATLOG_OBJECT_CONTROL_MASK   = 0x300,
        //Type flags
        COMBATLOG_OBJECT_TYPE_PLAYER   = 0x0400, //controller by player
        COMBATLOG_OBJECT_TYPE_NPC      = 0x0800, //controlled by server
        COMBATLOG_OBJECT_TYPE_PET      = 0x1000, //pets are controller by players or npcs, incl. mind control.
        COMBATLOG_OBJECT_TYPE_GUARDIAN = 0x2000, //not controlled, automatic defense
        COMBATLOG_OBJECT_TYPE_OBJECT   = 0x4000, //anything else, traps, totems.
        COMBATLOG_OBJECT_TYPE_MASK     = 0xFC00,
        //Special flags
        COMBATLOG_OBJECT_TARGET       = 0x10000,
        COMBATLOG_OBJECT_FOCUS        = 0x0020000,
        COMBATLOG_OBJECT_MAINTANK     = 0x00040000,
        COMBATLOG_OBJECT_MAINASSIST   = 0x00080000,
        COMBATLOG_OBJECT_NONE         = 0x80000000, //Whether the unit does not exist.
        COMBATLOG_OBJECT_SPECIAL_MASK = 0xFFFF0000
    }
}
