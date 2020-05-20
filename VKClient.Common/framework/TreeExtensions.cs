using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace VKClient.Common.Framework
{
    public static class TreeExtensions
    {
        public static IEnumerable<DependencyObject> Descendants(this DependencyObject item)
        {
            foreach (DependencyObject child1 in new VisualTreeAdapter(item).Children())
            {
                DependencyObject child = child1;
                yield return child;
                foreach (DependencyObject descendant in child.Descendants())
                    yield return descendant;
                child = (DependencyObject)null;
            }
        }

        public static IEnumerable<DependencyObject> DescendantsAndSelf(this DependencyObject item)
        {
            yield return item;
            foreach (DependencyObject descendant in item.Descendants())
                yield return descendant;
        }

        public static IEnumerable<DependencyObject> Ancestors(this DependencyObject item)
        {
            for (DependencyObject parent = new VisualTreeAdapter(item).Parent; parent != null; parent = new VisualTreeAdapter(parent).Parent)
                yield return parent;
        }

        public static IEnumerable<DependencyObject> AncestorsAndSelf(this DependencyObject item)
        {
            yield return item;
            foreach (DependencyObject ancestor in item.Ancestors())
                yield return ancestor;
        }

        public static IEnumerable<DependencyObject> Elements(this DependencyObject item)
        {
            foreach (DependencyObject child in new VisualTreeAdapter(item).Children())
                yield return child;
        }

        public static IEnumerable<DependencyObject> ElementsBeforeSelf(this DependencyObject item)
        {
            if (item.Ancestors().FirstOrDefault<DependencyObject>() != null)
            {
                foreach (DependencyObject element in item.Ancestors().First<DependencyObject>().Elements())
                {
                    if (!element.Equals((object)item))
                        yield return element;
                    else
                        break;
                }
            }
        }

        public static IEnumerable<DependencyObject> ElementsAfterSelf(this DependencyObject item)
        {
            if (item.Ancestors().FirstOrDefault<DependencyObject>() != null)
            {
                bool afterSelf = false;
                foreach (DependencyObject element in item.Ancestors().First<DependencyObject>().Elements())
                {
                    DependencyObject child = element;
                    if (afterSelf)
                        yield return child;
                    if (child.Equals((object)item))
                        afterSelf = true;
                    child = (DependencyObject)null;
                }
            }
        }

        public static IEnumerable<DependencyObject> ElementsAndSelf(this DependencyObject item)
        {
            yield return item;
            foreach (DependencyObject element in item.Elements())
                yield return element;
        }

        public static IEnumerable<DependencyObject> Descendants<T>(this DependencyObject item)
        {
            return item.Descendants().Where<DependencyObject>((Func<DependencyObject, bool>)(i => i is T)).Cast<DependencyObject>();
        }

        public static IEnumerable<DependencyObject> ElementsBeforeSelf<T>(this DependencyObject item)
        {
            return item.ElementsBeforeSelf().Where<DependencyObject>((Func<DependencyObject, bool>)(i => i is T)).Cast<DependencyObject>();
        }

        public static IEnumerable<DependencyObject> ElementsAfterSelf<T>(this DependencyObject item)
        {
            return item.ElementsAfterSelf().Where<DependencyObject>((Func<DependencyObject, bool>)(i => i is T)).Cast<DependencyObject>();
        }

        public static IEnumerable<DependencyObject> DescendantsAndSelf<T>(this DependencyObject item)
        {
            return item.DescendantsAndSelf().Where<DependencyObject>((Func<DependencyObject, bool>)(i => i is T)).Cast<DependencyObject>();
        }

        public static IEnumerable<DependencyObject> Ancestors<T>(this DependencyObject item)
        {
            return item.Ancestors().Where<DependencyObject>((Func<DependencyObject, bool>)(i => i is T)).Cast<DependencyObject>();
        }

        public static IEnumerable<DependencyObject> AncestorsAndSelf<T>(this DependencyObject item)
        {
            return item.AncestorsAndSelf().Where<DependencyObject>((Func<DependencyObject, bool>)(i => i is T)).Cast<DependencyObject>();
        }

        public static IEnumerable<DependencyObject> Elements<T>(this DependencyObject item)
        {
            return item.Elements().Where<DependencyObject>((Func<DependencyObject, bool>)(i => i is T)).Cast<DependencyObject>();
        }

        public static IEnumerable<DependencyObject> ElementsAndSelf<T>(this DependencyObject item)
        {
            return item.ElementsAndSelf().Where<DependencyObject>((Func<DependencyObject, bool>)(i => i is T)).Cast<DependencyObject>();
        }
    }
}
