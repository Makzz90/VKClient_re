using VKClient.Common.Backend.DataObjects;

namespace VKClient.Audio.Base.DataObjects
{
  public class MoneyTransferResponse
  {
    public MoneyTransfer transfer { get; set; }

    public User user { get; set; }
  }
}
