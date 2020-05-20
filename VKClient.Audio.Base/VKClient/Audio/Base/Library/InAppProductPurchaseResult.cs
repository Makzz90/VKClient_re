using System;

namespace VKClient.Audio.Base.Library
{
  public class InAppProductPurchaseResult
  {
    public InAppProductPurchaseStatus Status { get; set; }

    public string ReceiptXml { get; set; }

    public Guid TransactionId { get; set; }
  }
}
