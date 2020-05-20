using System.Windows;
using System.Windows.Media.Animation;

namespace VKClient.Common.ImageViewer
{
  public class AnimationInfo
  {
    public DependencyObject target { get; set; }

    public double from { get; set; }

    public double to { get; set; }

    public object propertyPath { get; set; }

    public int duration { get; set; }

    public IEasingFunction easing { get; set; }
  }
}
