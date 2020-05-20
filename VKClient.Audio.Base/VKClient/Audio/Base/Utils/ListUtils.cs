using System;
using System.Collections.Generic;
using System.Linq;

namespace VKClient.Audio.Base.Utils
{
  public static class ListUtils
  {
    public static void GetListChanges<T>(List<T> originalList, List<T> updatedList, Func<T, string> getKey, Func<T, T, bool> comparer, out List<Tuple<T, T>> updatedItems, out List<T> newItems, out List<T> deletedItems)
    {
      updatedItems = new List<Tuple<T, T>>();
      newItems = new List<T>();
      deletedItems = new List<T>();
      ILookup<string, T> lookup1 = originalList.ToLookup<T, string>(getKey);
      ILookup<string, T> lookup2 = updatedList.ToLookup<T, string>(getKey);
      foreach (T updated in updatedList)
      {
        IEnumerable<T> source = lookup1[getKey(updated)];
        if (source.Any<T>())
        {
          T obj = source.First<T>();
          if (!comparer(updated, obj))
            updatedItems.Add(new Tuple<T, T>(obj, updated));
        }
        else
          newItems.Add(updated);
      }
      foreach (T original in originalList)
      {
        if (!lookup2[getKey(original)].Any<T>())
          deletedItems.Add(original);
      }
    }

    public static bool ListContainsAllOfAnother<T>(IEnumerable<T> parentList, IEnumerable<T> childList)
    {
      return childList.Intersect<T>(parentList).Count<T>() == childList.Count<T>();
    }
  }
}
