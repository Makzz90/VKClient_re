using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using VKClient.Audio.Base.Events;
using VKClient.Audio.Base.Extensions;
using VKClient.Common.Framework;
using VKClient.Common.Gifts.ViewModels;
using VKClient.Common.Localization;

namespace VKClient.Common.Gifts.Views
{
  public class GiftListItemUC : UserControl
  {
      public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(GiftListItemUC), new PropertyMetadata(new PropertyChangedCallback((d, e) => ((GiftListItemUC)d).UpdateTitle())));
      private const int NAME_MAX_WIDTH = 320;
    internal Grid gridHeader;
    internal TextBlock textBlockName;
    private bool _contentLoaded;

    public string Title
    {
      get
      {
        return (string) base.GetValue(GiftListItemUC.TitleProperty);
      }
      set
      {
        base.SetValue(GiftListItemUC.TitleProperty, value);
      }
    }

    public event EventHandler DeleteClicked;

    public GiftListItemUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
    }

    private void UpdateTitle()
    {
      this.textBlockName.Text = this.Title;
      double maxWidth = 320.0;
      GiftHeader dataContext = base.DataContext as GiftHeader;
      if (dataContext != null && !dataContext.IsMoreActionsVisible)
        maxWidth += 52.0;
      this.textBlockName.CorrectText(maxWidth);
    }

    private void ItemHeader_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      FrameworkElement frameworkElement = sender as FrameworkElement;
      GiftHeader giftHeader = (frameworkElement != null ? frameworkElement.DataContext : null) as GiftHeader;
      if (giftHeader == null)
        return;
      EventAggregator.Current.Publish(new GiftsPurchaseStepsEvent(GiftPurchaseStepsSource.gifts_page, GiftPurchaseStepsAction.profile));
      long fromId = giftHeader.FromId;
      string name = giftHeader.Name;
      if (fromId > 0L)
      {
        Navigator.Current.NavigateToUserProfile(fromId, name, "", false);
      }
      else
      {
        if (fromId >= 0L)
          return;
        Navigator.Current.NavigateToGroup(-fromId, name, false);
      }
    }

    private void ItemMoreActions_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      FrameworkElement frameworkElement = sender as FrameworkElement;
      GiftHeader giftHeader = (frameworkElement != null ? frameworkElement.DataContext : null) as GiftHeader;
      if (giftHeader == null)
        return;
      this.ShowMoreOptions((DependencyObject) sender, giftHeader);
    }

    private void ShowMoreOptions(DependencyObject obj, GiftHeader item)
    {
        if (obj == null)
        {
            return;
        }
        List<MenuItem> list = new List<MenuItem>();
        if (item.CanSeeGifts)
        {
            MenuItem menuItem = new MenuItem
            {
                Header = item.UsersGiftsStr
            };
            menuItem.Click += delegate(object sender, RoutedEventArgs args)
            {
                EventAggregator.Current.Publish(new GiftsPurchaseStepsEvent(GiftPurchaseStepsSource.gifts_page, GiftPurchaseStepsAction.gifts_page));
                long fromId = item.FromId;
                Navigator.Current.NavigateToGifts(fromId, item.FirstName, item.FirstNameGen);
            };
            list.Add(menuItem);
        }
        if (item.IsCurrentUser)
        {
            MenuItem menuItem2 = new MenuItem
            {
                Header = CommonResources.Delete
            };
            menuItem2.Click += delegate(object sender, RoutedEventArgs args)
            {
                EventHandler expr_06 = this.DeleteClicked;
                if (expr_06 == null)
                {
                    return;
                }
                expr_06.Invoke(this, EventArgs.Empty);
            };
            list.Add(menuItem2);
        }
        this.SetMenu(list);
        this.ShowMenu();
    }

    private void SetMenu(List<MenuItem> menuItems)
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
      ContextMenuService.SetContextMenu((DependencyObject) this.gridHeader, contextMenu2);
    }

    private void ShowMenu()
    {
      ContextMenu contextMenu = ContextMenuService.GetContextMenu((DependencyObject) this.gridHeader);
      if (contextMenu == null)
        return;
      contextMenu.IsOpen = true;
    }

    private void SendGiftBack_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      FrameworkElement frameworkElement = sender as FrameworkElement;
      GiftHeader giftHeader = (frameworkElement != null ? frameworkElement.DataContext : null) as GiftHeader;
      if (giftHeader == null)
        return;
      EventAggregator.Current.Publish(new GiftsPurchaseStepsEvent(GiftPurchaseStepsSource.gifts_page, GiftPurchaseStepsAction.reply));
      Navigator.Current.NavigateToGiftsCatalog(giftHeader.FromId, false);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/Gifts/Views/GiftListItemUC.xaml", UriKind.Relative));
      this.gridHeader = (Grid) base.FindName("gridHeader");
      this.textBlockName = (TextBlock) base.FindName("textBlockName");
    }
  }
}
