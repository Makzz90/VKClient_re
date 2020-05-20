using System.Windows.Media;

namespace VKClient.Common.UC
{
  public sealed class SelectionPageItem
  {
    public string Title { get; set; }

    public SolidColorBrush Foreground { get; set; }

    public CustomListPickerItem Source { get; set; }
  }
}
