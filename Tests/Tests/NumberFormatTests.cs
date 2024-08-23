using CombatlogParser.Formatting;
using System.Globalization;

namespace CombatlogParser.Tests;

public class NumberFormatTests
{
	[Test]
	public void ShortFormString()
	{
		double a = 10.0;
		double b = 1_500.0;
		double c = 165_400.0;
		double d = 1_760_000.0;
		double e = 26_331_000.0;
		double f = 27_356_000.0;
		string decimalSeperator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
		Assert.Multiple(() =>
			{
				Assert.That(a.ToShortFormString(), Is.EqualTo($"10{decimalSeperator}00"));
				Assert.That(b.ToShortFormString(), Is.EqualTo($"1{decimalSeperator}50k"));
				Assert.That(c.ToShortFormString(), Is.EqualTo($"165{decimalSeperator}4k"));
				Assert.That(d.ToShortFormString(), Is.EqualTo($"1{decimalSeperator}76m"));
				Assert.That(e.ToShortFormString(), Is.EqualTo($"26{decimalSeperator}33m"));
				Assert.That(f.ToShortFormString(), Is.EqualTo($"27{decimalSeperator}36m"));
			});
	}
}
