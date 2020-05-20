using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.CommonExtensions;
using VKClient.Common.Framework;
using VKClient.Common.Gifts.ViewModels;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.Stickers.ViewModels
{
  public class StockItemHeader : ViewModelBase, IHandle<StickersPackPurchasedEvent>, IHandle, IHandle<StickersPackActivatedDeactivatedEvent>, IHandle<GiftSentEvent>
  {
    private readonly ThemeHelper _themeHelper = new ThemeHelper();
    private const int IMAGE_WIDTH = 88;
    private readonly StockItem _stockItem;

    public long UserOrChatId { get; private set; }

    public bool IsChat { get; private set; }

    public bool IsRecentStickersPack { get; private set; }

    public StoreProduct Product
    {
      get
      {
        return this._stockItem.product ?? new StoreProduct();
      }
    }

    public int ProductId
    {
      get
      {
        return this.Product.id;
      }
    }

    public StockItem StockItem
    {
      get
      {
        return this._stockItem;
      }
    }

    public string Author
    {
      get
      {
        return this._stockItem.author;
      }
    }

    public string Title
    {
      get
      {
        return this.Product.title;
      }
    }

    public int Price
    {
      get
      {
        return this._stockItem.price;
      }
    }

    public string PriceStr
    {
      get
      {
        return this._stockItem.price_str ?? CommonResources.Unavailable;
      }
    }

    public string ImageUrl
    {
      get
      {
        double num = (double) (88 * ScaleFactor.GetRealScaleFactor()) / 100.0;
        if (num <= 0.0)
          return "";
        if (num <= 35.0)
          return this._stockItem.photo_35;
        if (num <= 70.0)
          return this._stockItem.photo_70;
        return this._stockItem.photo_140;
      }
    }

    public string Description
    {
      get
      {
        return this._stockItem.description;
      }
    }

    public string DemoPhotosBackground
    {
      get
      {
        return this._stockItem.background;
      }
    }

    public string DemoPhotosBackgroundThemed
    {
      get
      {
        if (this._themeHelper.PhoneDarkThemeVisibility != Visibility.Visible)
          return this.DemoPhotosBackground;
        return "";
      }
    }

    public List<string> DemoPhotos
    {
      get
      {
        return this._stockItem.demo_photos_560;
      }
    }

    public bool IsDemoPhotosSlideViewCycled
    {
      get
      {
        if (this._stockItem.demo_photos_560 != null)
          return this._stockItem.demo_photos_560.Count > 1;
        return false;
      }
    }

    public Visibility NavDotsVisibility
    {
      get
      {
        return (this._stockItem.demo_photos_560 != null && this._stockItem.demo_photos_560.Count > 1).ToVisiblity();
      }
    }

    public Visibility NewVisibility
    {
      get
      {
        return (this._stockItem.@new == 1).ToVisiblity();
      }
    }

    public bool IsPurchased
    {
      get
      {
        return this.Product.purchased == 1;
      }
      set
      {
        this.Product.purchased = value ? 1 : 0;
        this.NotifyProperties();
      }
    }

    public bool CanPurchaseFor
    {
      get
      {
        return this._stockItem.CanPurchaseFor;
      }
      set
      {
        this._stockItem.CanPurchaseFor = value;
      }
    }

    public bool IsActive
    {
      get
      {
        return this.Product.active == 1;
      }
      set
      {
        this.Product.active = value ? 1 : 0;
        this.NotifyProperties();
      }
    }

    public Visibility PurchaseVisibility
    {
      get
      {
        return (!this.IsPurchased && !this.IsRecentStickersPack).ToVisiblity();
      }
    }

    public Visibility PurchasedVisibility
    {
      get
      {
        return (this.IsPurchased && this.IsActive).ToVisiblity();
      }
    }

    public Visibility ActivateVisibility
    {
      get
      {
        return (this.IsPurchased && !this.IsActive).ToVisiblity();
      }
    }

    public StockItemHeader(StockItem stockItem, bool isRecentStickersPack = false, long userOrChatId = 0, bool isChat = false)
    {
      this._stockItem = stockItem;
      this.UserOrChatId = userOrChatId;
      this.IsChat = isChat;
      this.IsRecentStickersPack = isRecentStickersPack;
      EventAggregator.Current.Subscribe(this);
    }

    public void SetPurchasedState()
    {
      this.IsPurchased = true;
      this.IsActive = true;
    }

    private void NotifyProperties()
    {
      Execute.ExecuteOnUIThread((Action) (() =>
      {
        // ISSUE: method reference
        this.NotifyPropertyChanged<Visibility>(() => this.PurchaseVisibility);
        // ISSUE: method reference
        this.NotifyPropertyChanged<Visibility>(() => this.PurchasedVisibility);
        // ISSUE: method reference
        this.NotifyPropertyChanged<Visibility>(() => this.ActivateVisibility);
      }));
    }

    public void Handle(StickersPackPurchasedEvent message)
    {
      if (message.StockItemHeader.ProductId != this.ProductId)
        return;
      this.SetPurchasedState();
    }

    public void Handle(StickersPackActivatedDeactivatedEvent message)
    {
      if (message.StockItemHeader.ProductId != this.ProductId)
        return;
      this.IsActive = message.IsActive;
    }

    public void Handle(GiftSentEvent message)
    {
      if ((long) this.ProductId != -message.GiftId)
        return;
      List<long> userIds = message.UserIds;
      if (this.IsChat || userIds == null || !userIds.Contains(this.UserOrChatId))
        return;
      this.CanPurchaseFor = false;
    }
  }
}
