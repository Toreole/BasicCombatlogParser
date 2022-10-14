namespace CombatlogParser
{
    /// <summary>
    /// The prefixes for combat subevents.
    /// </summary>
    public enum CombatlogEventPrefix
    {
        UNDEFINED = -1,

        /// <summary>
        /// prefix params: [NONE]
        /// </summary>
        SWING,

        /// <summary>
        /// prefix params: spellID, spellName, spellSchool
        /// </summary>
        RANGE,

        /// <summary>
        /// prefix params: spellID, spellName, spellSchool
        /// </summary>
        SPELL,

        /// <summary>
        /// prefix params: spellID, spellName, spellSchool
        /// </summary>
        SPELL_PERIODIC,

        /// <summary>
        /// prefix params: spellID, spellName, spellSchool
        /// </summary>
        SPELL_BUILDING, //affects buildings/structures. e.g. Wintergrasp

        /// <summary>
        /// prefix params: environmentalType
        /// </summary>
        ENVIRONMENTAL
    }

    /// <summary>
    /// The suffixes for combat subevents.
    /// </summary>
    public enum CombatlogEventSuffix
    {
        UNDEFINED = -1,

        /// <summary>
        /// NOTE: in advanced, InfoGUID is SourceGUID (baseAmount only for SPELL_PERIODIC_DAMAGE / SPELL_DAMAGE / SWING_DAMAGE)
        /// Suffix params:
        /// amount, [baseAmount], overkill, school, resisted, blocked, absorbed, critical, glancing, crushing, isOffHand
        /// </summary>
        _DAMAGE,

        ///<summary>
        /// NOTE: only in advanced; InfoGUID is TargetGUID (only used for SWING_DAMAGE_LANDED) (baseAmount only for SWING_DAMAGE_LANDED)
        /// Suffix params:
        /// amount, [baseAmount], overkill, schoo, resisted, blocked, absorbed, critical, glancing, crushing, isOffHand
        /// </summary>
        _DAMAGE_LANDED,

        /// <summary>
        /// Suffix params:
        /// missType, isOffHand, amountMissed, baseAmount, critical
        /// </summary>
        _MISSED,

        /// <summary>
        /// Suffix params: (baseAmount only for SPELL_PERIODIC_HEAL and SPELL_HEAL)
        /// amount, [baseAmount], overhealing, absorbed, critical
        /// </summary>
        _HEAL,

        /// <summary>
        /// Suffix params:
        /// extraGUID, extraName, extraFlags, extraRaidFlags, extraSpellID, extraSpellName, extraSchool, absorbedAmount, totalAmount
        /// </summary>
        _HEAL_ABSORBED,

        /// <summary>
        /// DEPENDS ON CONTEXT:
        /// SPELLS: use SPELL *prefix* params
        /// SWINGS: dont have any prefix params. <br/>
        /// Suffix:
        /// casterGUID, casterName, casterFlags, casterRaidFlags, absorbSpellID, absorbSpellName, absorbSpellSchool, amount, totalAmount, critical
        /// </summary>
        _ABSORBED,

        /// <summary>
        /// Suffix params:
        /// amount, overEnergize, powerType, [maxPower]
        /// </summary>
        _ENERGIZE,

        /// <summary>
        /// Suffix params:
        /// amount, powerTyp, extraAmount, [maxPower]
        /// </summary>
        _DRAIN,

        /// <summary>
        /// Suffix params:
        /// amount, powerType, extraAmount
        /// </summary>
        _LEECH,

        /// <summary>
        /// Suffix params:
        /// extraSpellID, extraSpellName, extraSchool
        /// </summary>
        _INTERRUPT,

        /// <summary>
        /// Suffix params:
        /// extraSpellID, extraSpellName, extraSchool, auraType
        /// </summary>
        _DISPEL,

        /// <summary>
        /// Suffix params:
        ///extraSpellID, extraSpellName, extraSchool
        /// </summary>
        _DISPEL_FAILED,

        /// <summary>
        /// Suffix params:
        /// extraSpellID, extraSpellName, extraSchool, auraType
        /// </summary>
        _STOLEN,

        /// <summary>
        /// Suffix params:
        /// amount
        /// </summary>
        _EXTRA_ATTACKS,

        /// <summary>
        /// Suffix params:
        /// auraType
        /// </summary>
        _AURA_APPLIED,

        /// <summary>
        /// Suffix params:
        /// auraType
        /// </summary>
        _AURA_REMOVED,

        /// <summary>
        /// Suffix params:
        /// auraType, amount
        /// </summary>
        _AURA_APPLIED_DOSE,

        /// <summary>
        /// Suffix params:
        /// auraType, amount
        /// </summary>
        _AURA_REMOVED_DOSE,

        /// <summary>
        /// Suffix params:
        /// auraType, [amount]
        /// </summary>
        _AURA_REFRESH,

        /// <summary>
        /// Suffix params:
        /// auraType
        /// </summary>
        _AURA_BROKEN,

        /// <summary>
        /// Suffix params:
        /// extraSpellID, extraSpellName, extraSchool, auraType
        /// </summary>
        _AURA_BROKEN_SPELL,

        /// <summary>
        /// Suffix params:
        /// [NONE]
        /// </summary>
        _CAST_START,

        /// <summary>
        /// Suffix params:
        /// [NONE]
        /// </summary>
        _CAST_SUCCESS,

        /// <summary>
        /// Suffix params:
        /// failedType
        /// </summary>
        _CAST_FAILED,

        /// <summary>
        /// Suffix params:
        /// [unconsciousOnDeath]
        /// </summary>
        _INSTAKILL,

        /// <summary>
        /// Suffix params:
        /// [NONE]
        /// </summary>
        _DURABILITY_DAMAGE,

        /// <summary>
        /// Suffix params:
        /// [NONE]
        /// </summary>
        _DURABILITY_DAMAGE_ALL,

        /// <summary>
        /// Suffix params:
        /// [NONE]
        /// </summary>
        _CREATE,

        /// <summary>
        /// Suffix params:
        /// [NONE]
        /// </summary>
        _SUMMON,

        /// <summary>
        /// Suffix params:
        /// [NONE]
        /// </summary>
        _RESURRECT
    }

    /// <summary>
    /// Full events that dont adhere to the prefix/suffix scheme of combat events, and are treated seperately.
    /// </summary>
    public enum CombatlogMiscEvents
    {
        UNDEFINED = -1,

        /// <summary>
        /// See SPELL and _DAMAGE
        /// </summary>
        DAMAGE_SPLIT,

        /// <summary>
        /// See SPELL and _DAMAGE
        /// </summary>
        DAMAGE_SHIELD,

        /// <summary>
        /// See SPELL and _MISSED
        /// </summary>
        DAMAGE_SHIELD_MISSED,

        /// <summary>
        /// spellName, itemID, itemName
        /// </summary>
        ENCHANT_APPLIED,

        /// <summary>
        /// spellName, itemID, itemName
        /// </summary>
        ENCHANT_REMOVED,

        /// <summary>
        /// [NONE]
        /// </summary>
        PARTY_KILL,

        /// <summary>
        /// recapID, [unconsciousOnDeath]
        /// </summary>
        UNIT_DIED,

        /// <summary>
        /// recapID, [unconsciousOnDeath]
        /// </summary>
        UNIT_DESTROYED,

        /// <summary>
        /// recapID, [unconsciousOnDeath]
        /// </summary>
        UNIT_DISSIPATES,

        /// <summary>
        /// encounterID, encounterName, difficultyID, groupSize, instanceID
        /// </summary>
        ENCOUNTER_START,

        /// <summary>
        /// encounterID, encounterName, difficultyID, groupSize, success, fightTime
        /// </summary>
        ENCOUNTER_END,

        /// <summary>
        /// uiMapID, uiMapName, x0, x1, y0, y1
        /// </summary>
        MAP_CHANGE,

        /// <summary>
        /// instanceID, zoneName, difficultyID
        /// </summary>
        ZONE_CHANGE,

        /// <summary>
        /// instanceID, marker, x, y
        /// </summary>
        WORLD_MARKER_PLACED,

        /// <summary>
        /// marker
        /// </summary>
        WORLD_MARKER_REMOVED,

        /// <summary>
        /// The most complex log event. Contains all information about a players stats, equipment, and talents. 
        /// Is fired upon ENCOUNTER_START. For more see:
        /// https://wowpedia.fandom.com/wiki/COMBAT_LOG_EVENT#COMBATANT_INFO
        /// </summary>
        COMBATANT_INFO
    }
}
