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
  public class GiftsCatalogCategoryItemUC : UserControlVirtualizable
  {
    private bool _contentLoaded;

    public GiftsCatalogCategoryItemUC()
    {
      this.InitializeComponent();
    }

    private void Item_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      FrameworkElement frameworkElement = sender as FrameworkElement;
      GiftsSectionItemHeader sectionItemHeader = (frameworkElement != null ? frameworkElement.DataContext : null) as GiftsSectionItemHeader;
      if (sectionItemHeader == null)
        return;
      EventAggregator.Current.Publish(new GiftsPurchaseStepsEvent(GiftPurchaseStepsSource.store, GiftPurchaseStepsAction.gift_page));
      sectionItemHeader.NavigateToGiftSend();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/Gifts/Views/GiftsCatalogCategoryItemUC.xaml", UriKind.Relative));
    }
  }
}
