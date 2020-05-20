using Microsoft.Phone.Controls;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
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
        ((Control) contextMenu).Background = ((Brush) Application.Current.Resources["PhoneMenuBackgroundBrush"]);
        ((Control) contextMenu).Foreground = ((Brush) Application.Current.Resources["PhoneMenuForegroundBrush"]);
        contextMenu.IsZoomEnabled = false;
        foreach (MenuItem menuItem in menuItems)
          ((PresentationFrameworkCollection<object>) contextMenu.Items).Add(menuItem);
      }
      return contextMenu;
    }
  }
}
