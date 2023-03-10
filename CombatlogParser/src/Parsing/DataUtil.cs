using CombatlogParser.Data.Metadata;

namespace CombatlogParser.Parsing
{
    public static class DataUtil
    {
        /// <summary>
        /// Try to find a PerformanceMetadata object in an array by the playerGUID.
        /// </summary>
        /// <param name="pds"></param>
        /// <param name="searchGUID"></param>
        /// <param name="perf"></param>
        /// <returns></returns>
        public static bool TryGetByGUID(this PerformanceMetadata[] pds, string searchGUID, out PerformanceMetadata? perf)
        {
            foreach (var performance in pds)
                if (performance.PlayerMetadata != null && performance.PlayerMetadata.GUID.EndsWithF(searchGUID))
                {
                    perf = performance;
                    return true;
                }
            perf = null;
            return false;
        }
    }
}
