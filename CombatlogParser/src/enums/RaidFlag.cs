using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CombatlogParser
{
    /// <summary>
    /// Less informational flags, more the targets of the raid markers.
    /// </summary>
    //technically not a flag because a unit can only have one raid target at a time.
    public enum RaidFlag : uint
    {
        None = 0,

        COMBATLOG_OBJECT_RAIDTARGET1 = 0x01,
        COMBATLOG_OBJECT_RAIDTARGET2 = 0x02,
        COMBATLOG_OBJECT_RAIDTARGET3 = 0x04,
        COMBATLOG_OBJECT_RAIDTARGET4 = 0x08,
        COMBATLOG_OBJECT_RAIDTARGET5 = 0x10,
        COMBATLOG_OBJECT_RAIDTARGET6 = 0x20,
        COMBATLOG_OBJECT_RAIDTARGET7 = 0x40,
        COMBATLOG_OBJECT_RAIDTARGET8 = 0x80,

        COMBATLOG_OBJECT_RAIDTARGET_MASK = 0xFF
    }
}
