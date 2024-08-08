using CombatlogParser.Formatting;

namespace CombatlogParser.Tests
{
    public class UtilsTests
    {
        [Test]
        public void TestCountArguments()
        {
            Assert.Multiple(() =>
            {
                Assert.That(ParsingUtil.CountArguments("one,two,three", 0), Is.EqualTo(3));
                Assert.That(ParsingUtil.CountArguments("one,two,three", 5), Is.EqualTo(2));
				Assert.That(ParsingUtil.CountArguments("one,two,three", 20), Is.EqualTo(0));
                Assert.That(ParsingUtil.CountArguments("one,two,\"well, that, explains, it\",three", 0), Is.EqualTo(4));
			});
        }

		[Test]
        public void MillisToReadableString()
        {
            string resultA = NumberFormatting.MillisecondsToReadableTimeString(60_000);
            string resultB = NumberFormatting.MillisecondsToReadableTimeString(119_432);
            string resultC = NumberFormatting.MillisecondsToReadableTimeString(0);
            Assert.Multiple(() =>
            {
                Assert.That(resultA, Is.EqualTo("1:00"));
                Assert.That(resultB, Is.EqualTo("1:59"));
                Assert.That(resultC, Is.EqualTo("0:00"));
            });
        }

        [Test]
        public void MillisToReadableStringEnabledMillis()
        {
            string resultA = NumberFormatting.MillisecondsToReadableTimeString(60_000, true);
            string resultB = NumberFormatting.MillisecondsToReadableTimeString(119_432, true);
            string resultC = NumberFormatting.MillisecondsToReadableTimeString(0, true);
            Assert.Multiple(() =>
            {
                Assert.That(resultA, Is.EqualTo("1:00.000"));
                Assert.That(resultB, Is.EqualTo("1:59.432"));
                Assert.That(resultC, Is.EqualTo("0:00.000"));
            });
        }

        [Test]
        public void MultiIntParse()
        {
            int[] resultA = ParsingUtil.AllIntsIn("0|10|500");
            int[] resultB = ParsingUtil.AllIntsIn("10");
            Assert.Multiple(() =>
            {
                Assert.That(resultA, Is.EqualTo(new int[] { 0, 10, 500 }));
                Assert.That(resultB, Is.EqualTo(new int[] { 10 }));
            });
        }

        [Test]
        public void TestTimestampParse()
        {
            string[] stringTimestamps =
            {
                "3/6 19:05:42.251",
                "10/5 05:45:23.256",
                "2/23 10:02:34.568",
                "11/15 16:26:51.831"
            };
            var currentYear = DateTime.Now.Year;
            DateTime[] expectedDateTimes =
            {
                new(currentYear, 3, 6, 19, 5, 42, 251),
                new(currentYear, 10, 5, 5, 45, 23, 256),
                new(currentYear, 2, 23, 10, 2, 34, 568),
                new(currentYear, 11, 15, 16, 26, 51, 831)
            };

            DateTime[] results = new DateTime[stringTimestamps.Length];
            for (int i = 0; i < stringTimestamps.Length; i++)
                results[i] = ParsingUtil.StringTimestampToDateTime(stringTimestamps[i]);
            Assert.That(results, Is.EqualTo(expectedDateTimes));
        }

    }
}
