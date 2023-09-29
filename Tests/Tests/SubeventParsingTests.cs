using CombatlogParser.Data.Events;

namespace CombatlogParser.Tests
{
    public class SubeventParsingTests
    {
        [Test]
        public void SubeventShort()
        {
            string subevent = "SPELL_DAMAGE";

            if (ParsingUtil.TryParsePrefixAffixSubeventF(subevent, out CombatlogEventPrefix prefix, out CombatlogEventSuffix suffix))
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

            if (ParsingUtil.TryParsePrefixAffixSubeventF(subevent, out CombatlogEventPrefix prefix, out CombatlogEventSuffix suffix))
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

            if (ParsingUtil.TryParsePrefixAffixSubeventF(subevent, out CombatlogEventPrefix prefix, out CombatlogEventSuffix suffix))
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

        [Test]
        public void SubeventLong()
        {
            string subevent = "SPELL_PERIODIC_AURA_APPLIED";

            if (ParsingUtil.TryParsePrefixAffixSubeventF(subevent, out CombatlogEventPrefix prefix, out CombatlogEventSuffix suffix))
            {
                Assert.Multiple(() =>
                {
                    Assert.That(prefix, Is.EqualTo(CombatlogEventPrefix.SPELL_PERIODIC));
                    Assert.That(suffix, Is.EqualTo(CombatlogEventSuffix._AURA_APPLIED));
                });
            }
            else
            {
                Assert.Fail("Parse failed.");
            }
        }

        [Test]
        public void SubeventNonCombat()
        {
            bool failed = !ParsingUtil.TryParsePrefixAffixSubeventF("ZONE_CHANGED", out _, out _);
            Assert.That(failed);
        }

        [Test]
        public void UnitDiedSubeventRecognized()
        {
            bool success = ParsingUtil.TryParsePrefixAffixSubeventF("UNIT_DIED", out var prefix, out var suffix);
            Assert.Multiple(() =>
            {
                Assert.That(success);
                Assert.That(prefix, Is.EqualTo(CombatlogEventPrefix.UNIT));
                Assert.That(suffix, Is.EqualTo(CombatlogEventSuffix._DIED));
            });
        }
    }
}