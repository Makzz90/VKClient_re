using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using VKClient.Audio.Base.Events;
using VKClient.Common.Framework;
using VKClient.Common.Gifts.ViewModels;
using VKClient.Common.Library.VirtItems;

namespace VKClient.Common.Gifts.Views
{
  public class GiftsCatalogCategoryUC : UserControlVirtualizable
  {
    public const int FIXED_HEIGHT = 289;
    private bool _contentLoaded;

    public GiftsCatalogCategoryUC()
    {
      this.InitializeComponent();
    }

    private void ListBox_ManipulationStarted(object sender, ManipulationStartedEventArgs e)
    {
      e.Handled = true;
    }

    private void ListBox_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
    {
      e.Handled = true;
    }

    private void ListBox_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
    {
      e.Handled = true;
    }

    private void Header_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      FrameworkElement frameworkElement = sender as FrameworkElement;
      GiftsCatalogCategoryViewModel categoryViewModel = (frameworkElement != null ? frameworkElement.DataContext : null) as GiftsCatalogCategoryViewModel;
      if (categoryViewModel == null)
        return;
      EventAggregator.Current.Publish(new GiftsPurchaseStepsEvent(GiftPurchaseStepsSource.store, GiftPurchaseStepsAction.catalog_page));
      Navigator.Current.NavigateToGiftsCatalogCategory(categoryViewModel.CategoryName, categoryViewModel.Title, categoryViewModel.UserOrChatId, categoryViewModel.IsChat);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/Gifts/Views/GiftsCatalogCategoryUC.xaml", UriKind.Relative));
    }
  }
}
