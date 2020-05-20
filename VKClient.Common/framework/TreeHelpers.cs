using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace VKClient.Common.Framework
{
  public static class TreeHelpers
	{
		//[IteratorStateMachine(typeof(TreeHelpers.<GetVisualAncestors>d__0))]
		public static IEnumerable<FrameworkElement> GetVisualAncestors(this FrameworkElement node)
		{
			for (FrameworkElement visualParent = node.GetVisualParent(); visualParent != null; visualParent = visualParent.GetVisualParent())
			{
				yield return visualParent;
			}
			yield break;
		}

		public static FrameworkElement GetVisualParent(this FrameworkElement node)
		{
			return VisualTreeHelper.GetParent(node) as FrameworkElement;
		}
	}
}
