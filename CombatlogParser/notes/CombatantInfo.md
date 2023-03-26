## COMBATANT_INFO payload
as of 25.03.2023, using some random event i found in a log file from 10.0.5
### Basics
- header: 3/15 20:37:17.333  COMBATANT_INFO, 
- PlayerGUID: Player-3391-0BFD6FBD,
- Unknown: 1,
- Strength/Agility/Stamina/Intelligence: 1033,1531,16855,10244,
- Dodge/Parry/Block: 0,0,0, 
- CritMelee/CritRanged/CritSpell: 2851,2851,2851, 
- Speed: 250, 
- Leech: 0, 
- HasteMelee/HasteRanged/HasteSpell: 2313,2313,2313, 
- Avoidance: 325, 
- Mastery: 3929, : mastery 
- VersatilityMelee/VersatilityRanged/VersatilitySpell: 2559,2559,2559, 
- Armor: 1989, 
- SpecID: 64, 

### Talents

Class and Spec:

There is some uncertainty what the first two numbers of each element mean, but the 3rd is the level of the talent.
Maybe something like (?, entryId, level)

[(62176,80241,1),(62177,80242,1),(62175,80240,1),(62164,80227,1),(62165,80228,1),(62178,80243,1),(62166,80229,1),(81468,102429,1),(62174,80239,1),(62173,80238,1),(62179,80244,1),(62181,80247,1),(62171,80235,1),(62167,80230,1),(62120,80179,1),(62113,80171,1),(62122,80181,1),(62118,80177,1),(62124,80183,1),(62115,80174,1),(62104,80161,1),(62127,80187,1),(62098,80155,1),(62096,80153,2),(62095,80152,1),(62102,80159,1),(62158,80221,2),(62161,80224,1),(62168,80231,1),(62126,80186,1),(62111,80169,2),(62114,80173,2),(62152,80215,1),(62154,80217,1),(62155,80218,1),(62156,80219,1),(62162,80225,2),(62085,80141,1),(62086,80142,1),(62128,80188,2),(62153,80216,1),(62094,80150,1),(62172,80236,2),(62105,80163,1),(62112,80170,2),(62151,80214,1),(62117,80176,1),(62099,80156,2),(62100,80157,1),(62101,80158,2),(62159,80222,1),(62180,80246,1)],

PvP: 

(0,198126,198148,377360),
  
### Equipment

Each element represents one piece of equipment. They follow this schema:
- (
- ItemId
- ItemLvl
- (PermanentEnchantId, TempEnchantId, OnUseSpellEnchantId) or ()
- (BonusListID, ...)
- (Gem ID, Gem iLvl, ...) or ()
- )

| index | item data | slot / name |
| -: | --------- | ----------- |
|  0 | [(200318,415,(),(6652,7937,7981,8095,8828,1489),()), | head (mage set) |
|  1 | (193001,418,(),(8836,8840,8902,8960,8783,8782),(192982,415,192948,415,192948,415)), | neck (lariat) |
|  2 | (193526,418,(),(8836,8840,8902,8960),()), | shoulders (amice of the blue) |
|  3 | (0,0,(),(),()), | shirt (empty) |
|  4 | (200315,421,(6625,0,0),(6652,9130,7977,8830,1498),()), | chest (mage set) |
|  5 | (144081,415,(),(7977,6652,7935,8822,8818,9144,8973,3305,8767),(192948,415)), | waist (girdle of endemic anger) |
|  6 | (200319,415,(6544,0,0),(6652,7981,8095,8827,1498),()), | legs (mage set) |
|  7 | (143988,421,(6607,0,0),(9130,7977,6652,8822,8818,9144,3311,8767),()), | feet (flameheart sandals) |
|  8 | (134310,415,(6574,0,0),(7977,6652,7935,8822,8819,9144,8973,3294,8767),(192948,415)), | wrist (manawrecker bindings) |
|  9 | (200317,421,(),(6652,8817,8822,9130,7977,8829,1498),()), | hands (mage set) |
| 10 | (192999,418,(6556,0,0),(8836,8840,8902,8780),(192948,415)), | finger 1 (signet of titanic insight) |
| 11 | (193000,418,(6556,0,0),(8836,8840,8902,8780),(192948,415)), | finger 2 (ring bound hourglass) |
| 12 | (193677,415,(),(7977,6652,9144,8973,1637,8767),()), | trinket 1 (furious ragefeather) |
| 13 | (194310,411,(),(6652,7980,1485,8767),()), | trinket 2 (desperate invokers codex) |
| 14 | (144111,415,(6592,0,0),(7977,6652,8822,8819,9144,8973,3305,8767),()), | back (wind soaked drape) |
| 15 | (193699,421,(6643,6514,0),(9130,7977,6652,9147,1643,8767),()), | main hand (staff of violent storms) |
| 16 | (0,0,(),(),()), | off-hand (empty) |
| 17 | (0,0,(),(),())], | tabard (empty) |

### Interesting Auras

An array of a GUID followed by a spellId (number).

[Player-3391-0BFD6FBD,278309 ,Player-3391-0BFD6FBD,382144, Player-3391-0BFD6FBD,382103, Player-3391-0BFD6FBD,396092, Player-3391-0AC2737C,21562, Player-3391-0BFD6FBD,393438, Player-3391-09082B36,389685, Player-3391-09B0CA05,1459, Player-3391-0BFD6FBD,371172, Player-3391-068572B4,1126, Player-3391-0C57F7E3,381750, Player-3391-0C4C04FA,6673],

### I don't know what this is supposed to be

30,0,0,0 
