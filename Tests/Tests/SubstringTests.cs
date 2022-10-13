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
    }
}
