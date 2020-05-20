using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VKClient.Common.Localization;
using VKClient.Common.Profiles.Shared.ViewModels;
using VKClient.Common.UC;

using VKClient.Common.Utils;

namespace VKClient.Common.Profiles.Shared.Views
{
  public class FullInfoUC : UserControl
  {
    internal GenericHeaderUC ucHeader;
    internal ScrollViewer scrollViewer;
    private bool _contentLoaded;

    public FullInfoUC()
    {
      this.InitializeComponent();
      this.ucHeader.HideSandwitchButton = true;
      this.ucHeader.OnHeaderTap += (Action) (() => this.scrollViewer.ScrollToTopWithAnimation());
    }

    private void InfoItem_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      ProfileInfoItem profileInfoItem = ((FrameworkElement) sender).DataContext as ProfileInfoItem;
      if (profileInfoItem == null || profileInfoItem.NavigationAction == null)
        return;
      e.Handled = true;
      profileInfoItem.NavigationAction();
    }

    private void InfoItem_OnHold(object sender, System.Windows.Input.GestureEventArgs e)
    {
      FrameworkElement element = (FrameworkElement) sender;
      IProfileInfoSupportCopyToClipboard supportCopyToClipboard = element.DataContext as IProfileInfoSupportCopyToClipboard;
      if (supportCopyToClipboard == null)
        return;
      string data = supportCopyToClipboard.GetData();
      if (string.IsNullOrEmpty(data))
        return;
      FullInfoUC.OpenCopyContextMenu(element, data);
    }

    private static void OpenCopyContextMenu(FrameworkElement element, string textToCopy)
    {
      if (string.IsNullOrEmpty(textToCopy))
        return;
      MenuItem menuItem1 = new MenuItem();
      string commentItemCopy = CommonResources.CommentItem_Copy;
      menuItem1.Header = (object) commentItemCopy;
      MenuItem menuItem2 = menuItem1;
      menuItem2.Click += (RoutedEventHandler) ((o, args) => Clipboard.SetText(textToCopy));
      List<MenuItem> menuItemList = new List<MenuItem>()
      {
        menuItem2
      };
      FullInfoUC.SetMenu((DependencyObject) element, (IReadOnlyCollection<MenuItem>) menuItemList);
    }

    private static void SetMenu(DependencyObject element, IReadOnlyCollection<MenuItem> menuItems)
    {
      if (menuItems.Count <= 0)
        return;
      ContextMenu contextMenu1 = new ContextMenu();
      SolidColorBrush solidColorBrush1 = (SolidColorBrush) Application.Current.Resources["PhoneMenuBackgroundBrush"];
      contextMenu1.Background = (Brush) solidColorBrush1;
      SolidColorBrush solidColorBrush2 = (SolidColorBrush) Application.Current.Resources["PhoneMenuForegroundBrush"];
      contextMenu1.Foreground = (Brush) solidColorBrush2;
      int num = 0;
      contextMenu1.IsZoomEnabled = num != 0;
      ContextMenu contextMenu2 = contextMenu1;
      foreach (MenuItem menuItem in (IEnumerable<MenuItem>) menuItems)
        contextMenu2.Items.Add((object) menuItem);
      ContextMenuService.SetContextMenu(element, contextMenu2);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/Profiles/Shared/Views/FullInfoUC.xaml", UriKind.Relative));
      this.ucHeader = (GenericHeaderUC) this.FindName("ucHeader");
      this.scrollViewer = (ScrollViewer) this.FindName("scrollViewer");
    }
  }
}
