using System.Windows;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Library.VirtItems;

namespace VKClient.Common.UC
{
  public abstract class NewsLinkUCBase : UserControlVirtualizable
  {
    public abstract void Initialize(Link link, double width, string parentPostId = "");

    public abstract double CalculateTotalHeight();

    protected double GetElementTotalHeight(FrameworkElement element)
    {
      Thickness margin = element.Margin;
      // ISSUE: explicit reference operation
      double num = ((Thickness) @margin).Top + element.Height;
      margin = element.Margin;
      // ISSUE: explicit reference operation
      double bottom = ((Thickness) @margin).Bottom;
      return num + bottom;
    }
  }
}
