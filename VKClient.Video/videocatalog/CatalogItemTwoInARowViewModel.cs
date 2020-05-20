using System.Windows;
using VKClient.Common.Framework;
using VKClient.Common.VideoCatalog;

namespace VKClient.Video.VideoCatalog
{
  public class CatalogItemTwoInARowViewModel : ViewModelBase
  {
    public CatalogItemViewModel Item1 { get; set; }

    public CatalogItemViewModel Item2 { get; set; }

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
