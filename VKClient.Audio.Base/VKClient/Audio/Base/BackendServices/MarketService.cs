using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Library;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Utils;

namespace VKClient.Audio.Base.BackendServices
{
  public class MarketService
  {
    private static MarketService _instance;

    public static MarketService Instance
    {
      get
      {
        return MarketService._instance ?? (MarketService._instance = new MarketService());
      }
    }

    public void GetFeed(long ownerId, int count, int offset, Action<BackendResult<MarketFeedResponse, ResultCode>> callback)
    {
      VKRequestsDispatcher.DispatchRequestToVK<MarketFeedResponse>("execute.getMarketFeed", new Dictionary<string, string>()
      {
        {
          "owner_id",
          ownerId.ToString()
        },
        {
          "count",
          count.ToString()
        },
        {
          "offset",
          offset.ToString()
        }
      }, callback,  null, false, true, new CancellationToken?(),  null);
    }

    public void GetProducts(long ownerId, int count, int offset, Action<BackendResult<VKList<Product>, ResultCode>> callback)
    {
      this.GetProducts(ownerId, 0, count, offset, callback);
    }

    public void GetProducts(long ownerId, long albumId, int count, int offset, Action<BackendResult<VKList<Product>, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>()
      {
        {
          "owner_id",
          ownerId.ToString()
        },
        {
          "count",
          count.ToString()
        },
        {
          "offset",
          offset.ToString()
        },
        {
          "extended",
          "1"
        }
      };
      if (albumId > 0L)
        parameters["album_id"] = albumId.ToString();
      VKRequestsDispatcher.DispatchRequestToVK<VKList<Product>>("market.get", parameters, callback,  null, false, true, new CancellationToken?(),  null);
    }

    public void GetAlbumTitleWithProducts(long ownerId, long albumId, int count, int offset, Action<BackendResult<MarketAlbum, ResultCode>> callback)
    {
      string str = string.Format("\r\n\r\nreturn\r\n{{\r\n    \"title\": API.market.getAlbumById({{ owner_id: {0}, album_ids: {1} }}).items[0].title,\r\n    \"products\": API.market.get({{ owner_id: {2}, count: {3}, offset: {4}, extended: 1, album_id: {5} }})\r\n}};", ownerId, albumId, ownerId, count, offset, albumId);
      string methodName = "execute";
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters.Add("code", str);
      Action<BackendResult<MarketAlbum, ResultCode>> callback1 = callback;
      // ISSUE: variable of the null type
      int num1 = 0;
      int num2 = 1;
      CancellationToken? cancellationToken = new CancellationToken?();
      // ISSUE: variable of the null type
      VKRequestsDispatcher.DispatchRequestToVK<MarketAlbum>(methodName, parameters, callback1, null, num1 != 0, num2 != 0, cancellationToken, null);
    }

    public void GetProduct(long ownerId, long productId, Action<BackendResult<VKList<Product>, ResultCode>> callback)
    {
      this.GetProductsByIds((IEnumerable<string>) new List<string>()
      {
        string.Format("{0}_{1}", ownerId, productId)
      }, callback);
    }

    public void GetProductsByIds(IEnumerable<string> productIds, Action<BackendResult<VKList<Product>, ResultCode>> callback)
    {
      VKRequestsDispatcher.DispatchRequestToVK<VKList<Product>>("market.getById", new Dictionary<string, string>()
      {
        {
          "item_ids",
          string.Join(",", productIds)
        },
        {
          "extended",
          "1"
        }
      }, callback,  null, false, true, new CancellationToken?(),  null);
    }

    public void GetProductData(long ownerId, long productId, Action<BackendResult<ProductData, ResultCode>> callback)
    {
      VKRequestsDispatcher.DispatchRequestToVK<ProductData>("execute.getProductData", new Dictionary<string, string>()
      {
        {
          "owner_id",
          ownerId.ToString()
        },
        {
          "product_id",
          productId.ToString()
        }
      }, callback,  null, false, true, new CancellationToken?(),  null);
    }

