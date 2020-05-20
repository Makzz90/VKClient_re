using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using VKClient.Audio.Base.BackendServices;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.UC;

namespace VKClient.Common.Gifts.ViewModels
{
  public class GiftsSectionItemHeader
  {
    private readonly GiftsSectionItem _section;
    private readonly string _categoryName;
    private readonly long _userOrChatId;
    private readonly bool _isChat;
    private const string STICKERS_CATEGORY_NAME = "stickers";

    public long GiftId
    {
      get
      {
        Gift gift = this._section.gift;
        if (gift == null)
          return 0;
        return gift.id;
      }
    }

    public string Description
    {
      get
      {
        return this._section.description;
      }
    }

    public int GiftsLeft
    {
      get
      {
        return this._section.gifts_left;
      }
    }

    public int Price
    {
      get
      {
        return this._section.price;
      }
    }

    public string ImageUrl
    {
      get
      {
        Gift gift = this._section.gift;
        if (gift == null)
          return  null;
        return gift.thumb_256;
      }
    }

    public string PriceStr
    {
      get
      {
        if (this.GiftsLeft <= 0)
          return this._section.price_str;
        return string.Format(CommonResources.GiftsForFree, this.GiftsLeft);
      }
    }

    public SolidColorBrush PriceForeground
    {
      get
      {
        if (this.GiftsLeft <= 0)
          return (SolidColorBrush) Application.Current.Resources["PhoneGray400_Gray500Brush"];
        return (SolidColorBrush) Application.Current.Resources["PhoneBlue300_GrayBlue100Brush"];
      }
    }

    private bool IsProduct
    {
      get
      {
        Gift gift = this._section.gift;
        if (gift == null)
          return true;
        return (ulong) gift.stickers_product_id > 0UL;
      }
    }

    public GiftsSectionItemHeader(GiftsSectionItem section, string categoryName, long userOrChatId = 0, bool isChat = false)
    {
      this._section = section;
      this._categoryName = categoryName;
      this._userOrChatId = userOrChatId;
      this._isChat = isChat;
    }

    public void NavigateToGiftSend()
    {
      if (!this._isChat)
      {
        INavigator current = Navigator.Current;
        long giftId = this.GiftId;
        string categoryName = this._categoryName;
        string description = this.Description;
        string imageUrl = this.ImageUrl;
        int price = this.Price;
        int giftsLeft = this.GiftsLeft;
        List<long> userIds = new List<long>();
        userIds.Add(this._userOrChatId);
        int num = this.IsProduct ? 1 : 0;
        current.NavigateToGiftSend(giftId, categoryName, description, imageUrl, price, giftsLeft, userIds, num != 0);
      }
      else if (this._categoryName == "stickers")
        this.LoadUserIds((Action<long, Action<BackendResult<List<long>, ResultCode>>, CancellationToken>) ((chatId, result, cancellationToken) => GiftsService.Instance.GetChatUsersForProduct(chatId, -this.GiftId, result, new CancellationToken?(cancellationToken))));
      else
        this.LoadUserIds((Action<long, Action<BackendResult<List<long>, ResultCode>>, CancellationToken>) ((chatId, result, cancellationToken) => GiftsService.Instance.GetChatUsers(chatId, result, new CancellationToken?(cancellationToken))));
    }

    private void LoadUserIds(Action<long, Action<BackendResult<List<long>, ResultCode>>, CancellationToken> loadAction)
    {
      CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
      FullscreenLoader fullscreenLoader = new FullscreenLoader();
      fullscreenLoader.HideOnBackKeyPress = true;
      Action<FullscreenLoaderHiddenEventArgs> action = (Action<FullscreenLoaderHiddenEventArgs>) (args => cancellationTokenSource.Cancel());
      fullscreenLoader.HiddenCallback = action;
      FullscreenLoader loader = fullscreenLoader;
      loader.Show( null, true);
      loadAction(this._userOrChatId, (Action<BackendResult<List<long>, ResultCode>>) (result =>
      {
        loader.Hide(false);
        if (result.ResultCode == ResultCode.Succeeded)
        {
          List<long> resultData = result.ResultData;
          if (resultData == null || resultData.Count == 0)
          {
            if (this._categoryName == "stickers")
              Execute.ExecuteOnUIThread((Action) (() => MessageBox.Show(CommonResources.AllChatParticipantsHaveStickerPack, CommonResources.StickerPack, (MessageBoxButton) 0)));
            else
              new GenericInfoUC().ShowAndHideLater(CommonResources.Error,  null);
          }
          else
            Navigator.Current.NavigateToGiftSend(this.GiftId, this._categoryName, this.Description, this.ImageUrl, this.Price, this.GiftsLeft, resultData, this.IsProduct);
        }
        else
          GenericInfoUC.ShowBasedOnResult((int) result.ResultCode, "", (VKRequestsDispatcher.Error) null);
      }), cancellationTokenSource.Token);
    }
  }
}
