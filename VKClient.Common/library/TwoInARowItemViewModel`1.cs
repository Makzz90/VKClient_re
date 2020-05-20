using System.Windows;

namespace VKClient.Common.Library
{
  public class TwoInARowItemViewModel<T>
  {
    public T Item1 { get; set; }

    public T Item2 { get; set; }

    public Visibility Item2Visibility
    {
      get
      {
        if (this.Item2 == null)
          return Visibility.Collapsed;
        return Visibility.Visible;
      }
    }
  }
}
