using System;
using System.Windows;

namespace VKClient.Common.UC
{
  public class MenuItemExtended
  {
    public int Id { get; set; }

    public string Title { get; set; }

    public string Description { get; set; }

    public Visibility DescriptionVisibility
    {
      get
      {
        if (!string.IsNullOrEmpty(this.Description))
          return Visibility.Visible;
        return Visibility.Collapsed;
      }
    }

    public Action Action { get; set; }
  }
}
