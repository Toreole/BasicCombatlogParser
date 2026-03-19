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
	public static float Median(float[] sortedValues)
	{
		if (sortedValues.Length % 2 == 1)
		{
			return sortedValues[sortedValues.Length / 2];
		}
		else
		{
			var halfAboveMiddle = sortedValues.Length / 2;
			return 0.5f * (sortedValues[halfAboveMiddle] + sortedValues[halfAboveMiddle - 1]);
		}
	}
}
