using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using VKClient.Audio.Base.BackendServices;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.CommonExtensions;
using VKClient.Common.Emoji;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Library.VirtItems;
using VKClient.Common.Stickers.Views;

namespace VKClient.Common.Stickers.ViewModels
{
  public class StickersStoreViewModel : ViewModelStatefulBase
  {
    private readonly List<List<StockItemHeader>> _stickersPacks = new List<List<StockItemHeader>>();
    private ObservableCollection<IVirtualizable> _stickersList = new ObservableCollection<IVirtualizable>();
    private StickersStoreViewModel.CurrentSource _stickersListSource;

    public List<StoreBannerHeader> Banners { get; set; }

    public Visibility BannersVisibility
    {
      get
      {
        return (this.Banners != null && this.Banners.Count > 0).ToVisiblity();
      }
    }

    public bool IsSlideViewCycled
    {
      get
      {
        if (this.Banners != null)
          return this.Banners.Count > 1;
        return false;
      }
    }

    public ObservableCollection<IVirtualizable> StickersList
    {
      get
      {
        return this._stickersList;
      }
      private set
      {
        this._stickersList = value;
        this.NotifyPropertyChanged("StickersList");
      }
    }

    public StickersStoreViewModel.CurrentSource StickersListSource
    {
      get
      {
        return this._stickersListSource;
      }
      set
      {
        if (this._stickersListSource == value)
          return;
        this._stickersListSource = value;
        this.NotifyPropertyChanged<StickersStoreViewModel.CurrentSource>((System.Linq.Expressions.Expression<Func<StickersStoreViewModel.CurrentSource>>) (() => this.StickersListSource));
        this.NotifyPropertyChanged<SolidColorBrush>((System.Linq.Expressions.Expression<Func<SolidColorBrush>>) (() => this.PopularTabForeground));
        this.NotifyPropertyChanged<SolidColorBrush>((System.Linq.Expressions.Expression<Func<SolidColorBrush>>) (() => this.NewTabForeground));
        this.NotifyPropertyChanged<SolidColorBrush>((System.Linq.Expressions.Expression<Func<SolidColorBrush>>) (() => this.FreeTabForeground));
        this.ReloadItems();
      }
    }

    public SolidColorBrush PopularTabForeground
    {
      get
      {
        return this.GetForeground(StickersStoreViewModel.CurrentSource.Popular);
      }
    }

    public SolidColorBrush NewTabForeground
    {
      get
      {
        return this.GetForeground(StickersStoreViewModel.CurrentSource.New);
      }
    }

    public SolidColorBrush FreeTabForeground
    {
      get
      {
        return this.GetForeground(StickersStoreViewModel.CurrentSource.Free);
      }
    }

    private SolidColorBrush GetForeground(StickersStoreViewModel.CurrentSource currentSource)
    {
      if (this._stickersListSource == currentSource)
        return (SolidColorBrush) Application.Current.Resources["PhoneBlue300_GrayBlue100Brush"];
      return (SolidColorBrush) Application.Current.Resources["PhoneGray400_Gray500Brush"];
    }

    public override void Load(Action<bool> callback)
    {
      StoreService.Instance.GetStickersStoreCatalog((Action<BackendResult<StoreCatalog, ResultCode>>) (result => Execute.ExecuteOnUIThread((Action) (() =>
      {
        StoreCatalog resultData = result.ResultData;
        bool flag = result.ResultCode == ResultCode.Succeeded && resultData != null;
        if (flag)
        {
          List<StoreBanner> banners = resultData.banners;
          if (banners != null)
            this.Banners = banners.Select<StoreBanner, StoreBannerHeader>((Func<StoreBanner, StoreBannerHeader>) (banner => new StoreBannerHeader(banner))).ToList<StoreBannerHeader>();
          this.NotifyPropertyChanged<List<StoreBannerHeader>>((System.Linq.Expressions.Expression<Func<List<StoreBannerHeader>>>) (() => this.Banners));
          this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.BannersVisibility));
          this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>) (() => this.IsSlideViewCycled));
          this._stickersPacks.Clear();
          List<StoreSection> sections = resultData.sections;
          if (sections != null)
          {
            foreach (StoreSection storeSection in sections)
            {
              List<StockItem> stockItemList;
              if (storeSection == null)
              {
                stockItemList = (List<StockItem>) null;
              }
              else
              {
                VKList<StockItem> stickers = storeSection.stickers;
                stockItemList = stickers != null ? stickers.items : (List<StockItem>) null;
              }
              List<StockItem> source = stockItemList;
              if (source != null)
                this._stickersPacks.Add(source.Select<StockItem, StockItemHeader>((Func<StockItem, StockItemHeader>) (stockItem => new StockItemHeader(stockItem, false))).ToList<StockItemHeader>());
            }
          }
          this.ReloadItems();
          StickersSettings.Instance.UpdateStickersDataAndAutoSuggest((IAccountStickersData) result.ResultData);
          AppGlobalStateManager.Current.GlobalState.NewStoreItemsCount = 0;
          EventAggregator.Current.Publish((object) new NewStoreItemsCountChangedEvent());
        }
        callback(flag);
      }))));
    }

    private void ReloadItems()
    {
      int index = (int) this._stickersListSource;
      if (index < 0 || index > this._stickersPacks.Count - 1)
        return;
      this.StickersList = new ObservableCollection<IVirtualizable>(StickersStoreViewModel.CreateVirtualizableItems((IEnumerable<StockItemHeader>) this._stickersPacks[index]));
    }

    private static List<IVirtualizable> CreateVirtualizableItems(IEnumerable<StockItemHeader> stickers)
    {
      List<IVirtualizable> virtualizableList = new List<IVirtualizable>();
      foreach (StockItemHeader sticker1 in stickers)
      {
        StockItemHeader sticker = sticker1;
        UCItem ucItem = new UCItem(480.0, new Thickness(), (Func<UserControlVirtualizable>) (() =>
        {
          return (UserControlVirtualizable) new StickersPackListItemUC()
          {
            DataContext = (object) sticker
          };
        }), (Func<double>) (() => 100.0), (Action<UserControlVirtualizable>) null, 0.0, false);
        virtualizableList.Add((IVirtualizable) ucItem);
      }
      return virtualizableList;
    }

    public enum CurrentSource
    {
      New,
      Popular,
      Free,
    }
  }
}
