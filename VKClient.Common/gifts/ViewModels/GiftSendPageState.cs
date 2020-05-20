using System.Collections.Generic;
using System.Runtime.Serialization;

namespace VKClient.Common.Gifts.ViewModels
{
  [DataContract]
  public class GiftSendPageState
  {
    [DataMember]
    public List<long> UserIds { get; set; }

    [DataMember]
    public string Message { get; set; }

    [DataMember]
    public bool AreNameAndTextPublic { get; set; }

    [DataMember]
    public string Description { get; set; }

    [DataMember]
    public string ImageUrl { get; set; }

    [DataMember]
    public int Price { get; set; }

    [DataMember]
    public int GiftsLeft { get; set; }
  }
}
