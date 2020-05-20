using System;
using System.Collections.Generic;
using System.Windows;

namespace VKClient.Common.Framework
{
  public static class EnumerableTreeExtensions
  {
    private static IEnumerable<DependencyObject> DrillDown(this IEnumerable<DependencyObject> items, Func<DependencyObject, IEnumerable<DependencyObject>> function)
    {
      foreach (DependencyObject dependencyObject1 in items)
      {
        foreach (DependencyObject dependencyObject2 in function(dependencyObject1))
          yield return dependencyObject2;
      }
    }

    public static IEnumerable<DependencyObject> DrillDown<T>(this IEnumerable<DependencyObject> items, Func<DependencyObject, IEnumerable<DependencyObject>> function) where T : DependencyObject
    {
      foreach (DependencyObject dependencyObject1 in items)
      {
        foreach (DependencyObject dependencyObject2 in function(dependencyObject1))
        {
          if (dependencyObject2 is T)
            yield return dependencyObject2;
        }
      }
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
