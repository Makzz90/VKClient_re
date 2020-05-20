using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.UC;
using VKClient.Common.Utils;

namespace VKClient.Common.Market.Views
{
  public class MarketFeedAlbumsItemUC : UserControl
  {
      public static readonly DependencyProperty CollectionsProperty = DependencyProperty.Register("Collections", typeof(VKList<MarketAlbum>), typeof(MarketFeedAlbumsItemUC), new PropertyMetadata(new PropertyChangedCallback(MarketFeedAlbumsItemUC.Collections_OnChanged)));
    private long _ownerId;
    internal GroupHeaderUC ucGroupHeader;
    internal ItemsControl itemsControl;
    internal Grid gridFooter;
    private bool _contentLoaded;

    public VKList<MarketAlbum> Collections
    {
      get
      {
        return (VKList<MarketAlbum>) base.GetValue(MarketFeedAlbumsItemUC.CollectionsProperty);
      }
      set
      {
        base.SetValue(MarketFeedAlbumsItemUC.CollectionsProperty, value);
      }
    }

    public MarketFeedAlbumsItemUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
      ((UIElement) this.gridFooter).Visibility = Visibility.Collapsed;
    }

    private static void Collections_OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      // ISSUE: explicit reference operation
      ((MarketFeedAlbumsItemUC) d).UpdateCollections(e.NewValue as VKList<MarketAlbum>);
    }

    private void UpdateCollections(VKList<MarketAlbum> collections)
    {
      this.itemsControl.ItemsSource = ( null);
      this.ucGroupHeader.Counter = 0;
      if (collections == null)
        return;
      int num = 0;
      List<TwoInARowItemViewModel<MarketAlbum>> arowItemViewModelList = new List<TwoInARowItemViewModel<MarketAlbum>>();
      IEnumerator<IEnumerable<MarketAlbum>> enumerator = collections.items.Partition<MarketAlbum>(2).GetEnumerator();
      try
      {
        while (((IEnumerator) enumerator).MoveNext())
        {
          List<MarketAlbum> list = (List<MarketAlbum>) Enumerable.ToList<MarketAlbum>(enumerator.Current);
          TwoInARowItemViewModel<MarketAlbum> arowItemViewModel = new TwoInARowItemViewModel<MarketAlbum>() { Item1 = list[0] };
          ++num;
          if (list.Count > 1)
          {
            arowItemViewModel.Item2 = list[1];
            ++num;
          }
          arowItemViewModelList.Add(arowItemViewModel);
        }
      }
      finally
      {
        if (enumerator != null)
          ((IDisposable) enumerator).Dispose();
      }
      this.itemsControl.ItemsSource = ((IEnumerable) arowItemViewModelList);
      this.ucGroupHeader.Counter = collections.count;
      if (collections.count > num)
        ((UIElement) this.gridFooter).Visibility = Visibility.Visible;
      this._ownerId = collections.items.Count > 0 ? collections.items[0].owner_id : 0L;
    }

    private void GridFooter_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      if (this._ownerId == 0L)
        return;
      Navigator.Current.NavigateToMarketAlbums(this._ownerId);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/Market/Views/MarketFeedAlbumsItemUC.xaml", UriKind.Relative));
      this.ucGroupHeader = (GroupHeaderUC) base.FindName("ucGroupHeader");
      this.itemsControl = (ItemsControl) base.FindName("itemsControl");
      this.gridFooter = (Grid) base.FindName("gridFooter");
    }
  }
}
