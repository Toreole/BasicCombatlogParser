using CombatlogParser.Data.Events.EventData;

namespace CombatlogParser.Data.Events.Filters;

/// <summary>
/// Baseclass for all EventFilter types to filter CombatlogEvents.
/// </summary>
public abstract class EventFilter
{
    public abstract bool Match(CombatlogEvent combatlogEvent);

    public static explicit operator EventFilter(EventFilter[] filters)
    {
        return new AllOfFilter(filters);
    }
}

/// <summary>
/// Allows for specializing event filters for specific types of events.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IEventFilter<T>
{
    bool Match(T tEvent);
}
