using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Extensions;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using Windows.ApplicationModel.Store;
using Windows.Foundation;

namespace VKClient.Audio.Base.Library
{
  public static class InAppPurchaseService
  {
    private static Dictionary<string, string> _votesPrices;
    private const string PRODUCT_ID_PREFIX = "windows.phone.votes.";

    public static async void LoadUnfulfilledConsumables(string productId, Action<List<InAppUnfulfilledProduct>> callback)
    {
        if (callback != null)
        {
            List<InAppUnfulfilledProduct> list = new List<InAppUnfulfilledProduct>();
            if (AppGlobalStateManager.Current.GlobalState.PaymentType == AccountPaymentType.money)
            {
                callback.Invoke(list);
            }
            else
            {
                try
                {
                    IEnumerator<UnfulfilledConsumable> var_2 = (await CurrentApp.GetUnfulfilledConsumablesAsync()).GetEnumerator();
                    try
                    {
                        while (var_2.MoveNext())
                        {
                            UnfulfilledConsumable var_3_BD = var_2.Current;
                            string var_4_C9 = InAppPurchaseService.ToServerMerchantProductId(var_3_BD.ProductId);
                            if (string.IsNullOrEmpty(productId) || !(var_4_C9 != productId))
                            {
                                list.Add(new InAppUnfulfilledProduct
                                {
                                    ProductId = var_4_C9,
                                    TransactionId = var_3_BD.TransactionId
                                });
                            }
                        }
                    }
                    finally
                    {
                        if (var_2 != null)
                        {
                            var_2.Dispose();
                        }
                    }
                }
                catch
                {
                }
                callback.Invoke(list);
            }
        }
    }


    public static void ReportConsumableProductFulfillment(InAppUnfulfilledProduct product, Action callback = null)
    {
        if (product == null)
        {
            return;
        }
        Execute.ExecuteOnUIThread(async delegate
        {
            try
            {
                await CurrentApp.ReportConsumableFulfillmentAsync(InAppPurchaseService.ToInAppProductId(product.ProductId), product.TransactionId);
            }
            catch
            {
            }
            Action expr_A2 = callback;
            if (expr_A2 != null)
            {
                expr_A2.Invoke();
            }
        });
    }

    public static async void LoadProductReceipt(string productId, Action<string> callback)
    {
        if (callback != null)
        {
            string text = "";
            try
            {
                productId = InAppPurchaseService.ToInAppProductId(productId);
                string text2 = await CurrentApp.GetProductReceiptAsync(productId);
                text = text2;
            }
            catch
            {
            }
            callback.Invoke(text);
        }
    }


    public static void RequestProductPurchase(string productId, Action<InAppProductPurchaseResult> callback)
    {
        if (callback == null)
        {
            return;
        }
        Execute.ExecuteOnUIThread(async delegate
        {
            InAppProductPurchaseResult inAppProductPurchaseResult = new InAppProductPurchaseResult
            {
                Status = InAppProductPurchaseStatus.Cancelled
            };
            try
            {
                productId = InAppPurchaseService.ToInAppProductId(productId);
                PurchaseResults purchaseResults = await CurrentApp.RequestProductPurchaseAsync(productId);
                if (purchaseResults != null)
                {
                    inAppProductPurchaseResult.ReceiptXml = purchaseResults.ReceiptXml;
                    inAppProductPurchaseResult.TransactionId = purchaseResults.TransactionId;
                    if (!string.IsNullOrEmpty(purchaseResults.ReceiptXml))
                    {
                        inAppProductPurchaseResult.Status = InAppProductPurchaseStatus.Purchased;
                    }
                }
            }
            catch
            {
                inAppProductPurchaseResult.Status = InAppProductPurchaseStatus.Error;
            }
            callback.Invoke(inAppProductPurchaseResult);
        });
    }


    public static async void LoadProductPrices(Action<Dictionary<string, string>> callback)
    {
        if (InAppPurchaseService._votesPrices != null)
        {
            if (callback != null)
            {
                callback.Invoke(InAppPurchaseService._votesPrices);
            }
        }
        else
        {
            try
            {
                InAppPurchaseService._votesPrices = new Dictionary<string, string>();
                ListingInformation listingInformation = await CurrentApp.LoadListingInformationAsync();
                IEnumerator<ProductListing> var_3 = listingInformation.ProductListings.Values.GetEnumerator();
                try
                {
                    while (var_3.MoveNext())
                    {
                        ProductListing var_4_B6 = var_3.Current;
                        string var_5_CE = string.Format("windows.phone.votes.{0}", var_4_B6.ProductId.ToLowerInvariant());
                        InAppPurchaseService._votesPrices[var_5_CE]= listingInformation.ProductListings[var_4_B6.ProductId].FormattedPrice;
                    }
                }
                finally
                {
                    if (var_3 != null)
                    {
                        var_3.Dispose();
                    }
                }
            }
            catch
            {
            }
            if (callback != null)
            {
                callback.Invoke(InAppPurchaseService._votesPrices);
            }
        }
    }


    private static string ToInAppProductId(string productId)
    {
      return productId.Substring("windows.phone.votes.".Length).Capitalize();
    }

    private static string ToServerMerchantProductId(string productId)
    {
      return string.Format("{0}{1}", "windows.phone.votes.", productId.ToLowerInvariant());
    }
  }
}
