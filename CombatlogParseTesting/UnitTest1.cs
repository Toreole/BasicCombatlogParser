using CombatlogParser;
using CombatlogParser.Data;

namespace CombatlogParseTesting
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void SubeventShort()
        {
            string subevent = "SPELL_DAMAGE";
            CombatlogEventPrefix prefix;
            CombatlogEventSuffix suffix;

            if(ParsingUtil.TryParsePrefixAffixSubevent(subevent, out prefix, out suffix))
            {
                Assert.AreEqual(CombatlogEventPrefix.SPELL, prefix);
                Assert.AreEqual(CombatlogEventSuffix._DAMAGE, suffix);
            }
            else
            {
                Assert.Fail("Parse failed.");
            }
        }

        [TestMethod]
        public void SubeventLongSuffix()
        {
            string subevent = "SPELL_AURA_APPLIED_DOSE";
            CombatlogEventPrefix prefix;
            CombatlogEventSuffix suffix;

            if (ParsingUtil.TryParsePrefixAffixSubevent(subevent, out prefix, out suffix))
            {
                Assert.AreEqual(CombatlogEventPrefix.SPELL, prefix);
                Assert.AreEqual(CombatlogEventSuffix._AURA_APPLIED_DOSE, suffix);
            }
            else
            {
                Assert.Fail("Parse failed.");
            }
        }

        [TestMethod]
        public void SubeventLongPrefix()
        {
            string subevent = "SPELL_PERIODIC_DAMAGE";
            CombatlogEventPrefix prefix;
            CombatlogEventSuffix suffix;

            if (ParsingUtil.TryParsePrefixAffixSubevent(subevent, out prefix, out suffix))
            {
                Assert.AreEqual(CombatlogEventPrefix.SPELL_PERIODIC, prefix);
                Assert.AreEqual(CombatlogEventSuffix._DAMAGE, suffix);
            }
            else
            {
                Assert.Fail("Parse failed.");
            }
        }
    }
}