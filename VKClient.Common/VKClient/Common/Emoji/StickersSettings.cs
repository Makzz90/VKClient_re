using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using VKClient.Audio.Base.BackendServices;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Audio.Base.Library;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Library.Events;
using VKClient.Common.Stickers.AutoSuggest;
using VKClient.Common.Stickers.ViewModels;
using VKClient.Common.Stickers.Views;

namespace VKClient.Common.Emoji
{
  public class StickersSettings : IHandle<StickersPackPurchasedEvent>, IHandle, IHandle<StickersUpdatedEvent>, IHandle<StickersPackActivatedDeactivatedEvent>, IHandle<StickersPacksReorderedEvent>, IHandle<StickersTapEvent>, IHandle<StickerItemTapEvent>
  {
    private static StickersSettings _instance;

    public static StickersSettings Instance
    {
      get
      {
        return StickersSettings._instance ?? (StickersSettings._instance = new StickersSettings());
      }
    }

    public List<StockItem> StickersList
    {
      get
      {
        return AppGlobalStateManager.Current.GlobalState.StickersStockItems;
      }
      set
      {
        AppGlobalStateManager.Current.GlobalState.StickersStockItems = value;
        AppGlobalStateManager.Current.GlobalState.NeedRefetchStickers = false;
        EventAggregator.Current.Publish((object) new StickersSettings.StickersListUpdatedEvent());
      }
    }

    public StoreStickers RecentStickers
    {
      get
      {
        return AppGlobalStateManager.Current.GlobalState.RecentStickers;
      }
      set
      {
        AppGlobalStateManager.Current.GlobalState.RecentStickers = value;
      }
    }

    public int NewStoreItemsCount
    {
      get
      {
        return AppGlobalStateManager.Current.GlobalState.NewStoreItemsCount;
      }
      set
      {
        AppGlobalStateManager.Current.GlobalState.NewStoreItemsCount = value;
        EventAggregator.Current.Publish((object) new NewStoreItemsCountChangedEvent());
      }
    }

    public bool HasStickersUpdates
    {
      get
      {
        return AppGlobalStateManager.Current.GlobalState.HasStickersUpdates;
      }
      set
      {
        AppGlobalStateManager.Current.GlobalState.HasStickersUpdates = value;
        EventAggregator.Current.Publish((object) new HasStickersUpdatesChangedEvent());
      }
    }

    private StickersSettings()
    {
      EventAggregator.Current.Subscribe((object) this);
    }

    public List<SpriteListItemData> CreateSpriteListItemData()
    {
      List<SpriteListItemData> spriteListItemDataList1 = new List<SpriteListItemData>()
      {
        new SpriteListItemData()
        {
          IsEmoji = true
        }
      };
      if (this.StickersList == null)
        return spriteListItemDataList1;
      List<SpriteListItemData> spriteListItemDataList2 = spriteListItemDataList1;
      SpriteListItemData spriteListItemData = new SpriteListItemData();
      spriteListItemData.IsRecentStickers = true;
      StockItem stockItem1 = new StockItem();
      stockItem1.product = new StoreProduct()
      {
        stickers = this.RecentStickers
      };
      int num = 0;
      StockItemHeader stockItemHeader = new StockItemHeader(stockItem1, num != 0);
      spriteListItemData.StickerStockItemHeader = stockItemHeader;
      spriteListItemDataList2.Add(spriteListItemData);
      spriteListItemDataList1.AddRange(this.StickersList.Select<StockItem, SpriteListItemData>((Func<StockItem, SpriteListItemData>) (stockItem => new SpriteListItemData()
      {
        StickerStockItemHeader = new StockItemHeader(stockItem, false)
      })));
      return spriteListItemDataList1;
    }

