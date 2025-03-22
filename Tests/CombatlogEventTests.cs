using CombatlogParser.Events;
using CombatlogParser.Data.WowEnums;

namespace Tests;

public class CombatlogEventTests
{
	[Test]
	public void TestDictionaryEmpty()
	{
		CombatlogEventDictionary dic = new CombatlogEventDictionaryBuilder().Build();
		DamageEvent[] events = dic.GetEvents<DamageEvent>();
		//nothing has been added so it should be empty.
		Assert.That(events, Is.EquivalentTo(Array.Empty<DamageEvent>()));
	}

	[Test]
	public void TestDictionaryOneValue()
	{
		CombatlogEventDictionaryBuilder builder = new();
		CombatlogEvent dmgEvent = CombatlogEvent.Create(
			"3/6 20:21:15.495  SWING_DAMAGE,Creature-0-3773-2522-23135-155906-0000063D1E,\"Wildtier\",0x2114,0x0,Creature-0-3773-2522-23135-190245-0000063CF4,\"Bruthüterin Diurna\",0x10a48,0x0,Creature-0-3773-2522-23135-155906-0000063D1E,Player-3391-068AB778,72992,72992,9140,9140,5935,0,1,0,0,0,-2.76,24.33,2126,2.2872,416,6174,8399,-1,1,0,0,0,nil,nil,nil\r\n",
			CombatlogEventPrefix.SWING,
			CombatlogEventSuffix._DAMAGE)!;
		builder.Add(dmgEvent);
		var dictionary = builder.Build();
		DamageEvent[] damageEvents = dictionary.GetEvents<DamageEvent>();
		Assert.That(damageEvents, Is.EquivalentTo([(DamageEvent)dmgEvent]));
	}

	[Test]
	public void TestUnitDiedEventParse()
	{
		CombatlogEvent? diedEvent = CombatlogEvent.Create("5/24 20:22:47.199  UNIT_DIED,0000000000000000,nil,0x80000000,0x80000000,Player-3391-068AB778,\"Neferu-Silvermoon\",0x514,0x0,1", CombatlogEventPrefix.UNIT, CombatlogEventSuffix._DIED);
		Assert.Multiple(() =>
		{
			Assert.That(diedEvent, Is.Not.Null);
			Assert.That(diedEvent, Is.InstanceOf<UnitDiedEvent>());
			Assert.That(diedEvent?.TargetName, Is.EqualTo("Neferu-Silvermoon"));
		});
	}
}
