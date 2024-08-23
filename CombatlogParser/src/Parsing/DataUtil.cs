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
				if (performance != null && performance.PlayerMetadata != null && performance.PlayerMetadata.GUID.EndsWithF(searchGUID))
				{
					perf = performance;
					return true;
				}
			perf = null;
			return false;
		}


		/// <summary>
		/// Sorts the totals in the dictionary in a descending order and outputs them alongside the units GUID in an array.
		/// </summary>
		/// <param name="lookup"></param>
		/// <param name="total">The total of all values</param>
		/// <returns></returns>
		public static KeyValuePair<T, long>[] SortDescendingAndSumTotal<T>(this Dictionary<T, long> lookup, out long total) where T : notnull
		{
			var results = new KeyValuePair<T, long>[lookup.Count];
			int i = 0;
			total = 0;
			foreach (var pair in lookup.OrderByDescending(x => x.Value))
			{
				results[i] = new(
					pair.Key,
					pair.Value
				);
				total = pair.Value + total;
				i++;
			}
			return results;
		}

	}
}
