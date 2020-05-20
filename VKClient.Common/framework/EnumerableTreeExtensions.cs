using System;
using System.Collections.Generic;
using System.Windows;

namespace VKClient.Common.Framework
{
  public static class EnumerableTreeExtensions
  {
    private static IEnumerable<DependencyObject> DrillDown(this IEnumerable<DependencyObject> items, Func<DependencyObject, IEnumerable<DependencyObject>> function)
    {
        using (IEnumerator<DependencyObject> enumerator = items.GetEnumerator())
        {
            while (enumerator.MoveNext())
            {
                DependencyObject current = enumerator.Current;
                using (IEnumerator<DependencyObject> enumerator2 = function.Invoke(current).GetEnumerator())
                {
                    while (enumerator2.MoveNext())
                    {
                        DependencyObject current2 = enumerator2.Current;
                        yield return current2;
                    }
                }
                //IEnumerator<DependencyObject> enumerator2 = null;
            }
        }
        //IEnumerator<DependencyObject> enumerator = null;
        yield break;
       // yield break;
    }

    public static IEnumerable<DependencyObject> DrillDown<T>(this IEnumerable<DependencyObject> items, Func<DependencyObject, IEnumerable<DependencyObject>> function) where T : DependencyObject
    {
        using (IEnumerator<DependencyObject> enumerator = items.GetEnumerator())
        {
            while (enumerator.MoveNext())
            {
                DependencyObject current = enumerator.Current;
                using (IEnumerator<DependencyObject> enumerator2 = function.Invoke(current).GetEnumerator())
                {
                    while (enumerator2.MoveNext())
                    {
                        DependencyObject current2 = enumerator2.Current;
                        if (current2 is T)
                        {
                            yield return (T)((object)current2);
                        }
                    }
                }
                //IEnumerator<DependencyObject> enumerator2 = null;
            }
        }
        //IEnumerator<DependencyObject> enumerator = null;
        yield break;
      //  yield break;
    }

    public static IEnumerable<DependencyObject> Descendants(this IEnumerable<DependencyObject> items)
    {
      return items.DrillDown((Func<DependencyObject, IEnumerable<DependencyObject>>) (i => i.Descendants()));
    }

    public static IEnumerable<DependencyObject> DescendantsAndSelf(this IEnumerable<DependencyObject> items)
    {
      return items.DrillDown((Func<DependencyObject, IEnumerable<DependencyObject>>) (i => i.DescendantsAndSelf()));
    }

    public static IEnumerable<DependencyObject> Ancestors(this IEnumerable<DependencyObject> items)
    {
      return items.DrillDown((Func<DependencyObject, IEnumerable<DependencyObject>>) (i => i.Ancestors()));
    }

    public static IEnumerable<DependencyObject> AncestorsAndSelf(this IEnumerable<DependencyObject> items)
    {
      return items.DrillDown((Func<DependencyObject, IEnumerable<DependencyObject>>) (i => i.AncestorsAndSelf()));
    }

    public static IEnumerable<DependencyObject> Elements(this IEnumerable<DependencyObject> items)
    {
      return items.DrillDown((Func<DependencyObject, IEnumerable<DependencyObject>>) (i => i.Elements()));
    }

    public static IEnumerable<DependencyObject> ElementsAndSelf(this IEnumerable<DependencyObject> items)
    {
      return items.DrillDown((Func<DependencyObject, IEnumerable<DependencyObject>>) (i => i.ElementsAndSelf()));
    }

    public static IEnumerable<DependencyObject> Descendants<T>(this IEnumerable<DependencyObject> items) where T : DependencyObject
    {
      return items.DrillDown<T>((Func<DependencyObject, IEnumerable<DependencyObject>>) (i => i.Descendants()));
    }

    public static IEnumerable<DependencyObject> DescendantsAndSelf<T>(this IEnumerable<DependencyObject> items) where T : DependencyObject
    {
      return items.DrillDown<T>((Func<DependencyObject, IEnumerable<DependencyObject>>) (i => i.DescendantsAndSelf()));
    }

    public static IEnumerable<DependencyObject> Ancestors<T>(this IEnumerable<DependencyObject> items) where T : DependencyObject
    {
      return items.DrillDown<T>((Func<DependencyObject, IEnumerable<DependencyObject>>) (i => i.Ancestors()));
    }

    public static IEnumerable<DependencyObject> AncestorsAndSelf<T>(this IEnumerable<DependencyObject> items) where T : DependencyObject
    {
      return items.DrillDown<T>((Func<DependencyObject, IEnumerable<DependencyObject>>) (i => i.AncestorsAndSelf()));
    }

    public static IEnumerable<DependencyObject> Elements<T>(this IEnumerable<DependencyObject> items) where T : DependencyObject
    {
      return items.DrillDown<T>((Func<DependencyObject, IEnumerable<DependencyObject>>) (i => i.Elements()));
    }

    public static IEnumerable<DependencyObject> ElementsAndSelf<T>(this IEnumerable<DependencyObject> items) where T : DependencyObject
    {
      return items.DrillDown<T>((Func<DependencyObject, IEnumerable<DependencyObject>>) (i => i.ElementsAndSelf()));
    }
  }
}
