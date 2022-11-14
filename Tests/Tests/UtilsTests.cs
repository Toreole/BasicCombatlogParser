using CombatlogParser.Data;

namespace CombatlogParser.Tests
{
    public class UtilsTests
    {
        [Test]
        public void MillisToReadableString()
        {
            string resultA = ParsingUtil.MillisecondsToReadableTimeString(60_000);
            string resultB = ParsingUtil.MillisecondsToReadableTimeString(119_432);
            string resultC = ParsingUtil.MillisecondsToReadableTimeString(0);
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
            string resultA = ParsingUtil.MillisecondsToReadableTimeString(60_000, true);
            string resultB = ParsingUtil.MillisecondsToReadableTimeString(119_432, true);
            string resultC = ParsingUtil.MillisecondsToReadableTimeString(0, true);
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
    }
}
