using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;

namespace VKClient.Common.Utils
{
  public static class ListExtensions
  {
    public static bool NotNullAndHasAtLeastOneNonNullElement(this IList list)
    {
      if (list != null && list.Count > 0)
        return list[0] != null;
      return false;
    }

    public static void ForEach<T>(this IEnumerable<T> source, Action<T> a)
    {
      foreach (T obj in source)
        a(obj);
    }

    public static void Shuffle<T>(this IList<T> list)
    {
      Random random = new Random();
      int count = list.Count;
      while (count > 1)
      {
        --count;
        int index = random.Next(count + 1);
        T obj = list[index];
        list[index] = list[count];
        list[count] = obj;
      }
    }

    public static IEnumerable<IEnumerable<T>> Partition<T>(this IEnumerable<T> source, int size)
    {
      T[] array = (T[]) null;
      int newSize = 0;
      foreach (T obj in source)
      {
        if (array == null)
          array = new T[size];
        array[newSize] = obj;
        ++newSize;
        if (newSize == size)
        {
          yield return (IEnumerable<T>) new ReadOnlyCollection<T>((IList<T>) array);
          array = (T[]) null;
          newSize = 0;
        }
      }
      if (array != null)
      {
        Array.Resize<T>(ref array, newSize);
        yield return (IEnumerable<T>) new ReadOnlyCollection<T>((IList<T>) array);
      }
    }

    public static void Apply<T>(this IEnumerable<T> enumerable, Action<T> action)
    {
      foreach (T obj in enumerable)
        action(obj);
    }

    public static string GetCommaSeparated(this List<string> ids, string separator = ",")
    {
      StringBuilder stringBuilder = new StringBuilder();
      int count = ids.Count;
      for (int index = 0; index < count; ++index)
      {
        stringBuilder = stringBuilder.Append(ids[index]);
        if (index != count - 1)
          stringBuilder = stringBuilder.Append(separator);
      }
      return stringBuilder.ToString();
    }

    public static string GetCommaSeparated(this List<long> ids)
    {
      StringBuilder stringBuilder = new StringBuilder();
      int count = ids.Count;
      for (int index = 0; index < count; ++index)
      {
        stringBuilder = stringBuilder.Append(ids[index].ToString((IFormatProvider) CultureInfo.InvariantCulture));
        if (index != count - 1)
          stringBuilder = stringBuilder.Append(",");
      }
      return stringBuilder.ToString();
    }

    public static string GetCommaSeparated(this List<int> ids)
    {
      StringBuilder stringBuilder = new StringBuilder();
      int count = ids.Count;
      for (int index = 0; index < count; ++index)
      {
        stringBuilder = stringBuilder.Append(ids[index].ToString((IFormatProvider) CultureInfo.InvariantCulture));
        if (index != count - 1)
          stringBuilder = stringBuilder.Append(",");
      }
      return stringBuilder.ToString();
    }

    public static void MergeWithList<T>(this List<T> list, List<T> anotherList, Func<T, T, bool> equalityFunc)
    {
    }

    public static bool IsNullOrEmpty(this IList list)
    {
      if (list != null)
        return list.Count == 0;
      return true;
    }

    public static List<T>[] Split<T>(this IEnumerable<T> list, Func<T, bool> predicate)
    {
      List<T>[] objListArray = new List<T>[2];
      List<T> objList1 = new List<T>();
      List<T> objList2 = new List<T>();
      foreach (T obj in list)
      {
        if (predicate(obj))
          objList1.Add(obj);
        else
          objList2.Add(obj);
      }
      objListArray[0] = objList1;
      objListArray[1] = objList2;
      return objListArray;
    }

    public static IEnumerable<T> TakeLast<T>(this IEnumerable<T> source, int count)
    {
      List<T> list = source.ToList<T>();
      return list.Skip<T>(Math.Max(0, list.Count - count));
    }
  }
}
