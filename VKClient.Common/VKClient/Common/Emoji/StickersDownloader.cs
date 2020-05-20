using System.Collections.Generic;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Library;
using VKClient.Common.Framework;
using VKClient.Common.Utils;

namespace VKClient.Common.Emoji
{
  public class StickersDownloader
  {
    private static StickersDownloader _instance;

    public static StickersDownloader Instance
    {
      get
      {
        if (StickersDownloader._instance == null)
          StickersDownloader._instance = new StickersDownloader();
        return StickersDownloader._instance;
      }
    }

    private StickersDownloader()
    {
    }

    public void InitiateDownload(StoreProduct storeProduct)
    {
      List<int> stickerIds = storeProduct.stickers.sticker_ids;
      string baseUrl = storeProduct.stickers.base_url;
      List<RemoteLocalMapping> downloadList = new List<RemoteLocalMapping>();
      foreach (int stickerId in stickerIds)
      {
        downloadList.Add(new RemoteLocalMapping()
        {
          RemoteUri = baseUrl + (object) stickerId + "/256b.png",
          LocalPath = this.GetLocalPathForStickerId256(storeProduct, stickerId)
        });
        if (ScaleFactor.GetScaleFactor() == 100)
          downloadList.Add(new RemoteLocalMapping()
          {
            RemoteUri = baseUrl + (object) stickerId + "/128b.png",
            LocalPath = this.GetLocalPathForStickerId128(storeProduct, stickerId)
          });
      }
      downloadList.Add(new RemoteLocalMapping()
      {
        RemoteUri = storeProduct.base_url + "/background.png",
        LocalPath = storeProduct.id.ToString() + "background.png"
      });
      BatchDownloadManager.GetDownloadManager(storeProduct.id.ToString(), downloadList).Start();
    }

    public string GetLocalPathForStickerId512(StoreProduct storeProduct, int stickerId)
    {
      return this.GetLocalPathForStickerId512(storeProduct.stickers.base_url, stickerId);
    }

    public string GetLocalPathForStickerId256(StoreProduct storeProduct, int stickerId)
    {
      return this.GetLocalPathForStickerId256(storeProduct.stickers.base_url, stickerId);
    }

    public string GetLocalPathForStickerId128(StoreProduct storeProduct, int stickerId)
    {
      return this.GetLocalPathForStickerId128(storeProduct.stickers.base_url, stickerId);
    }

    public string GetLocalPathForStickerId128(string baseUri, int stickerId)
    {
      if (ScaleFactor.GetScaleFactor() != 100)
        return this.GetLocalPathForStickerId256(baseUri, stickerId);
      return baseUri + (object) stickerId + "/128b.png?" + VeryLowProfileImageLoader.REQUIRE_CACHING_KEY + "=True";
    }

    public string GetLocalPathForStickerId256(string baseUri, int stickerId)
    {
      return baseUri + (object) stickerId + "/256b.png?" + VeryLowProfileImageLoader.REQUIRE_CACHING_KEY + "=True";
    }

    public string GetLocalPathForStickerId512(string baseUri, int stickerId)
    {
      return baseUri + (object) stickerId + "/512b.png?" + VeryLowProfileImageLoader.REQUIRE_CACHING_KEY + "=True";
    }
  }
}
