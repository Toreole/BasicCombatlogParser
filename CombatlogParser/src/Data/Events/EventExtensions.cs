namespace CombatlogParser.Data.Events
{
    public static class EventExtensions
    {
        public static TEvent[] GetEvents<TEvent>(this ICollection<CombatlogEvent> source) where TEvent : CombatlogEvent
        {
            TEvent[] items = new TEvent[source.Count(x => x is TEvent)];
            int i = 0;
            foreach (CombatlogEvent e in source)
                if (e is TEvent et)
                    items[i++] = et;
            return items;
        }

        public static TEvent[] AllThatMatch<TEvent>(this ICollection<TEvent> source, IEventFilter filter) where TEvent: CombatlogEvent
        {
            TEvent[] items = new TEvent[source.Count(x => filter.Match(x))];
            int i = 0;
            foreach (TEvent e in source)
                if (filter.Match(e))
                    items[i++] = e;
            return items;
        }
    }
}
