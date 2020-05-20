using System;
using System.Collections.Generic;
using System.Linq;
using VKClient.Audio.Base.DataObjects;

namespace VKClient.Audio.Base.Library
{
  public static class VotesPacksGenerator
  {
    public static void Generate(AccountPaymentType paymentType, List<StockItem> stockItems, Action<List<VotesPack>> callback)
    {
      if (paymentType == AccountPaymentType.inapp)
        InAppPurchaseService.LoadProductPrices((Action<Dictionary<string, string>>) (votesPrices =>
        {
          if (votesPrices == null)
            return;
          VotesPacksGenerator.CreateVotesPacks((IEnumerable<StockItem>) stockItems, (IDictionary<string, string>) votesPrices, callback);
        }));
      else
        VotesPacksGenerator.CreateVotesPacks((IEnumerable<StockItem>) stockItems,  null, callback);
    }

    private static void CreateVotesPacks(IEnumerable<StockItem> stockItems, IDictionary<string, string> inAppPrices = null, Action<List<VotesPack>> callback = null)
    {
      List<VotesPack> votesPackList = new List<VotesPack>();
      foreach (StockItem stockItem in stockItems.Where<StockItem>((Func<StockItem, bool>) (stockItem => stockItem.product != null)))
      {
        StoreProduct product = stockItem.product;
        int votes = product.votes;
        string merchantProductId = stockItem.merchant_product_id;
        VotesPack votesPack = new VotesPack()
        {
          ProductId = product.id,
          MerchantProductId = merchantProductId,
          Title = product.title,
          VotesCount = votes
        };
        if (inAppPrices != null && inAppPrices.ContainsKey(merchantProductId))
        {
          votesPack.PaymentType = AccountPaymentType.inapp;
          votesPack.PriceStr = inAppPrices[merchantProductId];
        }
        else
        {
          votesPack.PaymentType = AccountPaymentType.money;
          votesPack.PriceStr = stockItem.price_str;
        }
        votesPackList.Add(votesPack);
      }
      if (callback == null)
        return;
      callback(votesPackList);
    }
  }
}
