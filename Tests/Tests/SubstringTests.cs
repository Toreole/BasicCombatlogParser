using CombatlogParser.Data;

namespace CombatlogParser.Tests
{
    
    public class SubstringTests
    {
        [Test]
        public void SubstringShortQuotations()
        {
            string line = "\"Hello, World\"";
            int startIndex = 0;
            string sub = ParsingUtil.NextSubstring(line, ref startIndex);
            Assert.That(sub, Is.EqualTo("Hello, World"));
        }

        [Test]
        public void SubstringMultiSimple()
        {
            string line = "One,Two,Three,Four";
            int startIndex = 0;
            string[] subs = new string[4];
            for(int i = 0; i < 4; i++)
            {
                subs[i] = ParsingUtil.NextSubstring(line, ref startIndex);
            }
            Assert.Multiple(() =>
            {
                Assert.That(subs[0], Is.EqualTo("One"));
                Assert.That(subs[1], Is.EqualTo("Two"));
                Assert.That(subs[2], Is.EqualTo("Three"));
                Assert.That(subs[3], Is.EqualTo("Four"));
            });
        }

        [Test]
        public void SubstringMultiMixed()
        {
            string line = "\"One, Two\",\"Three\",Four";
            int startIndex = 0;
            string[] subs = new string[3];
            for (int i = 0; i < 3; i++)
            {
                subs[i] = ParsingUtil.NextSubstring(line, ref startIndex);
            }
            Assert.Multiple(() =>
            {
                Assert.That(subs[0], Is.EqualTo("One, Two"));
                Assert.That(subs[1], Is.EqualTo("Three"));
                Assert.That(subs[2], Is.EqualTo("Four"));
            });
        }

        [Test]
        public void ContainsSubstring()
        {
            string line = "012 HELLO YES";
            bool result1 = line.ContainsSubstringAt("HELLO", 4);
            bool result2 = line.ContainsSubstringAt("012 H", 0);
            bool result3 = line.ContainsSubstringAt("012 HELLO YES 2", 0);
            bool result4 = line.ContainsSubstringAt("012 HELLO YES", 0);
            Assert.Multiple(() =>
            {
                Assert.That( result1); //should succeed
                Assert.That( result2); //should succeed
                Assert.That(!result3); //this should fail because the other string is too long
                Assert.That( result4); //should succeed because theyre the same
            });
        }

        [Test]
        public void StartsWithF()
        {
            string line = "012 HELLO YES";
            bool result1 = line.StartsWithF("012");
            bool result2 = line.StartsWithF("12");
            bool result3 = line.StartsWithF("012 HELLO YESs");
            bool result4 = line.StartsWithF("012 HELLO YES");
            Assert.Multiple(() =>
            {
                Assert.That(result1);
                Assert.That(!result2);
                Assert.That(!result3);
                Assert.That(result4);
            });
        }

        [Test]
        public void EndsWithF()
        {
            string line = "012 HELLO YES";
            bool result1 = line.EndsWithF(" YES");
            bool result2 = line.EndsWithF("HELLO");
            bool result3 = line.EndsWithF("012 HELLO YESs");
            bool result4 = line.EndsWithF("012 HELLO YES");
            Assert.Multiple(() =>
            {
                Assert.That(result1);
                Assert.That(!result2);
                Assert.That(!result3);
                Assert.That(result4);
            });
        }
    }
}
