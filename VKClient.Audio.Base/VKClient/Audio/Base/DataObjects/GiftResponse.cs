using System.Collections.Generic;
using VKClient.Common.Backend.DataObjects;

namespace VKClient.Audio.Base.DataObjects
{
  public class GiftResponse
  {
    public List<User> users { get; set; }

    public int balance { get; set; }
  }
}
