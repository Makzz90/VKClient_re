using System.Collections.Generic;
using VKClient.Common.Backend.DataObjects;

namespace VKClient.Audio.Base.DataObjects
{
  public class GiftsResponse
  {
    public VKList<GiftItemData> gifts { get; set; }

    public User user { get; set; }

    public List<User> users { get; set; }

    public List<Group> groups { get; set; }
  }
}
