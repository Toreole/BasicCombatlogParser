namespace CombatlogParser;

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
}
