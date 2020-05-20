using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Audio.Base.Library;
using VKClient.Common.CommonExtensions;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Stickers.ViewModels;

namespace VKClient.Common.Emoji
{
  public class SpriteListItemData : ViewModelBase, ISupportBackspaceListItem, IHandle<NewStoreItemsCountChangedEvent>, IHandle
  {
    private bool _isSelected;

    public bool IsEmoji { get; set; }

    public bool IsRecentStickers { get; set; }

    public bool IsStore { get; set; }

    public bool IsSettings { get; set; }

    public StoreProduct StickerProduct
    {
      get
      {
        return this.StickerStockItemHeader.Product ?? new StoreProduct();
      }
    }

    public StockItemHeader StickerStockItemHeader { get; set; }

    public int Counter
    {
      get
      {
        if (!this.IsStore)
          return 0;
        return Math.Min(AppGlobalStateManager.Current.GlobalState.NewStoreItemsCount, 99);
      }
    }

    public Visibility CounterVisibility
    {
      get
      {
        return (this.Counter > 0).ToVisiblity();
      }
    }

    public string TabThumb
    {
      get
      {
        if (this.IsEmoji)
          return "/Resources/Smile32px.png";
        if (this.IsRecentStickers)
          return "/Resources/Recent32px.png";
        if (this.IsStore)
          return "/Resources/Shop32px.png";
        return this.IsSettings ? "/Resources/Settings32px.png" : "";
      }
    }

    public string TabThumbSticker
    {
      get
      {
        if (this.StickerProduct == null)
          return "";
        return string.Format("{0}thumb_102.png", (object) this.StickerProduct.base_url);
      }
    }

    public Visibility TabIconVisibility
    {
      get
      {
        return (!this.IsStickersPack).ToVisiblity();
      }
    }

    public Visibility TabImageVisibility
    {
      get
      {
        return (this.TabIconVisibility == Visibility.Collapsed).ToVisiblity();
      }
    }

    public double TabImageOpacity
    {
      get
      {
        return this.StickerProduct.purchased != 0 ? 1.0 : 0.4;
      }
    }

    public double ImageDim
    {
      get
      {
        return !this.IsStickersPack ? 32.0 : 40.0;
      }
    }

    public bool IsSelected
    {
      get
      {
        return this._isSelected;
      }
      set
      {
        this._isSelected = value;
        this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>) (() => this.IsSelected));
        this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.TabThumb));
        this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.TabThumbSticker));
        this.NotifyPropertyChanged<SolidColorBrush>((System.Linq.Expressions.Expression<Func<SolidColorBrush>>) (() => this.TabBackground));
      }
    }

    public SolidColorBrush TabBackground
    {
      get
      {
        if (!this.IsSelected)
          return new SolidColorBrush(Colors.Transparent);
        return Application.Current.Resources["PhoneGray100_Gray700Brush"] as SolidColorBrush;
      }
    }

    public bool IsStickersPack
    {
      get
      {
        if (!this.IsEmoji && !this.IsStore && !this.IsSettings)
          return !this.IsRecentStickers;
        return false;
      }
    }

    public Visibility StickersPackVisibility
    {
      get
      {
        return this.IsStickersPack.ToVisiblity();
      }
    }

    public Visibility NoStickersVisibility
    {
      get
      {
        int num;
        if (this.IsStickersPack || this.IsRecentStickers)
        {
          StoreStickers stickers = this.StickerProduct.stickers;
          if ((stickers != null ? stickers.sticker_ids : (List<int>) null) != null)
          {
            List<int> stickerIds = this.StickerProduct.stickers.sticker_ids;
            num = stickerIds != null ? ( (stickerIds.Count) == 0 ? 1 : 0) : 0;
          }
          else
            num = 1;
        }
        else
          num = 0;
        return (num != 0).ToVisiblity();
      }
    }

    public bool IsBackspaceVisible
    {
      get
      {
        return this.IsEmoji;
      }
    }

    public SpriteListItemData()
    {
      this.StickerStockItemHeader = new StockItemHeader(new StockItem(), false);
      EventAggregator.Current.Subscribe((object) this);
    }

    public void ProcessSystemTab()
    {
      if (this.IsStore)
      {
        CurrentStickersPurchaseFunnelSource.Source = StickersPurchaseFunnelSource.keyboard;
        EventAggregator.Current.Publish((object) new StickersPurchaseFunnelEvent(StickersPurchaseFunnelAction.store));
        Navigator.Current.NavigateToStickersStore();
      }
      else
      {
        if (!this.IsSettings)
          return;
        Navigator.Current.NavigateToStickersManage();
      }
    }

    private void UpdateStoreCounter()
    {
      if (!this.IsStore)
        return;
      Execute.ExecuteOnUIThread((Action) (() =>
      {
        this.NotifyPropertyChanged<int>((System.Linq.Expressions.Expression<Func<int>>) (() => this.Counter));
        this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.CounterVisibility));
      }));
    }

    public void Handle(NewStoreItemsCountChangedEvent message)
    {
      this.UpdateStoreCounter();
    }
  }
}
