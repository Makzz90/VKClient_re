using System.Windows;

namespace VKClient.Common.CommonExtensions
{
  public static class BoolExtensions
  {
    public static Visibility ToVisiblity(this bool value)
    {
      if (!value)
        return Visibility.Collapsed;
      return Visibility.Visible;
    }
  }
}
