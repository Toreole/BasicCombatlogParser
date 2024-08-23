using System.Text;

namespace CombatlogParser.Formatting;

public static class NumberFormatting
{
	static readonly char[] magnitudeSymbols = { 'k', 'm', 'b', 't' }; //not expecting wow to ever go to trillions and higher

	/// <summary>
	/// Formats a possibly very large number in a way that it has a maximum of 3 digits before the decimal and a total of 4 digits at all times,
	/// signaling the magnitude with "k", "m", etc. appropriately.
	/// Example: 123456789 will be formatted as 123.4m, which isnt correctly rounded, but acceptable.
	/// </summary>
	/// <param name="number"></param>
	/// <returns></returns>
	public static string ToShortFormString(this double number)
	{
		double num = number;
		int magnitudeIndex = -1;
		while (num > 1000.0)
		{
			magnitudeIndex++;
			num /= 1000.0;
		}
		//the resulting string should show a max of 4 total digits, and a max of 2 decimal places.
		int frontDigits = 1;
		frontDigits += num >= 10.0 ? 1 : 0;
		frontDigits += num >= 100.0 ? 1 : 0;
		int decimalDigits = Math.Min(2, 4 - frontDigits);
		StringBuilder stringBuilder = new(num.ToString($"N{decimalDigits}"));
		if (magnitudeIndex != -1)
			stringBuilder.Append(magnitudeSymbols[magnitudeIndex]);
		return stringBuilder.ToString();
	}
	/// <summary>
	/// see <see cref="ToShortFormString(double)"/>
	/// </summary>
	/// <param name="number"></param>
	/// <returns></returns>
	public static string ToShortFormString(this long number) => ((double)number).ToShortFormString();

	/// <summary>
	/// Formats an amount of seconds as a string in the "mm:ss" format.
	/// </summary>
	/// <param name="seconds"></param>
	/// <returns></returns>
	public static string SecondsToMinutesAndSeconds(double seconds)
	{
		return TimeSpan.FromSeconds(Math.Max(0, seconds)).ToString(@"mm\:ss");
	}

	/// <summary>
	/// Formats millisecond time as a human readable string 
	/// e.g. 15240 becomes 0:15.240
	/// </summary>
	/// <param name="milliseconds">the total milliseconds</param>
	/// <param name="includeMillis">whether to include the trailing millis after seconds</param>
	public static string MillisecondsToReadableTimeString(uint milliseconds, bool includeMillis = false)
	{
		//the trailing milliseconds at the end.
		uint remMil = milliseconds % 1000;

		milliseconds = (milliseconds - remMil) / 1000;
		//seconds are now the first thing
		uint seconds = milliseconds % 60;
		//adjust for overflow
		seconds = seconds >= 60 ? 0 : seconds;

		milliseconds = (milliseconds - seconds) / 60;

		uint minutes = milliseconds % 60;
		//adjust for overflow
		minutes = minutes >= 60 ? 0 : minutes;

		return includeMillis ?
			  $"{minutes}:{seconds:00}.{remMil:000}"
			: $"{minutes}:{seconds:00}";
	}
}
