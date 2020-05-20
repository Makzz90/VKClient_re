using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace VKClient.Common.Framework.CodeForFun
{
	public static class VisualTreeExtensions
	{
		//[IteratorStateMachine(typeof(VisualTreeExtensions.<GetVisualChildren>d__0))]
		public static IEnumerable<DependencyObject> GetVisualChildren(this DependencyObject parent)
		{
			int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
			int num;
			for (int i = 0; i < childrenCount; i = num + 1)
			{
				yield return VisualTreeHelper.GetChild(parent, i);
				num = i;
			}
			yield break;
		}

		//[IteratorStateMachine(typeof(VisualTreeExtensions.<GetLogicalChildrenBreadthFirst>d__1))]
		public static IEnumerable<FrameworkElement> GetLogicalChildrenBreadthFirst(this FrameworkElement parent)
		{
			Queue<FrameworkElement> queue = new Queue<FrameworkElement>(Enumerable.OfType<FrameworkElement>(parent.GetVisualChildren()));
			while (queue.Count > 0)
			{
				FrameworkElement frameworkElement = queue.Dequeue();
				yield return frameworkElement;
				using (IEnumerator<FrameworkElement> enumerator = Enumerable.OfType<FrameworkElement>(frameworkElement.GetVisualChildren()).GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						FrameworkElement current = enumerator.Current;
						queue.Enqueue(current);
					}
				}
				frameworkElement = null;
			}
			yield break;
		}
	}
}
