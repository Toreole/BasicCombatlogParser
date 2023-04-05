Healing in Combatlogs:

### simple types of healing
SPELL_HEAL:

> Player-3391-0C4C04FA,"Crowfield-Silvermoon",0x511,0x0,Player-3391-0C4C04FA,"Crowfield-Silvermoon",0x511,0x0,382551,"Heilender Schmerz",0x1,Player-3391-0C4C04FA,0000000000000000,331022,340560,12466,1492,8582,24516,1,1030,1300,0,-249.50,362.22,2122,1.5493,418,15325,15325,0,0,nil

> Player-3391-0BAE466A,"Iklad-Silvermoon",0x514,0x0,Creature-0-3894-2522-7208-89-0000121E6D,"Höllenbestie",0x2114,0x0,81269,"Erblühen",0x8,Creature-0-3894-2522-7208-89-0000121E6D,Player-3391-084E3343,288650,288650,10609,10609,7932,0,1,0,0,0,-245.85,367.78,2122,0.0000,416,3173,3173,3173,0,nil

SPELL_PERIODIC_HEAL:

> Player-3391-0C57F7E3,"Roggalot-Silvermoon",0x514,0x0,Player-3391-0AF07C2C,"Sjoowie-Silvermoon",0x514,0x2,394457,"Versprechen der Bruthüterin",0x40,Player-3391-0AF07C2C,0000000000000000,714161,742700,13190,2193,60433,0,1,451,1000,0,-255.10,371.20,2122,1.1491,418,1361,1361,0,0,nil

### absorb healing

SPELL_ABSORBED and SPELL_DAMAGE

THE BELOW GRAYED OUT EVENT IS CAUSED BY AN ABSORB OF A MELEE SWING.
>3/15 20:37:19.483  SPELL_ABSORBED,Creature-0-3894-2522-7208-190496-0000121C5F,"Terros",0x10a48,0x0,Player-3391-0AF07C2C,"Sjoowie-Silvermoon",0x514,0x2,Player-3391-0C57F7E3,"Roggalot-Silvermoon",0x514,0x0,373862,"Zeitanomalie",0x40,9492,469847,nil

| Regular SPELL_ABSORBED Data | Label |
| ------------------- | ----- |
| Creature-0-3894-2522-7208-190496-0000121C5F,"Terros",0x10a48,0x0, | Source Unit |
| Player-3391-0C4C04FA,"Crowfield-Silvermoon",0x511,0x0, | Dest Unit |
| 395686,"Erweckte Erde",0x8, | Spell Prefix Payload |
| Player-3391-0C4C04FA,"Crowfield-Silvermoon",0x511,0x0, | Absorb Caster Unit |
| 190456,"Zähne zusammenbeißen",0x1, | Absorb Spell ID/Name/School |
| 11407,31158,nil | amount, totalAmount, critical |

> Creature-0-3894-2522-7208-190496-0000121C5F,"Terros",0x10a48,0x0,Player-3391-0C4C04FA,"Crowfield-Silvermoon",0x511,0x0,395686,"Erweckte Erde",0x8,Player-3391-0C4C04FA,0000000000000000,323075,340560,12466,1492,8582,0,1,1300,1300,0,-259.44,363.57,2122,0.9956,418,17485,31158,-1,8,0,0,11407,nil,nil,nil

SPELL_HEAL_ABSORBED and SPELL_PERIODIC_HEAL
| SPELL_HEAL_ABSORBED Data | Label |
| ---- | ----- |
| Creature-0-3767-2522-10334-190245-00001225A5,"Bruthüterin Diurna",0x10a48,0x0, | Source Unit |
| Player-3391-0A4ADA5E,"Sîggy-Silvermoon",0x512,0x0, | Dest Unit | 
| 388717,"Eisiger Schleier",0x10, | Spell Prefix Payload |
| Player-3391-0C58B361,"Auravoker-Silvermoon",0x514,0x0, | Extra Unit | 
| 363534,"Zurückspulen",0x40, | Extra spell id/name/school |
| 31767,31767 | absorbedAmount/totalAmount |

| SPELL_PERIODIC_HEAL Data | Label |
| ------------------------ | ----- |
| Player-3391-0C58B361,"Auravoker-Silvermoon",0x514,0x0, | Source Unit | 
| Player-3391-0A4ADA5E,"Sîggy-Silvermoon",0x512,0x0, | Dest Unit |
| 363534,"Zurückspulen",0x40, | Spell Prefix Payload | 
| Player-3391-0A4ADA5E,0000000000000000,327274, 369101,10026,2192,4753, 0,0,50000,50000,0,84.33,5.70,2126,0.0825,417, | Advanced Combatlog Info |
| 0,31767,0,31767,nil | Heal Payload (amount, base, overheal, absorbed, critical) |

SPELL_HEAL_ABSORBED and SPELL_HEAL

> Creature-0-3767-2522-10334-190245-00001225A5,"Bruthüterin Diurna",0x10a48,0x0,Player-3391-0C4C04FA,"Crowfield-Silvermoon",0x511,0x0,388717,"Eisiger Schleier",0x10,Player-3391-0BB11FA9,"Borntiny-Silvermoon",0x514,0x0,81751,"Abbitte",0x2,11586,11586

| SPELL_HEAL Data | Label |
| --------------- | ----- |
| Player-3391-0BB11FA9,"Borntiny-Silvermoon",0x514,0x0, | Source Unit |
| Player-3391-0C4C04FA,"Crowfield-Silvermoon",0x511,0x0, | Dest Unit |
| 81751,"Abbitte",0x2, | Spell Prefix Payload | 
| Player-3391-0C4C04FA,0000000000000000,340560, 340560,12466,1492, 8582,74971,1,1239,1300, 0,93.27,9.94, 2126,4.4333,418, | Advanced Combatlog Info |
| 0,11586,0,11586,nil | Heal Payload |
