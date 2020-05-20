using System.Collections.Generic;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;

namespace VKClient.Audio.Base.DataObjects
{
  public class StoreBuyProductParams
  {
      public StoreProductType ProductType { get; private set; }

      public int ProductId { get; private set; }

    public int RandomId
    {
      get
      {
        AutorizationData authData = VKRequestsDispatcher.AuthData;
        string str = authData != null ? authData.access_token :  null;
        if (string.IsNullOrEmpty(str))
          return 0;
        return this.ProductId ^ str.GetHashCode();
      }
    }

    public List<long> UserIds { get; set; }

    public string Message { get; set; }

    public StoreProductPrivacy Privacy { get; set; }

    public string StickerReferrer { get; set; }

    public StoreBuyProductParams(StoreProductType productType, int productId)
    {
      this.ProductType = productType;
      this.ProductId = productId;
    }
  }
}