    public void GetAlbums(long ownerId, int count, int offset, Action<BackendResult<VKList<MarketAlbum>, ResultCode>> callback)
    {
      VKRequestsDispatcher.DispatchRequestToVK<VKList<MarketAlbum>>("market.getAlbums", new Dictionary<string, string>()
      {
        {
          "owner_id",
          ownerId.ToString()
        },
        {
          "count",
          count.ToString()
        },
        {
          "offset",
          offset.ToString()
        }
      }, callback,  null, false, true, new CancellationToken?(),  null);
    }

    public void Search(long ownerId, long albumId, SearchParams searchParams, string query, int count, int offset, Action<BackendResult<VKList<Product>, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>()
      {
        {
          "owner_id",
          ownerId.ToString()
        },
        {
          "album_id",
          albumId.ToString()
        },
        {
          "count",
          count.ToString()
        },
        {
          "offset",
          offset.ToString()
        },
        {
          "extended",
          "1"
        }
      };
      if (!string.IsNullOrWhiteSpace(query))
        parameters["q"] = query;
      if (searchParams != null)
      {
        long num1 = searchParams.GetValue<long>("price_from");
        long num2 = searchParams.GetValue<long>("price_to");
        if (num1 > 0L || num2 > 0L)
        {
          long num3;
          if (num1 > 0L)
          {
            Dictionary<string, string> dictionary = parameters;
            string index = "price_from";
            num3 = num1 * 100L;
            string str = num3.ToString();
            dictionary[index] = str;
          }
          if (num2 > 0L)
          {
            Dictionary<string, string> dictionary = parameters;
            string index = "price_to";
            num3 = num2 * 100L;
            string str = num3.ToString();
            dictionary[index] = str;
          }
        }
        int num4 = searchParams.GetValue<int>("sort");
        if (num4 > 0)
          parameters["sort"] = num4.ToString();
        bool flag = searchParams.GetValue<bool>("rev");
        parameters["rev"] = flag ? "1" : "0";
      }
      VKRequestsDispatcher.DispatchRequestToVK<VKList<Product>>("market.search", parameters, callback,  null, false, true, new CancellationToken?(),  null);
    }

