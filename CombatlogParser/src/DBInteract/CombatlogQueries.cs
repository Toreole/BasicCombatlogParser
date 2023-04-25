using CombatlogParser.Data.DisplayReady;
using CombatlogParser.Data.Metadata;
using CombatlogParser.DBInteract;

namespace CombatlogParser;

public static partial class Queries
{
    /// <summary>
    /// Attempts getting the combatlog metadata for a log based on its ID.
    /// </summary>
    /// <param name="logID"></param>
    /// <returns>null if no such log can be found.</returns>
    public static CombatlogMetadata? GetCombatlogMetadataByID(uint logId)
    {
        using CombatlogDBContext context = new();
        return context.Combatlogs.FirstOrDefault(x => x.Id == logId);
    }

    public static CombatlogMetadata?[] GetCombatlogMetadata(uint lastId, int pageSize = 10)
    {
        using CombatlogDBContext context = new();
        return context.Combatlogs
            .Where(c => c.Id > lastId)
            .OrderBy(x => x.Id)
            .Take(pageSize)
            .ToArray();
    }

    public static EncounterInfoMetadata?[] GetEncounterInfoMetadata(uint lastId, int pageSize = 10)
    {
        using CombatlogDBContext context = new();
        return context.Encounters
            .Where(c => c.Id > lastId)
            .OrderBy(x => x.Id)
            .Take(pageSize)
            .ToArray();
    }

    public static PerformanceMetadata?[] GetPerformanceMetadata(uint lastId, int pageSize = 10)
    {
        using CombatlogDBContext context = new();
        return context.Performances
            .Where(c => c.Id > lastId)
            .OrderBy(x => x.Id)
            .Take(pageSize)
            .ToArray();
    }

    public static PlayerMetadata?[] GetPlayerMetadata(uint lastId, int pageSize = 10)
    {
        using CombatlogDBContext context = new();
        return context.Players
            .Where(c => c.Id > lastId)
            .OrderBy(x => x.Id)
            .Take(pageSize)
            .ToArray();
    }

    public static PerformanceMetadata? GetHighestDpsOnEncounterForPlayer(
        uint playerId, 
        EncounterId encounter, 
        DifficultyId difficulty
        )
    {
        using CombatlogDBContext context = new();
        return context.Performances
            .Where(p => p.PlayerMetadataId == playerId 
            && p.EncounterInfoMetadata!.WowEncounterId == encounter //p.WowEncounterId == encounter ----> WOWENCOUNTERID IS UNKNOWN FOR SOME REASON!
            && p.EncounterInfoMetadata!.Success
            && p.EncounterInfoMetadata!.DifficultyId == difficulty)
            .OrderBy(p => p.Dps)
            .FirstOrDefault();
    }

    public static int GetKillCountOnEncounterForPlayer(
        uint playerId,
        EncounterId encounter,
        DifficultyId difficulty
        )
    {
        using CombatlogDBContext context = new();
        return context.Performances
            .Where(p => p.PlayerMetadataId == playerId &&
            p.EncounterInfoMetadata!.WowEncounterId == encounter
            && p.EncounterInfoMetadata!.Success
            && p.EncounterInfoMetadata!.DifficultyId == difficulty)
            .Count();
    }

    public static PerformanceMetadata? GetMedianDpsOnKill(
        uint playerId,
        EncounterId encounter,
        DifficultyId difficulty
        )
    {
        using CombatlogDBContext context = new();
        var matching = context.Performances.Where(p =>
            p.PlayerMetadataId == playerId
            && p.EncounterInfoMetadata!.WowEncounterId == encounter
            && p.EncounterInfoMetadata!.Success
            && p.EncounterInfoMetadata!.DifficultyId == difficulty
        );
        int count = matching.Count();
        var medianEntry = matching.OrderBy(p => p.Dps).Skip(count / 2).FirstOrDefault();
        return medianEntry;
    }

