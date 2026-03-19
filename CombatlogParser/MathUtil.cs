using System.Collections.Generic;
using System.Numerics;

namespace CombatlogParser;

/// <summary>
/// Static class for Math methods that are not present in the default C# Math classes.
/// </summary>
public static class MathUtil
{
	public static float InverseLerp(float value, float a, float b)
	{
		return (value - a) / (b - a);
	}
	public static float Average(params float[] values)
	{
		return values.Sum() / values.Length;
	}

	/// <summary>
	/// Calculates the median value of a sorted array of values.
	/// </summary>
	/// <param name="sortedValues"></param>
	/// <returns></returns>
	public static float Median(IReadOnlyList<float> sortedValues)
	{
		if (sortedValues.Count % 2 == 1)
		{
			return sortedValues[sortedValues.Count / 2];
		}
		else
		{
			var halfAboveMiddle = sortedValues.Count / 2;
			return 0.5f * (sortedValues[halfAboveMiddle] + sortedValues[halfAboveMiddle - 1]);
		}
	}

	/// <summary>
	/// Calculates the given quantile of a set of sorted values.
	/// </summary>
	/// <param name="sortedValues">ReadOnlyList of sorted values (ascending), with at least 1 value.</param>
	/// <param name="quantile">between 0 and 1</param>
	/// <returns></returns>
	public static float Quantile(IReadOnlyList<float> sortedValues, float quantile)
	{
		var index = (int)((sortedValues.Count - 1) * quantile);
		return sortedValues[index];
	}
}
