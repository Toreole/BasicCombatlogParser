using System.Collections;

namespace CombatlogParser.Data.Events;

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

    public static IAdvancedParamEvent[] GetAdvancedParamEvents(this ICollection<CombatlogEvent> source) 
    {
        var items = new IAdvancedParamEvent[source.Count(x => x is IAdvancedParamEvent)];
        int i = 0;
        foreach (var e in source)
            if (e is IAdvancedParamEvent a)
                items[i++] = a;
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

    public static TEvent? GetFirstEvent<TEvent>(this ICollection<CombatlogEvent> sourceCollection) where TEvent : class
    {
        foreach (var ev in sourceCollection)
            if (ev is TEvent tev)
                return tev;
        return null;
    }

    public static TEvent? GetLastEvent<TEvent>(this ICollection<CombatlogEvent> sourceCollection) where TEvent : class
    {
        foreach (var ev in sourceCollection.Reverse())
            if (ev is TEvent tev)
                return tev;
        return null;
    }

    public static T[] ToArray<T>(this IList list) where T : class
    {
        int size = list.Count;
        if (size == 0 || list[0] is T == false)
            return Array.Empty<T>();

        T[] array = (T[])Array.CreateInstance(list[0]!.GetType(), size);

        for (int i = 0; i < size; i++)
        {
            array[i] = list[i] as T;
        }
        return array;
    }
}
