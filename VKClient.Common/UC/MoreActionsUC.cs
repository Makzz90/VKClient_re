using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using VKClient.Common.Framework;
using VKClient.Common.Library.VirtItems;

namespace VKClient.Common.UC
{
  public class MoreActionsUC : UserControlVirtualizable
  {
    internal Grid LayoutRoot;
    internal Border border;
    private bool _contentLoaded;

    public Action TapCallback { get; set; }

    public MoreActionsUC()
    {
      this.InitializeComponent();
    }

    public void SetBlue()
    {
      ((Panel) this.LayoutRoot).Background = ((Brush) Application.Current.Resources["PhoneHeaderBackgroundBrush"]);
      this.border.Background = ((Brush) Application.Current.Resources["PhoneBlueOnBlueIconBrush"]);
    }

    private void OptionsButtonTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      if (this.TapCallback != null)
      {
        this.TapCallback();
        e.Handled = true;
      }
      else
      {
        ISupportMenuActions dataContext = base.DataContext as ISupportMenuActions;
        if (dataContext == null)
          return;
        List<MenuItemData> menuItemsData = dataContext.GetMenuItemsData();
        List<MenuItem> menuItems = new List<MenuItem>();
        if (menuItemsData != null)
        {
            using (List<MenuItemData>.Enumerator enumerator = menuItemsData.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    MenuItemData m = enumerator.Current;
                    MenuItem menuItem = new MenuItem
                    {
                        Header = m.Title
                    };
                    menuItem.Click += delegate(object s, RoutedEventArgs ev)
                    {
                        m.OnTap.Invoke();
                    };
                    menuItems.Add(menuItem);
                }
            }
            if (menuItems.Count > 0)
            {
                this.SetMenu(menuItems);
                this.ShowMenu();
            }
        }
        e.Handled = true;
      }
    }

    public void SetMenu(List<MenuItem> menuItems)
    {
      if (menuItems == null || menuItems.Count == 0)
        return;
      ContextMenu contextMenu1 = new ContextMenu();
      SolidColorBrush solidColorBrush1 = (SolidColorBrush) Application.Current.Resources["PhoneMenuBackgroundBrush"];
      ((Control) contextMenu1).Background = ((Brush) solidColorBrush1);
      SolidColorBrush solidColorBrush2 = (SolidColorBrush) Application.Current.Resources["PhoneMenuForegroundBrush"];
      ((Control) contextMenu1).Foreground = ((Brush) solidColorBrush2);
      int num = 0;
      contextMenu1.IsZoomEnabled = num != 0;
      ContextMenu contextMenu2 = contextMenu1;
      foreach (MenuItem menuItem in menuItems)
        ((PresentationFrameworkCollection<object>) contextMenu2.Items).Add(menuItem);
      ContextMenuService.SetContextMenu((DependencyObject) this, contextMenu2);
    }

    public void ShowMenu()
    {
      ContextMenu contextMenu = ContextMenuService.GetContextMenu((DependencyObject) this);
      if (contextMenu == null)
        return;
      contextMenu.IsOpen = true;
    }

    private void LayoutRoot_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      e.Handled = true;
    }

    private void LayoutRoot_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
    {
      e.Handled = true;
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/MoreActionsUC.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) base.FindName("LayoutRoot");
      this.border = (Border) base.FindName("border");
    }
  }
}