    private void InsertRecentSticker(int stickerId)
    {
      StoreStickers recentStickers = this.RecentStickers;
      List<int> source = recentStickers != null ? recentStickers.sticker_ids : (List<int>) null;
      if (source == null)
        return;
      if (source.Contains(stickerId))
        source.Remove(stickerId);
      source.Insert(0, stickerId);
      if (source.Count > 32)
        source = source.Take<int>(32).ToList<int>();
      this.RecentStickers.sticker_ids = source;
      EventAggregator.Current.Publish((object) new StickersSettings.StickerRecentItemInsertedEvent());
    }

    public List<SpriteListItemData> CreateStoreSpriteListItem()
    {
      return new List<SpriteListItemData>()
      {
        new SpriteListItemData()
        {
          IsStore = true
        }
      };
    }

    public List<SpriteListItemData> CreateSettingsSpriteListItem()
    {
      return new List<SpriteListItemData>()
      {
        new SpriteListItemData()
        {
          IsSettings = true
        }
      };
    }

    public void Reset()
    {
      this.StickersList = (List<StockItem>) null;
      AppGlobalStateManager.Current.GlobalState.NeedRefetchStickers = true;
    }

    private void RemoveExistingStickers(List<StockItem> stockItems, StockItem stockItem)
    {
      int productId = stockItem.product.id;
      StockItem stockItem1 = stockItems.FirstOrDefault<StockItem>((Func<StockItem, bool>) (item => item.product.id == productId));
      if (stockItem1 == null)
        return;
      stockItems.Remove(stockItem1);
    }

    private void RemoveStickers(StockItem stockItem)
    {
      List<StockItem> stockItems = new List<StockItem>((IEnumerable<StockItem>) this.StickersList);
      this.RemoveExistingStickers(stockItems, stockItem);
      this.StickersList = stockItems;
    }

    private void AddStickers(StockItem stockItem)
    {
      List<StockItem> stockItemList = new List<StockItem>((IEnumerable<StockItem>) this.StickersList);
      this.RemoveExistingStickers(stockItemList, stockItem);
      int num = stockItemList.Count<StockItem>((Func<StockItem, bool>) (item => item.product.purchased == 0));
      stockItemList.Insert(stockItemList.Count - num, stockItem);
      this.StickersList = stockItemList;
    }

    private void InsertStickers(int index, StockItem stockItem)
    {
      if (this.StickersList == null || index < 0 || index > this.StickersList.Count)
        return;
      List<StockItem> stockItems = new List<StockItem>((IEnumerable<StockItem>) this.StickersList);
      this.RemoveExistingStickers(stockItems, stockItem);
      stockItems.Insert(index, stockItem);
      this.StickersList = stockItems;
    }

    public void UpdateStickersDataAndAutoSuggest(IAccountStickersData stickersData)
    {
      if (stickersData == null)
        return;
      StickersSettings.PreprocessStickersData(stickersData);
      List<StockItem> stickersList = this.StickersList;
      VKList<StockItem> stockItems = stickersData.StockItems;
      List<StockItem> stockItemList = stockItems != null ? stockItems.items : (List<StockItem>) null;
      int num = StickersSettings.GetStickersAreUpToDate((IReadOnlyList<StockItem>) stickersList, (IReadOnlyList<StockItem>) stockItemList) ? 1 : 0;
      this.UpdateStickersData(stickersData);
      if (num == 0)
        return;
      StickersAutoSuggestDictionary.Instance.EnsureDictIsLoadedAndUpToDate(true);
    }

