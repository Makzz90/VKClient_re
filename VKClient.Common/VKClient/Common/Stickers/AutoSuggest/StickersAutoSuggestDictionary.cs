using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VKClient.Audio.Base.BackendServices;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Emoji;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Stickers.ViewModels;

namespace VKClient.Common.Stickers.AutoSuggest
{
  public class StickersAutoSuggestDictionary : IBinarySerializable, IHandle<UserIsLoggedOutEvent>, IHandle, IHandle<StickersPackPurchasedEvent>, IHandle<StickersUpdatedEvent>, IHandle<StickersPackActivatedDeactivatedEvent>, IHandle<StickersPacksReorderedEvent>, IHandle<AutoSuggestEnabledChangedEvent>
  {
    private static readonly int REFRESH_AFTER_HOURS = 12;
    private static readonly string AUTO_SUGGEST_FILE_NAME = "StickersAutoSuggestData";
    private static readonly int MAX_TEXT_LENGTH = 100;
    private static readonly int MAX_SUGGEST_STICKERS_COUNT = 20;
    private DateTime _lastLoadedDate = DateTime.MinValue;
    private Dictionary<string, StickerKeywordItem> _lookupDictionary = new Dictionary<string, StickerKeywordItem>();
    private static StickersAutoSuggestDictionary _instance;
    private StickersKeywordsData _keywordsData;
    private bool _isLoading;
    private bool _isLoaded;
    private bool _isRestoringState;

    public static StickersAutoSuggestDictionary Instance
    {
      get
      {
        if (StickersAutoSuggestDictionary._instance == null)
          StickersAutoSuggestDictionary._instance = new StickersAutoSuggestDictionary();
        return StickersAutoSuggestDictionary._instance;
      }
    }

    public StickersAutoSuggestDictionary()
    {
      EventAggregator.Current.Subscribe((object) this);
    }

    public IEnumerable<StickersAutoSuggestItem> GetAutoSuggestItemsFor(string text)
    {
      if (!AppGlobalStateManager.Current.GlobalState.StickersAutoSuggestEnabled || string.IsNullOrWhiteSpace(text) || (text.Length > StickersAutoSuggestDictionary.MAX_TEXT_LENGTH || this._isRestoringState) || !this._isLoaded)
        return Enumerable.Empty<StickersAutoSuggestItem>();
      List<StickersAutoSuggestItem> source = new List<StickersAutoSuggestItem>();
      StickerKeywordItem stickerKeywordItem;
      if (this._lookupDictionary.TryGetValue(text, out stickerKeywordItem))
      {
        foreach (int sortBasedOnRecent in this.SortBasedOnRecents(stickerKeywordItem.user_stickers))
        {
          StickerItemData stickerItemData = new StickerItemData();
          stickerItemData.StickerId = sortBasedOnRecent;
          string pathForStickerId128 = StickersDownloader.Instance.GetLocalPathForStickerId128(this._keywordsData.base_url, sortBasedOnRecent);
          stickerItemData.LocalPath = pathForStickerId128;
          string pathForStickerId256 = StickersDownloader.Instance.GetLocalPathForStickerId256(this._keywordsData.base_url, sortBasedOnRecent);
          stickerItemData.LocalPathBig = pathForStickerId256;
          string pathForStickerId512 = StickersDownloader.Instance.GetLocalPathForStickerId512(this._keywordsData.base_url, sortBasedOnRecent);
          stickerItemData.LocalPathExtraBig = pathForStickerId512;
          StickerItemData stickerData = stickerItemData;
          source.Add(new StickersAutoSuggestItem(stickerData, false));
        }
        foreach (int promotedSticker in stickerKeywordItem.promoted_stickers)
        {
          StickerItemData stickerItemData = new StickerItemData();
          stickerItemData.StickerId = promotedSticker;
          string pathForStickerId128 = StickersDownloader.Instance.GetLocalPathForStickerId128(this._keywordsData.base_url, promotedSticker);
          stickerItemData.LocalPath = pathForStickerId128;
          string pathForStickerId256 = StickersDownloader.Instance.GetLocalPathForStickerId256(this._keywordsData.base_url, promotedSticker);
          stickerItemData.LocalPathBig = pathForStickerId256;
          string pathForStickerId512 = StickersDownloader.Instance.GetLocalPathForStickerId512(this._keywordsData.base_url, promotedSticker);
          stickerItemData.LocalPathExtraBig = pathForStickerId512;
          StickerItemData stickerData = stickerItemData;
          source.Add(new StickersAutoSuggestItem(stickerData, true));
        }
      }
      return source.Take<StickersAutoSuggestItem>(StickersAutoSuggestDictionary.MAX_SUGGEST_STICKERS_COUNT);
    }

