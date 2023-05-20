namespace CombatlogParser
{
    public enum EncounterId
    {
        UNKOWN = 0,
        All_Bosses = -2,

        //Shadowlands:
        Hungering_Destroyer = 2383,
        Shriekwing = 2398,
        Sludgefist = 2399,
        Sun_Kings_Salvation = 2402,
        Artificer_Xymox = 2405,
        Lady_Invera_Darkvein = 2406,
        Sire_Denathrius = 2407,
        Council_of_Blood = 2412,
        Stone_Legion_Generals = 2417,
        Huntsman_Altimor = 2418,

        KelThuzad = 2422,
        Tarragrue = 2423,
        The_Nine = 2429,
        Painsmith_Raznal = 2430,
        Fatescribe_RohKalo = 2431,
        Remnant_of_Nerzhul = 2432,
        Eye_of_the_Jailer = 2433,
        Guardian_of_the_First_Ones = 2436,
        Soulrender_Dormazain = 2434,
        Sylvanas_Windrunner = 2435,

        Vigilant_Vuardian = 2512,
        Halondrus_the_Reclaimer = 2529,
        The_Jailer = 2537,
        Lihuvim = 2539,
        Dausegne = 2540,
        Skolex = 2542,
        Lords_of_Dread = 2543,
        Prototype_Pantheon = 2544,
        Anduin_Wrynn = 2546,
        Rygelon = 2549,
        Artficer_Xymox_SotFO = 2553,

        //Dragonflight:
        Eranog = 2587, 
        Primal_Council = 2590,
        Sennarth = 2592,
        Kurog_Grimtotem = 2605,
        Raszageth = 2607,
        Broodkeeper_Diurna = 2614,
        Dathea_Ascended = 2635,
        Terros = 2639,

        Kazzara_the_Hellforged = 2688,
        The_Amalgamation_Chamber = 2687,
        The_Forgotten_Experiments = 2693,
        Assault_of_the_Zaqali = 2682,
        Rashok = 2680,
        Zskarn = 2689,
        Magmorax = 2683,
        Echo_of_Neltharion = 2684,
        Scalecommander_Sarkareth = 2685,

    }

    public static class EncounterUtil
    {
        public static EncounterId[] GetEncounters(this InstanceId instance)
        {
            return instance switch
            {
                InstanceId.Castle_Nathria => new[] {
                    EncounterId.Shriekwing, 
                    EncounterId.Huntsman_Altimor, 
                    EncounterId.Hungering_Destroyer, 
                    EncounterId.Lady_Invera_Darkvein,
                    EncounterId.Artificer_Xymox,
                    EncounterId.Sun_Kings_Salvation,
                    EncounterId.Council_of_Blood,
                    EncounterId.Sludgefist,
                    EncounterId.Stone_Legion_Generals,
                    EncounterId.Sire_Denathrius
                },
                InstanceId.Sanctum_of_Domination => new[] { 
                    EncounterId.Sylvanas_Windrunner, 
                    EncounterId.Tarragrue, 
                    EncounterId.Eye_of_the_Jailer,
                    EncounterId.The_Nine,
                    EncounterId.Soulrender_Dormazain,
                    EncounterId.Remnant_of_Nerzhul,
                    EncounterId.Painsmith_Raznal,
                    EncounterId.Guardian_of_the_First_Ones,
                    EncounterId.Fatescribe_RohKalo,
                    EncounterId.KelThuzad
                },
                InstanceId.Sepulcher_of_the_First_Ones => new[]
                {
                    EncounterId.Vigilant_Vuardian,
                    EncounterId.Skolex,
                    EncounterId.Dausegne,
                    EncounterId.Prototype_Pantheon,
                    EncounterId.Lihuvim,
                    EncounterId.Artficer_Xymox_SotFO,
                    EncounterId.Halondrus_the_Reclaimer,
                    EncounterId.Anduin_Wrynn,
                    EncounterId.Lords_of_Dread,
                    EncounterId.Rygelon,
                    EncounterId.The_Jailer
                },

                InstanceId.Vault_of_the_Incarnates => new[]
                {
                    EncounterId.Eranog,
                    EncounterId.Primal_Council,
                    EncounterId.Dathea_Ascended,
                    EncounterId.Terros,
                    EncounterId.Sennarth,
                    EncounterId.Kurog_Grimtotem,
                    EncounterId.Broodkeeper_Diurna,
                    EncounterId.Raszageth
                },
                InstanceId.Aberrus_the_Shadowed_Crucible => new[]
                {
                    EncounterId.Kazzara_the_Hellforged,
                    EncounterId.The_Amalgamation_Chamber,
                    EncounterId.The_Forgotten_Experiments,
                    EncounterId.Assault_of_the_Zaqali,
                    EncounterId.Rashok,
                    EncounterId.Zskarn,
                    EncounterId.Magmorax,
                    EncounterId.Echo_of_Neltharion,
                    EncounterId.Scalecommander_Sarkareth
                },

                _ => Array.Empty<EncounterId>()
            };
        }
        public static InstanceId GetInstanceId(this EncounterId encounter)
        {
            return encounter switch
            {
                EncounterId.Hungering_Destroyer or
                EncounterId.Shriekwing or
                EncounterId.Sun_Kings_Salvation or
                EncounterId.Artificer_Xymox or
                EncounterId.Lady_Invera_Darkvein or
                EncounterId.Sire_Denathrius or
                EncounterId.Council_of_Blood or
                EncounterId.Stone_Legion_Generals or
                EncounterId.Huntsman_Altimor or
                EncounterId.Sludgefist
                 => InstanceId.Castle_Nathria,

                EncounterId.KelThuzad or
                EncounterId.Tarragrue or
                EncounterId.The_Nine or
                EncounterId.Painsmith_Raznal or
                EncounterId.Fatescribe_RohKalo or
                EncounterId.Remnant_of_Nerzhul or
                EncounterId.Eye_of_the_Jailer or
                EncounterId.Soulrender_Dormazain or
                EncounterId.Sylvanas_Windrunner or
                EncounterId.Guardian_of_the_First_Ones
                 => InstanceId.Sanctum_of_Domination,

                EncounterId.Vigilant_Vuardian or
                EncounterId.Halondrus_the_Reclaimer or
                EncounterId.The_Jailer or
                EncounterId.Lihuvim or
                EncounterId.Dausegne or
                EncounterId.Skolex or
                EncounterId.Lords_of_Dread or
                EncounterId.Prototype_Pantheon or
                EncounterId.Anduin_Wrynn or
                EncounterId.Rygelon or
                EncounterId.Artficer_Xymox_SotFO
                => InstanceId.Sepulcher_of_the_First_Ones,

                EncounterId.Eranog or
                EncounterId.Primal_Council or
                EncounterId.Sennarth or
                EncounterId.Kurog_Grimtotem or
                EncounterId.Raszageth or
                EncounterId.Broodkeeper_Diurna or
                EncounterId.Dathea_Ascended or
                EncounterId.Terros
                => InstanceId.Vault_of_the_Incarnates,

                EncounterId.Kazzara_the_Hellforged or
                EncounterId.The_Amalgamation_Chamber or
                EncounterId.The_Forgotten_Experiments or
                EncounterId.Assault_of_the_Zaqali or
                EncounterId.Rashok or
                EncounterId.Zskarn or
                EncounterId.Magmorax or
                EncounterId.Echo_of_Neltharion or
                EncounterId.Scalecommander_Sarkareth 
                => InstanceId.Aberrus_the_Shadowed_Crucible,

                _ => InstanceId.UNKNOWN
            };
        }
    }
}