    private static void PreprocessStickersData(IAccountStickersData stickersData)
    {
      VKList<StoreProduct> vkList1 = new VKList<StoreProduct>();
      VKList<StockItem> vkList2 = new VKList<StockItem>();
      VKList<StoreProduct> products = stickersData.Products;
      if ((products != null ? products.items : (List<StoreProduct>) null) != null)
      {
        VKList<StockItem> stockItems = stickersData.StockItems;
        if ((stockItems != null ? stockItems.items : (List<StockItem>) null) != null)
        {
          foreach (StoreProduct storeProduct in stickersData.Products.items)
          {
            StoreProduct product = storeProduct;
            StockItem stockItem = stickersData.StockItems.items.FirstOrDefault<StockItem>((Func<StockItem, bool>) (i => i.product.id == product.id));
            if (stockItem != null && product != null && (product.promoted != 1 || product.purchased != 1 || product.active != 0))
            {
              product.description = stockItem.description;
              product.author = stockItem.author;
              product.photo_140 = stockItem.photo_140;
              vkList1.items.Add(product);
              vkList2.items.Add(stockItem);
            }
          }
          vkList1.count = vkList1.items.Count;
          vkList2.count = vkList2.items.Count;
          stickersData.Products = vkList1;
          stickersData.StockItems = vkList2;
        }
      }
      StoreStickers recentStickers = stickersData.RecentStickers;
      if ((recentStickers != null ? recentStickers.sticker_ids : (List<int>) null) == null)
        return;
      stickersData.RecentStickers.sticker_ids = stickersData.RecentStickers.sticker_ids.Take<int>(32).ToList<int>();
    }

    private static bool GetStickersAreUpToDate(IReadOnlyList<StockItem> stockItems1, IReadOnlyList<StockItem> stockItems2)
    {
      if (stockItems1 == null || stockItems2 == null)
        return false;
      if (stockItems1.Count != stockItems2.Count)
        return true;
      for (int index = 0; index < stockItems1.Count; ++index)
      {
        StockItem stockItem1 = stockItems1[index];
        int? nullable1;
        if (stockItem1 == null)
        {
          nullable1 = new int?();
        }
        else
        {
          StoreProduct product = stockItem1.product;
          nullable1 = product != null ? new int?(product.id) : new int?();
        }
        int? nullable2 = nullable1;
        StockItem stockItem2 = stockItems2[index];
        int? nullable3;
        if (stockItem2 == null)
        {
          nullable3 = new int?();
        }
        else
        {
          StoreProduct product = stockItem2.product;
          nullable3 = product != null ? new int?(product.id) : new int?();
        }
        int? nullable4 = nullable3;
        if ((nullable2.GetValueOrDefault() == nullable4.GetValueOrDefault() ? (nullable2.HasValue != nullable4.HasValue ? 1 : 0) : 1) != 0)
          return true;
      }
      return false;
    }

    private void UpdateStickersData(IAccountStickersData stickersData)
    {
      if (stickersData.StockItems != null)
        this.StickersList = stickersData.StockItems.items;
      if (stickersData.RecentStickers != null)
        this.RecentStickers = stickersData.RecentStickers;
      int? newStoreItemsCount = stickersData.NewStoreItemsCount;
      if (newStoreItemsCount.HasValue)
      {
        newStoreItemsCount = stickersData.NewStoreItemsCount;
        this.NewStoreItemsCount = newStoreItemsCount.Value;
      }
      bool? hasStickersUpdates = stickersData.HasStickersUpdates;
      if (!hasStickersUpdates.HasValue)
        return;
      hasStickersUpdates = stickersData.HasStickersUpdates;
      this.HasStickersUpdates = hasStickersUpdates.Value;
    }

    public void Handle(StickersPackPurchasedEvent message)
    {
      StockItemHeader stockItemHeader = message.StockItemHeader;
      this.InsertStickers(0, stockItemHeader.StockItem);
      EventAggregator.Current.Publish((object) new StickersSettings.StickersKeyboardOpenRequestEvent(stockItemHeader));
      if (!message.IsGift)
        return;
      this.HasStickersUpdates = true;
    }

    public void Handle(StickersUpdatedEvent message)
    {
      this.InsertStickers(0, message.StockItemHeader.StockItem);
    }

    public void Handle(StickersPackActivatedDeactivatedEvent message)
    {
      if (message.IsActive)
      {
        this.AddStickers(message.StockItemHeader.StockItem);
        EventAggregator.Current.Publish((object) new StickersSettings.StickersKeyboardOpenRequestEvent(message.StockItemHeader));
      }
      else
        this.RemoveStickers(message.StockItemHeader.StockItem);
    }

