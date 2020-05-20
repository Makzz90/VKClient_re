using System.Windows;

namespace VKClient.Common.CommonExtensions
{
  public static class BoolExtensions
  {
    public static Visibility ToVisiblity(this bool value)
    {
      return !value ? Visibility.Collapsed : Visibility.Visible;
    }
  }
}
