using System.Text;

namespace CombatlogParser.Formatting;

public static class NumberFormatting
{
    static readonly char[] magnitudeSymbols = { 'k', 'm', 'b', 't' }; //not expecting wow to ever go to trillions and higher
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
    public static string ToShortFormString(this long number) => ((double)number).ToShortFormString();
}
