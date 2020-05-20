using Microsoft.Phone.Shell;
using System.Windows.Media;

namespace VKClient.Common.Utils
{
  public static class ApplicationBarBuilder
  {
    public static ApplicationBar Build(Color? backgroundColor = null, Color? foregroundColor = null, double opacity = 0.9)
    {
      ApplicationBar applicationBar = new ApplicationBar();
      Color? nullable = backgroundColor;
      Color color1 = nullable ?? VKConstants.AppBarBGColor;
      applicationBar.BackgroundColor = color1;
      nullable = foregroundColor;
      Color color2 = nullable ?? VKConstants.AppBarFGColor;
      applicationBar.ForegroundColor = color2;
      double num = opacity;
      applicationBar.Opacity = num;
      return applicationBar;
    }
  }
}
