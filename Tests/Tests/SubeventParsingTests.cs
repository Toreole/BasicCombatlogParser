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

        [Test]
        public void SubeventLong()
        {
            string subevent = "SPELL_PERIODIC_AURA_APPLIED";

            if (ParsingUtil.TryParsePrefixAffixSubevent(subevent, out CombatlogEventPrefix prefix, out CombatlogEventSuffix suffix))
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
            string subevent = "ZONE_CHANGED";

            if(ParsingUtil.TryParsePrefixAffixSubevent(subevent, out CombatlogEventPrefix _, out CombatlogEventSuffix _))
                Assert.Fail("Allowed invalid combatlog event");
            else
                Assert.Pass();
        }

        [Test]
        public void FastSubeventPrefSufParse()
        {
            string[] subevents = { "SPELL_PERIODIC_AURA_APPLIED", "SPELL_DAMAGE" };
            
            CombatlogEventPrefix[] prefixes = new CombatlogEventPrefix[2];
            CombatlogEventSuffix[] suffixes = new CombatlogEventSuffix[2];
            for (int i = 0; i < subevents.Length; i++)
                _ = ParsingUtil.TryParsePrefixAffixSubeventF(subevents[i], out prefixes[i], out suffixes[i]);

            Assert.Multiple(() =>
            {
                Assert.That(prefixes[0], Is.EqualTo(CombatlogEventPrefix.SPELL_PERIODIC));
                Assert.That(prefixes[1], Is.EqualTo(CombatlogEventPrefix.SPELL));

                Assert.That(suffixes[0], Is.EqualTo(CombatlogEventSuffix._AURA_APPLIED));
                Assert.That(suffixes[1], Is.EqualTo(CombatlogEventSuffix._DAMAGE));

                //Asserts for attempts not needed, as the return is implicit when checking the out parameters.
            });
        }
    }
}