    private List<int> SortBasedOnRecents(List<int> user_stickers)
    {
      List<int> intList1 = new List<int>();
      if (user_stickers == null)
        return intList1;
      StickersSettings instance = StickersSettings.Instance;
      List<int> intList2;
      if (instance == null)
      {
        intList2 = (List<int>) null;
      }
      else
      {
        StoreStickers recentStickers = instance.RecentStickers;
        intList2 = recentStickers != null ? recentStickers.sticker_ids : (List<int>) null;
      }
      List<int> intList3 = intList2;
      if (intList3 == null)
        return user_stickers;
      foreach (int num in intList3)
      {
        if (user_stickers.Contains(num))
          intList1.Add(num);
      }
      foreach (int userSticker in user_stickers)
      {
        if (!intList1.Contains(userSticker))
          intList1.Add(userSticker);
      }
      return intList1;
    }

    public string PrepareTextForLookup(string text)
    {
      if (text.EndsWith("  "))
        return "";
      text = text.ToLowerInvariant();
      text = text.Replace('ё', 'е');
      text = text.Replace(" ", "");
      return text;
    }

    public void EnsureDictIsLoadedAndUpToDate(bool forceReload = false)
    {
      if ((AppGlobalStateManager.Current.LoggedInUserId == 0L || this._isLoading ? 0 : (forceReload ? 1 : ((DateTime.UtcNow - this._lastLoadedDate).TotalHours >= (double) StickersAutoSuggestDictionary.REFRESH_AFTER_HOURS ? 1 : 0))) == 0)
        return;
      this._isLoading = true;
      StoreService.Instance.GetStickersKeywords((Action<BackendResult<StickersKeywordsData, ResultCode>>) (res =>
      {
        this._isLoading = false;
        if (res.ResultCode == ResultCode.Succeeded)
        {
          this.SetKeywordsData(res.ResultData);
          this._lastLoadedDate = DateTime.UtcNow;
          this._isLoaded = true;
          EventAggregator.Current.Publish((object) new StickersAutoSuggestDictionary.AutoSuggestDictionaryUpdatedEvent());
        }
        else
        {
          if (!forceReload)
            return;
          this._lastLoadedDate = DateTime.MinValue;
        }
      }));
    }

    private void SetKeywordsData(StickersKeywordsData keywordsData)
    {
      this._keywordsData = keywordsData;
      this.BuildDictionary();
    }

    private void BuildDictionary()
    {
      if (this._keywordsData == null)
        return;
      this._lookupDictionary.Clear();
      foreach (StickerKeywordItem stickerKeywordItem in this._keywordsData.dictionary)
      {
        foreach (string word in stickerKeywordItem.words)
        {
          string key = this.PrepareTextForLookup(word);
          if (!this._lookupDictionary.ContainsKey(key))
            this._lookupDictionary.Add(key, stickerKeywordItem);
        }
      }
    }

    public void SaveState()
    {
      CacheManager.TrySerialize((IBinarySerializable) this, StickersAutoSuggestDictionary.AUTO_SUGGEST_FILE_NAME, false, CacheManager.DataType.CachedData);
    }

    public async void RestoreStateAsync()
    {
      this._isRestoringState = true;
      int num = await CacheManager.TryDeserializeAsync((IBinarySerializable) this, StickersAutoSuggestDictionary.AUTO_SUGGEST_FILE_NAME, CacheManager.DataType.CachedData) ? 1 : 0;
      this._isRestoringState = false;
    }

    public void Write(BinaryWriter writer)
    {
      writer.Write(1);
      writer.Write(this._isLoaded);
      writer.Write(this._lastLoadedDate);
      writer.Write<StickersKeywordsData>(this._keywordsData, false);
    }

    public void Read(BinaryReader reader)
    {
      reader.ReadInt32();
      this._isLoaded = reader.ReadBoolean();
      this._lastLoadedDate = reader.ReadDateTime();
      this.SetKeywordsData(reader.ReadGeneric<StickersKeywordsData>());
    }

    public void Handle(UserIsLoggedOutEvent message)
    {
      this.ResetInstance();
    }

    private void ResetInstance()
    {
      StickersAutoSuggestDictionary._instance = new StickersAutoSuggestDictionary();
    }

    public void Handle(StickersPackPurchasedEvent message)
    {
      this.RefreshDictIfNeeded();
    }

    public void Handle(StickersUpdatedEvent message)
    {
      this.RefreshDictIfNeeded();
    }

    public void Handle(StickersPackActivatedDeactivatedEvent message)
    {
      this.RequestDeferredUpdate();
    }

    public void Handle(StickersPacksReorderedEvent message)
    {
      this.RequestDeferredUpdate();
    }

    public void Handle(AutoSuggestEnabledChangedEvent message)
    {
      this.RequestDeferredUpdate();
    }

    private void RefreshDictIfNeeded()
    {
      if (StickersAutoSuggestDictionary._instance != this)
        return;
      this.EnsureDictIsLoadedAndUpToDate(true);
    }

    private void RequestDeferredUpdate()
    {
      if (StickersAutoSuggestDictionary._instance != this)
        return;
      this._lastLoadedDate = DateTime.MinValue;
    }

    public class AutoSuggestDictionaryUpdatedEvent
    {
    }
  }
}
