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
    private readonly long _userOrChatId;
    private readonly bool _isChat;
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
        this.NotifyPropertyChanged<StickersStoreViewModel.CurrentSource>((System.Linq.Expressions.Expression<Func<StickersStoreViewModel.CurrentSource>>)(() => this.StickersListSource));
        this.NotifyPropertyChanged<SolidColorBrush>((System.Linq.Expressions.Expression<Func<SolidColorBrush>>)(() => this.PopularTabForeground));
        this.NotifyPropertyChanged<SolidColorBrush>((System.Linq.Expressions.Expression<Func<SolidColorBrush>>)(() => this.NewTabForeground));
        this.NotifyPropertyChanged<SolidColorBrush>((System.Linq.Expressions.Expression<Func<SolidColorBrush>>)(() => this.FreeTabForeground));
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

    public StickersStoreViewModel(long userOrChatId, bool isChat = false)
    {
      this._userOrChatId = userOrChatId;
      this._isChat = isChat;
    }

    private SolidColorBrush GetForeground(StickersStoreViewModel.CurrentSource currentSource)
    {
      if (this._stickersListSource == currentSource)
        return (SolidColorBrush) Application.Current.Resources["PhoneBlue300_GrayBlue100Brush"];
      return (SolidColorBrush) Application.Current.Resources["PhoneGray400_Gray500Brush"];
    }

    public override void Load(Action<ResultCode> callback)
    {
        StoreService.Instance.GetStickersStoreCatalog(this._isChat ? 0L : this._userOrChatId, (Action<BackendResult<StoreCatalog, ResultCode>>)(result => Execute.ExecuteOnUIThread((Action)(() =>
        {
            ResultCode resultCode = result.ResultCode;
            StoreCatalog resultData = result.ResultData;
            if ((resultCode != ResultCode.Succeeded ? 0 : (resultData != null ? 1 : 0)) != 0)
            {
                List<StoreBanner> banners = resultData.banners;
                if (banners != null)
                    this.Banners = banners.Select<StoreBanner, StoreBannerHeader>((Func<StoreBanner, StoreBannerHeader>)(banner => new StoreBannerHeader(banner))).ToList<StoreBannerHeader>();
                this.NotifyPropertyChanged<List<StoreBannerHeader>>(() => this.Banners);
                this.NotifyPropertyChanged<Visibility>(() => this.BannersVisibility);
                this.NotifyPropertyChanged<bool>(() => this.IsSlideViewCycled);
                this._stickersPacks.Clear();
                List<StoreSection> sections = resultData.sections;
                if (sections != null)
                {
                    foreach (StoreSection storeSection in sections)
                    {
                        List<StockItem> stockItemList1;
                        if (storeSection == null)
                        {
                            stockItemList1 = (List<StockItem>)null;
                        }
                        else
                        {
                            VKList<StockItem> stickers = storeSection.stickers;
                            stockItemList1 = stickers != null ? stickers.items : (List<StockItem>)null;
                        }
                        List<StockItem> stockItemList2 = stockItemList1;
                        if (stockItemList2 != null)
                        {
                            List<StockItemHeader> stockItemHeaderList = new List<StockItemHeader>();
                            VKList<StockItem> stockItems = resultData.StockItems;
                            List<StockItem> source = stockItems != null ? stockItems.items : (List<StockItem>)null;
                            if (source != null)
                            {
                                foreach (StockItem stockItem1 in stockItemList2)
                                {
                                    int num1 = 0;
                                    long userOrChatId = this._userOrChatId;
                                    int num2 = this._isChat ? 1 : 0;
                                    StockItemHeader stockItemHeader = new StockItemHeader(stockItem1, num1 != 0, userOrChatId, num2 != 0);
                                    stockItemHeaderList.Add(stockItemHeader);
                                    StoreProduct product = stockItemHeader.StockItem.product;
                                    if (product != null)
                                    {
                                        StockItem stockItem2 = source.FirstOrDefault<StockItem>((Func<StockItem, bool>)(item =>
                                        {
                                            StoreProduct product2 = item.product;
                                            int? nullable = product2 != null ? new int?(product2.id) : new int?();
                                            int id = product.id;
                                            if (nullable.GetValueOrDefault() != id)
                                                return false;
                                            return nullable.HasValue;
                                        }));
                                        if (stockItem2 != null)
                                            stockItemHeader.StockItem.CanPurchaseFor = stockItem2.CanPurchaseFor;
                                    }
                                }
                            }
                            this._stickersPacks.Add(stockItemHeaderList);
                        }
                    }
                }
                this.ReloadItems();
                StickersSettings.Instance.UpdateStickersDataAndAutoSuggest((IAccountStickersData)resultData);
                AppGlobalStateManager.Current.GlobalState.NewStoreItemsCount = 0;
                EventAggregator.Current.Publish((object)new NewStoreItemsCountChangedEvent());
            }
            callback(resultCode);
        }))));
    }

    private void ReloadItems()
    {
      int stickersListSource = (int) this._stickersListSource;
      if (stickersListSource < 0 || stickersListSource > this._stickersPacks.Count - 1)
        return;
      this.StickersList = new ObservableCollection<IVirtualizable>(StickersStoreViewModel.CreateVirtualizableItems((IEnumerable<StockItemHeader>) this._stickersPacks[stickersListSource]));
    }

    private static List<IVirtualizable> CreateVirtualizableItems(IEnumerable<StockItemHeader> stickers)
    {
        List<IVirtualizable> virtualizableList = new List<IVirtualizable>();
        foreach (StockItemHeader sticker1 in stickers)
        {
            StockItemHeader sticker = sticker1;
            UCItem ucItem = new UCItem(480.0, new Thickness(), (Func<UserControlVirtualizable>)(() =>
            {
                return (UserControlVirtualizable)new StickersPackListItemUC()
                {
                    DataContext = (object)sticker
                };
            }), (Func<double>)(() => 100.0), (Action<UserControlVirtualizable>)null, 0.0, false);
            virtualizableList.Add((IVirtualizable)ucItem);
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
