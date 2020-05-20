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
        return (object) this.Item2 == null ? Visibility.Collapsed : Visibility.Visible;
      }
    }
  }
}