    public void Handle(StickersPacksReorderedEvent message)
    {
      this.InsertStickers(message.NewIndex, message.StockItem);
    }

    public void Handle(StickersTapEvent message)
    {
      if (this.StickersList == null)
        return;
      long stickersProductId = message.StickersProductId;
      int stickerId = message.StickerId;
      if (stickersProductId == 0L && stickerId == 0)
        return;
      StockItem stockItem1 = stickersProductId > 0L ? this.StickersList.FirstOrDefault<StockItem>((Func<StockItem, bool>) (item => (long) item.product.id == stickersProductId)) : this.StickersList.FirstOrDefault<StockItem>((Func<StockItem, bool>) (item => item.product.stickers.sticker_ids.Contains(stickerId)));
      if (stockItem1 != null)
      {
        EventAggregator.Current.Publish((object) new StickersSettings.StickersItemTapEvent(new StockItemHeader(stockItem1, false)));
      }
      else
      {
        FullscreenLoader loader = new FullscreenLoader()
        {
          HideOnBackKeyPress = true
        };
        loader.Show(null, true);
        Action<StockItem> processLoadedStockItem = (Action<StockItem>) (stockItem => Execute.ExecuteOnUIThread((Action) (() =>
        {
          if (!loader.IsShowed)
            return;
          loader.Hide(false);
          if (stockItem == null || this.StickersList == null)
            return;
          if (stockItem.product.active == 1)
          {
            if (this.StickersList != null)
              this.InsertStickers(0, stockItem);
            EventAggregator.Current.Publish((object) new StickersSettings.StickersItemTapEvent(new StockItemHeader(stockItem, false)));
          }
          else
          {
            CurrentStickersPurchaseFunnelSource.Source = StickersPurchaseFunnelSource.message;
            StickersPackView.Show(stockItem, "message");
          }
        })));
        if (stickersProductId > 0L)
        {
          StoreService instance = StoreService.Instance;
          int num = 0;
          List<long> productIds = new List<long>();
          productIds.Add(stickersProductId);
          long purchaseForId = 0;
          Action<BackendResult<VKList<StockItem>, ResultCode>> callback = (Action<BackendResult<VKList<StockItem>, ResultCode>>) (result =>
          {
            Action<StockItem> action = processLoadedStockItem;
            VKList<StockItem> resultData = result.ResultData;
            StockItem stockItem2;
            if (resultData == null)
            {
              stockItem2 = null;
            }
            else
            {
              List<StockItem> items = resultData.items;
              stockItem2 = items != null ? items.FirstOrDefault<StockItem>() : (StockItem) null;
            }
            action(stockItem2);
          });
          instance.GetStockItems((StoreProductType) num, productIds, null, purchaseForId, callback);
        }
        else
          StoreService.Instance.GetStockItemByStickerId((long) stickerId, (Action<BackendResult<StockItem, ResultCode>>) (result => processLoadedStockItem(result.ResultData)), new CancellationToken?());
      }
    }

    public void Handle(StickerItemTapEvent message)
    {
      this.InsertRecentSticker(message.StickerItem.StickerId);
    }

    public class StickersListUpdatedEvent
    {
    }

    public class StickersKeyboardOpenRequestEvent
    {
        public StockItemHeader StockItemHeader { get; set; }

      public StickersKeyboardOpenRequestEvent(StockItemHeader stockItemHeader)
      {
        this.StockItemHeader = stockItemHeader;
      }
    }

    public class StickersItemTapEvent
    {
        public StockItemHeader StockItemHeader { get; set; }

      public StickersItemTapEvent(StockItemHeader stockItemHeader)
      {
        this.StockItemHeader = stockItemHeader;
      }
    }

    public class StickerRecentItemInsertedEvent
    {
    }
  }
}
