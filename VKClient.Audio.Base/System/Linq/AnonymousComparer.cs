using System.Collections.Generic;

namespace System.Linq
{
  public static class AnonymousComparer
  {
    public static IComparer<T> Create<T>(Func<T, T, int> compare)
    {
      if (compare == null)
        throw new ArgumentNullException("compare");
      return (IComparer<T>) new AnonymousComparer.Comparer<T>(compare);
    }

    public static IEqualityComparer<T> Create<T, TKey>(Func<T, TKey> compareKeySelector)
    {
      if (compareKeySelector == null)
        throw new ArgumentNullException("compareKeySelector");
      return (IEqualityComparer<T>) new AnonymousComparer.EqualityComparer<T>((Func<T, T, bool>) ((x, y) =>
      {
        if ((object) x == (object) y)
          return true;
        if ((object) x == null || (object) y == null)
          return false;
        return compareKeySelector(x).Equals((object) compareKeySelector(y));
      }), (Func<T, int>) (obj =>
      {
        if ((object) obj == null)
          return 0;
        return compareKeySelector(obj).GetHashCode();
      }));
    }

    public static IEqualityComparer<T> Create<T>(Func<T, T, bool> equals, Func<T, int> getHashCode)
    {
      if (equals == null)
        throw new ArgumentNullException("equals");
      if (getHashCode == null)
        throw new ArgumentNullException("getHashCode");
      return (IEqualityComparer<T>) new AnonymousComparer.EqualityComparer<T>(equals, getHashCode);
    }

    public static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TKey, TKey, int> compare)
    {
      return source.OrderBy<TSource, TKey>(keySelector, AnonymousComparer.Create<TKey>(compare));
    }

