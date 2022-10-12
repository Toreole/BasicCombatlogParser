namespace CombatlogParser.Data
{
    /// <summary>
    /// Struct wrapper for events that have a _DAMAGE suffix
    /// </summary>
    public struct CombatlogDamageEvent
    {
        private CombatlogEvent baseEvent;

        public int Damage { get; private set; } 
        public int BaseDamage { get; private set; }
        public string SpellName => baseEvent.SubeventPrefix == CombatlogEventPrefix.SWING ? "Melee" : (string)baseEvent.PrefixParam1;

        public CombatlogDamageEvent(CombatlogEvent ev)
        {
            baseEvent = ev;
            Damage = int.Parse((string)baseEvent.SuffixParams[0]);
            BaseDamage = int.Parse((string)baseEvent.SuffixParams[1]);
        }
    }
}
