using System.Collections.Generic;
using VKClient.Common.Backend.DataObjects;

namespace VKClient.Audio.Base.DataObjects
{
  public class LookupContactsResponse
  {
    public List<User> found { get; set; }

    public List<User> other { get; set; }
  }
}