    public static IOrderedEnumerable<TSource> OrderByDescending<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TKey, TKey, int> compare)
    {
      return source.OrderByDescending<TSource, TKey>(keySelector, AnonymousComparer.Create<TKey>(compare));
    }

    public static IOrderedEnumerable<TSource> ThenBy<TSource, TKey>(this IOrderedEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TKey, TKey, int> compare)
    {
      return source.ThenBy<TSource, TKey>(keySelector, AnonymousComparer.Create<TKey>(compare));
    }

    public static IOrderedEnumerable<TSource> ThenByDescending<TSource, TKey>(this IOrderedEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TKey, TKey, int> compare)
    {
      return source.ThenByDescending<TSource, TKey>(keySelector, AnonymousComparer.Create<TKey>(compare));
    }

    public static bool Contains<TSource, TCompareKey>(this IEnumerable<TSource> source, TSource value, Func<TSource, TCompareKey> compareKeySelector)
    {
      return source.Contains<TSource>(value, AnonymousComparer.Create<TSource, TCompareKey>(compareKeySelector));
    }

    public static IEnumerable<TSource> Distinct<TSource, TCompareKey>(this IEnumerable<TSource> source, Func<TSource, TCompareKey> compareKeySelector)
    {
      return source.Distinct<TSource>(AnonymousComparer.Create<TSource, TCompareKey>(compareKeySelector));
    }

    public static IEnumerable<TSource> Except<TSource, TCompareKey>(this IEnumerable<TSource> first, IEnumerable<TSource> second, Func<TSource, TCompareKey> compareKeySelector)
    {
      return first.Except<TSource>(second, AnonymousComparer.Create<TSource, TCompareKey>(compareKeySelector));
    }

    public static IEnumerable<TResult> GroupBy<TSource, TKey, TResult, TCompareKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TKey, IEnumerable<TSource>, TResult> resultSelector, Func<TKey, TCompareKey> compareKeySelector)
    {
      return source.GroupBy<TSource, TKey, TResult>(keySelector, resultSelector, AnonymousComparer.Create<TKey, TCompareKey>(compareKeySelector));
    }

    public static IEnumerable<IGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement, TCompareKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<TKey, TCompareKey> compareKeySelector)
    {
      return source.GroupBy<TSource, TKey, TElement>(keySelector, elementSelector, AnonymousComparer.Create<TKey, TCompareKey>(compareKeySelector));
    }

    public static IEnumerable<TResult> GroupBy<TSource, TKey, TElement, TResult, TCompareKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<TKey, IEnumerable<TElement>, TResult> resultSelector, Func<TKey, TCompareKey> compareKeySelector)
    {
      return source.GroupBy<TSource, TKey, TElement, TResult>(keySelector, elementSelector, resultSelector, AnonymousComparer.Create<TKey, TCompareKey>(compareKeySelector));
    }

    public static IEnumerable<TResult> GroupJoin<TOuter, TInner, TKey, TResult, TCompareKey>(this IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, IEnumerable<TInner>, TResult> resultSelector, Func<TKey, TCompareKey> compareKeySelector)
    {
      return outer.GroupJoin<TOuter, TInner, TKey, TResult>(inner, outerKeySelector, innerKeySelector, resultSelector, AnonymousComparer.Create<TKey, TCompareKey>(compareKeySelector));
    }

    public static IEnumerable<TSource> Intersect<TSource, TCompareKey>(this IEnumerable<TSource> first, IEnumerable<TSource> second, Func<TSource, TCompareKey> compareKeySelector)
    {
      return first.Intersect<TSource>(second, AnonymousComparer.Create<TSource, TCompareKey>(compareKeySelector));
    }

    public static IEnumerable<TResult> Join<TOuter, TInner, TKey, TResult, TCompareKey>(this IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, TInner, TResult> resultSelector, Func<TKey, TCompareKey> compareKeySelector)
    {
      return outer.Join<TOuter, TInner, TKey, TResult>(inner, outerKeySelector, innerKeySelector, resultSelector, AnonymousComparer.Create<TKey, TCompareKey>(compareKeySelector));
    }

    public static bool SequenceEqual<TSource, TCompareKey>(this IEnumerable<TSource> first, IEnumerable<TSource> second, Func<TSource, TCompareKey> compareKeySelector)
    {
      return first.SequenceEqual<TSource>(second, AnonymousComparer.Create<TSource, TCompareKey>(compareKeySelector));
    }

    public static Dictionary<TKey, TElement> ToDictionary<TSource, TKey, TElement, TCompareKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<TKey, TCompareKey> compareKeySelector)
    {
      return source.ToDictionary<TSource, TKey, TElement>(keySelector, elementSelector, AnonymousComparer.Create<TKey, TCompareKey>(compareKeySelector));
    }

    public static ILookup<TKey, TElement> ToLookup<TSource, TKey, TElement, TCompareKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<TKey, TCompareKey> compareKeySelector)
    {
      return source.ToLookup<TSource, TKey, TElement>(keySelector, elementSelector, AnonymousComparer.Create<TKey, TCompareKey>(compareKeySelector));
    }

    public static IEnumerable<TSource> Union<TSource, TCompareKey>(this IEnumerable<TSource> first, IEnumerable<TSource> second, Func<TSource, TCompareKey> compareKeySelector)
    {
      return first.Union<TSource>(second, AnonymousComparer.Create<TSource, TCompareKey>(compareKeySelector));
    }

    private class Comparer<T> : IComparer<T>
    {
      private readonly Func<T, T, int> compare;

      public Comparer(Func<T, T, int> compare)
      {
        this.compare = compare;
      }

      public int Compare(T x, T y)
      {
        return this.compare(x, y);
      }
    }

    private class EqualityComparer<T> : IEqualityComparer<T>
    {
      private readonly Func<T, T, bool> equals;
      private readonly Func<T, int> getHashCode;

      public EqualityComparer(Func<T, T, bool> equals, Func<T, int> getHashCode)
      {
        this.equals = equals;
        this.getHashCode = getHashCode;
      }

      public bool Equals(T x, T y)
      {
        return this.equals(x, y);
      }

      public int GetHashCode(T obj)
      {
        return this.getHashCode(obj);
      }
    }
  }
}
