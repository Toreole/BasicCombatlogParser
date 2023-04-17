using System.Collections;
using System.Collections.ObjectModel;

namespace CombatlogParser.Data.Events;

//IF YOURE LOOKING AT THIS CODE, IM SORRY FOR WHAT I HAVE DONE. I WILL ATONE FOR MY SINS.

/// <summary>
/// An object that holds various CombatlogEvents in sorted buckets. 
/// </summary>
// The goal with this was to minimize linear searches through EncounterInfo.CombatlogEvents by GetEvents<T> 
public class CombatlogEventDictionary
{
    private readonly Dictionary<Type, CombatlogEvent[]> dictionary = new();

    /// <summary>
    /// Gets all events of a given type in an array. If no events of a type are present, the array will be empty. <br />
    /// Do NOT modify the returned array.
    /// </summary>
    /// <typeparam name="EType">A type that inherits from CombatlogEvent</typeparam>
    public EType[] GetEvents<EType>() where EType : CombatlogEvent
    {
        bool b = dictionary.TryGetValue(typeof(EType), out CombatlogEvent[]? result);
        EType[] list = b ? (result as EType[])! : Array.Empty<EType>();
        return list;
    }
    internal void Add(Type type, CombatlogEvent[] arr) 
    {
        if (dictionary.ContainsKey(type))
            return;
        dictionary.Add(type, arr);
    }

    internal CombatlogEventDictionary()
    {

    }
}

/// <summary>
/// A helper to construct CombatlogEventDictionaries. Allows user to add one event at a time.
/// </summary>
public class CombatlogEventDictionaryBuilder
{
    private readonly Dictionary<Type, IList> dictionary = new();

    public void Add<T>(T value) where T : CombatlogEvent
    {
        GetList<T>().Add(value);
    }

    public void Add(CombatlogEvent value)
    {
        var targetType = value.GetType();
        GetList(targetType).Add(value);
    }

    private IList GetList(Type targetType)
    {
        bool exists = dictionary.ContainsKey(targetType);
        if (exists)
            return dictionary[targetType];
        var listType = typeof(List<>).MakeGenericType(targetType);
        var instantiatedList = (Activator.CreateInstance(listType) as IList)!;
        dictionary.Add(targetType, instantiatedList);
        return instantiatedList;
    }

    private List<T> GetList<T>() where T : CombatlogEvent
    {
        Type targetType = typeof(T);
        bool exists = dictionary.ContainsKey(targetType);
        if (exists)
            return (dictionary[targetType] as List<T>)!;
        var createdList = new List<T>(20);
        dictionary.Add(targetType, createdList);
        return createdList;
    }

    /// <summary>
    /// Creates the CombatlogEventDictionary and clears the internal storage so it can be re-used.
    /// </summary>
    public CombatlogEventDictionary Build()
    {
        CombatlogEventDictionary dict = new();
        Type AdvType = typeof(AdvancedParamEvent);
        foreach (var pair in dictionary)
        {
            //there is one specific special case that needs be seperated
            if (pair.Key == AdvType)
            {
                dict.Add(pair.Key, pair.Value.ToArray<CombatlogEvent, AdvancedParamEvent>());
                continue;
            }
            
            dict.Add(pair.Key, pair.Value.ToArray<CombatlogEvent>());
        }
        dictionary.Clear();
        return dict;
    }
}