    public static PlayerEncounterPerformanceOverview GetPerformanceOverview(
        uint playerId,
        EncounterId encounter,
        DifficultyId difficulty,
        MetricType metric
        )
    {
        using CombatlogDBContext context = new();
        var matching = context.Performances.Where(p =>
            p.PlayerMetadataId == playerId
            && p.EncounterInfoMetadata!.WowEncounterId == encounter
            && p.EncounterInfoMetadata!.Success
            && p.EncounterInfoMetadata!.DifficultyId == difficulty
        );
        switch(metric)
        {
            case MetricType.Dps: matching.OrderBy(p => p.Dps);
                break;
            case MetricType.Hps: matching.OrderBy(p => p.Hps);
                break;
        }
        int count = matching.Count();
        if(count == 0)
        {
            return new()
            {
                EncounterName = encounter.ToPrettyString() //only set encounter name, everything else is default.
            };
        }
        var medianEntry = matching.Skip(count >> 1).First();
        var bestEntry = matching.First();
        var fastest = matching.OrderBy(p => p.EncounterInfoMetadata!.EncounterDurationMS).First();
        //somehow fastest.EncounterInfoMetadata is null, so instead of bothering with figuring that out, just get it by id manually.
        var fastestEncounter = context.Encounters.Where(e => e.Id == fastest.EncounterInfoMetadataId).First();
        string highestMetric = metric switch
        {
            MetricType.Dps => bestEntry.Dps.ToString("0.0"),
            MetricType.Hps => bestEntry.Hps.ToString("0.0"),
            _ => string.Empty,
        };
        string medianMetric = metric switch
        {
            MetricType.Dps => medianEntry.Dps.ToString("0.0"),
            MetricType.Hps => medianEntry.Hps.ToString("0.0"),
            _ => string.Empty,
        };
        return new()
        {
            EncounterName = encounter.ToPrettyString(),
            FastestTime = ParsingUtil.MillisecondsToReadableTimeString((uint)fastestEncounter.EncounterDurationMS),
            HighestMetricValue = highestMetric,
            MedianMetricValue = medianMetric,
            KillCount = count.ToString()
        };
    }

    public static PerformanceMetadata[] GetDpsPerformances(
        uint playerId,
        EncounterId encounter,
        DifficultyId difficulty
        )
    {
        using CombatlogDBContext context = new();
        return context.Performances.Where(p =>
        p.PlayerMetadataId == playerId
        && p.EncounterInfoMetadata!.WowEncounterId == encounter
        && p.EncounterInfoMetadata!.DifficultyId == difficulty
        && p.EncounterInfoMetadata!.Success
        ).OrderBy(p => p.Dps).ToArray();
    }

    public static PerformanceMetadata[] GetHpsPerformances(
        uint playerId,
        EncounterId encounter,
        DifficultyId difficulty
        )
    {
        using CombatlogDBContext context = new();
        return context.Performances.Where(p =>
        p.PlayerMetadataId == playerId
        && p.EncounterInfoMetadata!.WowEncounterId == encounter
        && p.EncounterInfoMetadata!.DifficultyId == difficulty
        && p.EncounterInfoMetadata!.Success
        ).OrderBy(p => p.Hps).ToArray();
    }

    public static PlayerPerformance[] GetPlayerPerformances(
        uint playerId,
        EncounterId encounter,
        DifficultyId difficulty,
        MetricType metric
        )
    {
        var rawPerformances = metric switch
        {
            MetricType.Dps => GetDpsPerformances(playerId, encounter, difficulty),
            MetricType.Hps => GetHpsPerformances(playerId, encounter, difficulty),
            _ => Array.Empty<PerformanceMetadata>()
        };
        if (rawPerformances.Length == 0)
            return Array.Empty<PlayerPerformance>();
        var results = new PlayerPerformance[rawPerformances.Length];
        for(int i = 0; i < rawPerformances.Length; i++)
        {
            using CombatlogDBContext context = new();
            var encounterMetadata = context.Encounters.Where(e => e.Id == rawPerformances[i].EncounterInfoMetadataId).First();
            var combatlogMetadata = context.Combatlogs.Where(c => c.Id == encounterMetadata.CombatlogMetadataId).First();

            string durationString = ParsingUtil.MillisecondsToReadableTimeString((uint)encounterMetadata.EncounterDurationMS);
            string dateString = DateTime.UnixEpoch.AddMilliseconds(combatlogMetadata.MsTimeStamp).ToShortDateString();
            string ilvlString = rawPerformances[i].ItemLevel.ToString();
            string metricString = metric switch
            {
                MetricType.Dps => rawPerformances[i].Dps.ToString("0.0"),
                MetricType.Hps => rawPerformances[i].Hps.ToString("0.0"),
                _ => string.Empty
            };
            results[i] = new()
            {
                EncounterMetadataId = encounterMetadata.Id,
                MetricValue = metricString,
                Duration = durationString,
                Date = dateString,
                ItemLevel = ilvlString,
            };
        }
        return results;
    }

    public static EncounterInfoMetadata FindEncounterById(uint id)
    {
        using CombatlogDBContext context = new();
        return context.Encounters.First(x => x.Id == id);
    }
}