    public void GetComments(long ownerId, long productId, int knownCount, int offset, int count, Action<BackendResult<ProductLikesCommentsData, ResultCode>> callback)
    {
      VKRequestsDispatcher.DispatchRequestToVK<ProductLikesCommentsData>("execute.getProductComments", new Dictionary<string, string>()
      {
        {
          "owner_id",
          ownerId.ToString()
        },
        {
          "product_id",
          productId.ToString()
        },
        {
          "count",
          count.ToString()
        },
        {
          "offset",
          offset.ToString()
        },
        {
          "known_count",
          knownCount.ToString()
        },
        {
          "func_v",
          "2"
        }
      }, callback, (Func<string, ProductLikesCommentsData>) (jsonStr =>
      {
        jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "users", false);
        jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "users2", false);
        jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "users3", false);
        jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "groups", false);
        jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "comments", true);
        jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "tags", false);
        jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "likesAllIds", false);
        ProductLikesCommentsData response = JsonConvert.DeserializeObject<GenericRoot<ProductLikesCommentsData>>(jsonStr).response;
        GroupsService.Current.AddCachedGroups((IEnumerable<Group>) response.groups);
        if (knownCount < 0)
          response.Comments.Reverse();
        response.users2.AddRange((IEnumerable<User>) response.users3);
        return response;
      }), false, true, new CancellationToken?(),  null);
    }

    public void CreateComment(long ownerId, long itemId, string message, List<string> attachmentIds, bool fromGroup, long replyToCommentId, Action<BackendResult<Comment, ResultCode>> callback, int sticker_id = 0, string stickerReferrer = "")
    {
      Dictionary<string, string> dictionary = new Dictionary<string, string>()
      {
        {
          "code",
          string.Format("\r\n\r\nvar new_comment_id = API.market.createComment({{\r\n    owner_id: {0},\r\n    item_id: {1},\r\n    message: \"{2}\",\r\n    from_group: {3},\r\n    sticker_id: {4},\r\n    reply_to_comment: {5},\r\n    attachments: \"{6}\",\r\n    sticker_referrer: \"{7}\"\r\n}});\r\n\r\nvar last_comments = API.market.getComments({{\r\n    owner_id: {8},\r\n    item_id: {9},\r\n    need_likes: 1,\r\n    count: 10,\r\n    sort: \"desc\",\r\n    preview_length: 0,\r\n    allow_group_comments: 1\r\n}}).items;\r\n\r\nvar i = last_comments.length - 1;\r\nwhile (i >= 0)\r\n{{\r\n    if (last_comments[i].id == new_comment_id)\r\n        return last_comments[i];\r\n\r\n    i = i - 1;\r\n}}\r\n\r\nreturn null;\r\n\r\n                ", ownerId, itemId, message.Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r"), (fromGroup ? "1" : "0"), sticker_id, replyToCommentId, attachmentIds.GetCommaSeparated(","), stickerReferrer, ownerId, itemId)
        }
      };
      string methodName = "execute";
      Dictionary<string, string> parameters = dictionary;
      Action<BackendResult<Comment, ResultCode>> callback1 = callback;
      int num1 = 0;
      int num2 = 1;
      CancellationToken? cancellationToken = new CancellationToken?();
      // ISSUE: variable of the null type
      
      VKRequestsDispatcher.DispatchRequestToVK<Comment>(methodName, parameters, callback1, (Func<string, Comment>) (jsonStr => JsonConvert.DeserializeObject<VKRequestsDispatcher.GenericRoot<Comment>>(jsonStr).response), num1 != 0, num2 != 0, cancellationToken, null);
    }

    public void EditComment(long ownerId, long commentId, string message, List<string> attachmentIds, Action<BackendResult<VKClient.Audio.Base.ResponseWithId, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>()
      {
        {
          "owner_id",
          ownerId.ToString()
        },
        {
          "comment_id",
          commentId.ToString()
        },
        {
          "message",
          message
        }
      };
      if (!attachmentIds.IsNullOrEmpty())
        parameters["attachments"] = attachmentIds.GetCommaSeparated(",");
      VKRequestsDispatcher.DispatchRequestToVK<VKClient.Audio.Base.ResponseWithId>("market.editComment", parameters, callback, (Func<string, VKClient.Audio.Base.ResponseWithId>) (jsonStr => new VKClient.Audio.Base.ResponseWithId()), false, true, new CancellationToken?(),  null);
    }

    public void DeleteComment(long ownerId, long commentId, Action<BackendResult<VKClient.Audio.Base.ResponseWithId, ResultCode>> callback)
    {
      VKRequestsDispatcher.DispatchRequestToVK<VKClient.Audio.Base.ResponseWithId>("market.deleteComment", new Dictionary<string, string>()
      {
        {
          "owner_id",
          ownerId.ToString()
        },
        {
          "comment_id",
          commentId.ToString()
        }
      }, callback, (Func<string, VKClient.Audio.Base.ResponseWithId>) (jsonStr => new VKClient.Audio.Base.ResponseWithId()), false, true, new CancellationToken?(),  null);
    }

    public void ReportComment(long ownerId, long commentId, ReportReason reportReason, Action<BackendResult<VKClient.Audio.Base.ResponseWithId, ResultCode>> callback)
    {
      VKRequestsDispatcher.DispatchRequestToVK<VKClient.Audio.Base.ResponseWithId>("market.reportComment", new Dictionary<string, string>()
      {
        {
          "owner_id",
          ownerId.ToString()
        },
        {
          "comment_id",
          commentId.ToString()
        },
        {
          "reason",
          ((int) reportReason).ToString()
        }
      }, callback, (Func<string, VKClient.Audio.Base.ResponseWithId>) (jsonStr => new VKClient.Audio.Base.ResponseWithId()), false, true, new CancellationToken?(),  null);
    }
  }
}
