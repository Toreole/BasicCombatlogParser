using CombatlogParser.Data;

namespace CombatlogParser.Tests
{
    public class SubeventParsingTests
    {
        [Test]
        public void SubeventShort()
        {
            string subevent = "SPELL_DAMAGE";

            if (ParsingUtil.TryParsePrefixAffixSubevent(subevent, out CombatlogEventPrefix prefix, out CombatlogEventSuffix suffix))
            {
                Assert.Multiple(() =>
                {
                    Assert.That(prefix, Is.EqualTo(CombatlogEventPrefix.SPELL));
                    Assert.That(suffix, Is.EqualTo(CombatlogEventSuffix._DAMAGE));
                });
            }
            else
            {
                Assert.Fail("Parse failed.");
            }
        }

        [Test]
        public void SubeventLongSuffix()
        {
            string subevent = "SPELL_AURA_APPLIED_DOSE";

            if (ParsingUtil.TryParsePrefixAffixSubevent(subevent, out CombatlogEventPrefix prefix, out CombatlogEventSuffix suffix))
            {
                Assert.Multiple(() =>
                {
                    Assert.That(prefix, Is.EqualTo(CombatlogEventPrefix.SPELL));
                    Assert.That(suffix, Is.EqualTo(CombatlogEventSuffix._AURA_APPLIED_DOSE));
                });
            }
            else
            {
                Assert.Fail("Parse failed.");
            }
        }

        [Test]
        public void SubeventLongPrefix()
        {
            string subevent = "SPELL_PERIODIC_DAMAGE";

            if (ParsingUtil.TryParsePrefixAffixSubevent(subevent, out CombatlogEventPrefix prefix, out CombatlogEventSuffix suffix))
            {
                Assert.Multiple(() =>
                {
                    Assert.That(prefix, Is.EqualTo(CombatlogEventPrefix.SPELL_PERIODIC));
                    Assert.That(suffix, Is.EqualTo(CombatlogEventSuffix._DAMAGE));
                });
            }
            else
            {
                Assert.Fail("Parse failed.");
            }
        }
    }
}