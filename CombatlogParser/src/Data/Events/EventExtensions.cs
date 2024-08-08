using System.Collections;

namespace CombatlogParser.Data.Events;

public static class EventExtensions
{
    public static T[] ToArray<T>(this IList list) where T : class
    {
        int size = list.Count;
        if (size == 0 || list[0] is T == false)
            return Array.Empty<T>();

        T[] array = (T[])Array.CreateInstance(list[0]!.GetType(), size);

        for (int i = 0; i < size; i++)
        {
            if (list[i] is T typedObject)
                array[i] = typedObject;
        }
        return array;
    }

    public static T[] ToArray<T, ExplicitType>(this IList list) where T : class where ExplicitType : class, T
    {
        int size = list.Count;
        if (size == 0 || list[0] is T == false)
            return Array.Empty<T>();

        T[] array = new ExplicitType[size];

        for (int i = 0; i < size; i++)
        {
			if (list[i] is T typedObject)
				array[i] = typedObject;
		}
        return array;
    }
}
