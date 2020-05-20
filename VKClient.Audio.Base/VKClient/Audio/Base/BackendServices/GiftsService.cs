using System;
using System.Collections.Generic;
using System.Threading;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;

namespace VKClient.Audio.Base.BackendServices
{
  public class GiftsService
  {
    private static GiftsService _instance;

    public static GiftsService Instance
    {
      get
      {
        return GiftsService._instance ?? (GiftsService._instance = new GiftsService());
      }
    }

    public void GetCatalog(long userId, Action<BackendResult<GiftsCatalogResponse, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      if (userId > 0L)
        parameters["user_id"] = userId.ToString();
      VKRequestsDispatcher.DispatchRequestToVK<GiftsCatalogResponse>("execute.getGiftsCatalog", parameters, callback,  null, false, true, new CancellationToken?(),  null);
    }

    public void GetCatalog(long userId, string categoryName, Action<BackendResult<List<GiftsSection>, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      if (userId > 0L)
        parameters["user_id"] = userId.ToString();
      if (!string.IsNullOrEmpty(categoryName))
        parameters["filters"] = categoryName;
      VKRequestsDispatcher.DispatchRequestToVK<List<GiftsSection>>("gifts.getCatalog", parameters, callback,  null, false, true, new CancellationToken?(),  null);
    }

    public void Get(long userId, int count, int offset, Action<BackendResult<GiftsResponse, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      string index1 = "user_id";
      string str1 = userId.ToString();
      parameters[index1] = str1;
      string index2 = "count";
      string str2 = count.ToString();
      parameters[index2] = str2;
      string index3 = "offset";
      string str3 = offset.ToString();
      parameters[index3] = str3;
      string index4 = "fields";
      string str4 = "photo_max,can_see_gifts,first_name_gen";
      parameters[index4] = str4;
      VKRequestsDispatcher.DispatchRequestToVK<GiftsResponse>("execute.getGifts", parameters, callback,  null, false, true, new CancellationToken?(),  null);
    }

    public void GetGiftInfo(List<long> userIds, Action<BackendResult<GiftResponse, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      string index = "user_ids";
      string str = string.Join<long>(",", (IEnumerable<long>) userIds);
      parameters[index] = str;
      VKRequestsDispatcher.DispatchRequestToVK<GiftResponse>("execute.getGiftInfo", parameters, callback,  null, false, true, new CancellationToken?(),  null);
    }

    public void GetGiftInfoFromStore(long productId, long userOrChatId, bool isChat, Action<BackendResult<GiftInfoFromStoreResponse, ResultCode>> callback, CancellationToken? cancellationToken = null)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      string index1 = "product_id";
      string str1 = productId.ToString();
      parameters[index1] = str1;
      string index2 = "user_or_chat_id";
      string str2 = userOrChatId.ToString();
      parameters[index2] = str2;
      string index3 = "is_chat";
      string str3 = (isChat ? 1 : 0).ToString();
      parameters[index3] = str3;
      VKRequestsDispatcher.DispatchRequestToVK<GiftInfoFromStoreResponse>("execute.getGiftInfoFromStore", parameters, callback,  null, false, true, cancellationToken,  null);
    }

    public void Send(List<long> userIds, long giftId, int guid, string message, GiftPrivacy privacy, string section = "", Action<BackendResult<VKList<GiftSentResponse>, ResultCode>> callback = null)
    {
      Dictionary<string, string> dictionary = new Dictionary<string, string>();
      string index1 = "user_ids";
      string str1 = string.Join<long>(",", (IEnumerable<long>) userIds);
      dictionary[index1] = str1;
      string index2 = "gift_id";
      string str2 = giftId.ToString();
      dictionary[index2] = str2;
      string index3 = "guid";
      string str3 = guid.ToString();
      dictionary[index3] = str3;
      string index4 = "privacy";
      string str4 = ((int) privacy).ToString();
      dictionary[index4] = str4;
      string index5 = "no_inapp";
      string str5 = "1";
      dictionary[index5] = str5;
      Dictionary<string, string> parameters = dictionary;
      if (!string.IsNullOrWhiteSpace(message))
        parameters["message"] = message;
      if (!string.IsNullOrWhiteSpace(section))
        parameters["section"] = section;
      VKRequestsDispatcher.DispatchRequestToVK<VKList<GiftSentResponse>>("gifts.send", parameters, callback,  null, false, true, new CancellationToken?(), (Action) (() => EventAggregator.Current.Publish(new GiftsPurchaseStepsEvent(GiftPurchaseStepsSource.gift_page, GiftPurchaseStepsAction.purchase_window))));
    }

    public void Delete(long id, string giftHash, Action<BackendResult<int, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      string index1 = "id";
      string str1 = id.ToString();
      parameters[index1] = str1;
      string index2 = "gift_hash";
      string str2 = giftHash;
      parameters[index2] = str2;
      VKRequestsDispatcher.DispatchRequestToVK<int>("gifts.delete", parameters, callback,  null, false, true, new CancellationToken?(),  null);
    }

    public void GetChatUsers(long chatId, Action<BackendResult<List<long>, ResultCode>> callback, CancellationToken? cancellationToken = null)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      string index = "chat_id";
      string str = chatId.ToString();
      parameters[index] = str;
      VKRequestsDispatcher.DispatchRequestToVK<List<long>>("execute.getGiftChatUsers", parameters, callback,  null, false, true, cancellationToken,  null);
    }

    public void GetChatUsersForProduct(long chatId, long productId, Action<BackendResult<List<long>, ResultCode>> callback, CancellationToken? cancellationToken = null)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      string index1 = "chat_id";
      string str1 = chatId.ToString();
      parameters[index1] = str1;
      string index2 = "product_id";
      string str2 = productId.ToString();
      parameters[index2] = str2;
      VKRequestsDispatcher.DispatchRequestToVK<List<long>>("execute.getGiftChatUsersForProduct", parameters, callback,  null, false, true, cancellationToken,  null);
    }
  }
}
