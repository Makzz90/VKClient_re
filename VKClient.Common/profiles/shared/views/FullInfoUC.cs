using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
      //base.\u002Ector();
      this.InitializeComponent();
      this.ucHeader.HideSandwitchButton = true;
      this.ucHeader.OnHeaderTap += (Action) (() => this.scrollViewer.ScrollToTopWithAnimation());
    }

    private void InfoItem_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      ProfileInfoItem dataContext = ((FrameworkElement) sender).DataContext as ProfileInfoItem;
      if (dataContext == null || dataContext.NavigationAction == null)
        return;
      e.Handled = true;
      dataContext.NavigationAction();
    }

    private void InfoItem_OnHold(object sender, System.Windows.Input.GestureEventArgs e)
    {
      FrameworkElement element = (FrameworkElement) sender;
      IProfileInfoSupportCopyToClipboard dataContext = element.DataContext as IProfileInfoSupportCopyToClipboard;
      if (dataContext == null)
        return;
      string data = dataContext.GetData();
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
        menuItem1.Header = (object)commentItemCopy;
        MenuItem menuItem2 = menuItem1;
        menuItem2.Click += (RoutedEventHandler)((o, args) => Clipboard.SetText(textToCopy));
        List<MenuItem> menuItemList = new List<MenuItem>()
      {
        menuItem2
      };
        FullInfoUC.SetMenu((DependencyObject)element, (IReadOnlyCollection<MenuItem>)menuItemList);
    }

    private static void SetMenu(DependencyObject element, IReadOnlyCollection<MenuItem> menuItems)
    {
      if (menuItems.Count <= 0)
        return;
      ContextMenu contextMenu1 = new ContextMenu();
      SolidColorBrush solidColorBrush1 = (SolidColorBrush) Application.Current.Resources["PhoneMenuBackgroundBrush"];
      ((Control) contextMenu1).Background = ((Brush) solidColorBrush1);
      SolidColorBrush solidColorBrush2 = (SolidColorBrush) Application.Current.Resources["PhoneMenuForegroundBrush"];
      ((Control) contextMenu1).Foreground = ((Brush) solidColorBrush2);
      int num = 0;
      contextMenu1.IsZoomEnabled = num != 0;
      ContextMenu contextMenu2 = contextMenu1;
      foreach (MenuItem menuItem in (IEnumerable<MenuItem>) menuItems)
        ((PresentationFrameworkCollection<object>) contextMenu2.Items).Add(menuItem);
      ContextMenuService.SetContextMenu(element, contextMenu2);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/Profiles/Shared/Views/FullInfoUC.xaml", UriKind.Relative));
      this.ucHeader = (GenericHeaderUC) base.FindName("ucHeader");
      this.scrollViewer = (ScrollViewer) base.FindName("scrollViewer");
    }
  }
}
