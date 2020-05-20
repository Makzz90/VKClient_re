using System;
using System.Windows;
using System.Windows.Media;

namespace VKClient.Common.Framework
{
  public interface IScrollableArea
  {
    Action OnCompressionTop { get; set; }

    Action OnCompressionBottom { get; set; }

    Projection Projection { get; set; }

    Action<double> OnVerticalOffsetChanged { get; set; }

    double VerticalOffset { get; }

    Action<bool, bool> OnScrollStateChanged { get; set; }

    double ViewportHeight { get; }

    double ExtentHeight { get; }

    bool IsEnabled { get; set; }

    Rect Bounds { get; set; }

    bool IsManipulating { get; }

    void ScrollToTopOrBottom(bool toBottom = true, Action onCompletedCallback = null);

    void ScrollToVerticalOffset(double offset);

    void UpdateLayout();
  }
}
