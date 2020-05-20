using Microsoft.Phone.Controls;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace VKClient.Common.Utils
{
  public class ContextMenuHelper
  {
    public static ContextMenu CreateMenu(List<MenuItem> menuItems)
    {
      ContextMenu contextMenu = new ContextMenu();
      if (menuItems.Count > 0)
      {
        contextMenu.Background = (Brush) Application.Current.Resources["PhoneMenuBackgroundBrush"];
        contextMenu.Foreground = (Brush) Application.Current.Resources["PhoneMenuForegroundBrush"];
        contextMenu.IsZoomEnabled = false;
        foreach (MenuItem menuItem in menuItems)
          contextMenu.Items.Add((object) menuItem);
      }
      return contextMenu;
    }
  }
}
