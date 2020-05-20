using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace VKMessenger.Framework
{
	public static class DependencyObjectExtension
	{
		//[IteratorStateMachine(typeof(DependencyObjectExtension.<Descendents>d__0))]
		public static IEnumerable<DependencyObject> Descendents(this DependencyObject root, int depth)
		{
			int childrenCount = VisualTreeHelper.GetChildrenCount(root);
			int num;
			for (int i = 0; i < childrenCount; i = num + 1)
			{
				DependencyObject dependencyObject = VisualTreeHelper.GetChild(root, i);
				yield return dependencyObject;
				if (depth > 0)
				{
					DependencyObject arg_9D_0 = dependencyObject;
					num = depth - 1;
					depth = num;
					using (IEnumerator<DependencyObject> enumerator = arg_9D_0.Descendents(num).GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							DependencyObject current = enumerator.Current;
							yield return current;
						}
					}
					//IEnumerator<DependencyObject> enumerator = null;
				}
				dependencyObject = null;
				num = i;
			}
			yield break;
		}

		public static IEnumerable<DependencyObject> Descendents(this DependencyObject root)
		{
			return root.Descendents(2147483647);
		}

		//[IteratorStateMachine(typeof(DependencyObjectExtension.<Ancestors>d__2))]
		public static IEnumerable<DependencyObject> Ancestors(this DependencyObject root)
		{
			for (DependencyObject parent = VisualTreeHelper.GetParent(root); parent != null; parent = VisualTreeHelper.GetParent(parent))
			{
				yield return parent;
			}
			yield break;
		}
	}
}
